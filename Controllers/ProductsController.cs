using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusConnect.Data;
using CampusConnect.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CampusConnect.Hubs;

namespace CampusConnect.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<ChatHub> _hub; // 🔥 NEW

        public ProductsController(ApplicationDbContext context,
                                  IWebHostEnvironment environment,
                                  IHubContext<ChatHub> hub) // 🔥 NEW
        {
            _context = context;
            _environment = environment;
            _hub = hub;
        }

        // ================= MARKETPLACE =================
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.User.IsDeleted == false)
                .Where(p => p.IsApproved == true && p.IsSold == false);

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            return View(await products.ToListAsync());
        }

        // ================= MY PRODUCTS =================
        public async Task<IActionResult> MyProducts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // 🔥 ADD THIS BLOCK (IMPORTANT)
            var productInterest = new Dictionary<int, List<string>>();

            foreach (var product in products)
            {
                var users = _context.ChatMessages
                    .Where(m => m.ProductId == product.Id &&
                                (m.SenderId != userId)) // exclude self
                    .Select(m => m.SenderId)
                    .Distinct()
                    .ToList();

                var userEmails = _context.Users
                    .Where(u => users.Contains(u.Id))
                    .Select(u => u.Email)
                    .ToList();

                productInterest[product.Id] = userEmails;
            }

            ViewBag.ProductInterest = productInterest;

            return View(products);
        }

        // ================= MARK PRODUCT AS SOLD =================
        [HttpPost]
        public async Task<IActionResult> MarkSold(int productId, string buyerUserId)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null) return NotFound();

            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 🔒 OWNER CHECK
            if (product.UserId != currentUser)
                return Unauthorized();

            // ❌ BLOCK IF NOT APPROVED
            if (!product.IsApproved)
            {
                TempData["Error"] = "Your product is pending approval. You can sell only after approval.";
                return RedirectToAction(nameof(MyProducts));
            }

            // ❌ BLOCK IF ALREADY SOLD
            if (product.IsSold)
            {
                TempData["Error"] = "Product is already sold!";
                return RedirectToAction(nameof(MyProducts));
            }

            // ✅ MARK SOLD
            product.IsSold = true;
            product.BuyerId = buyerUserId;
            product.SoldAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // 🔥 GET WISHLIST USERS
            var wishlistUsers = _context.Wishlists
                .Where(w => w.ProductId == productId)
                .Select(w => w.UserId)
                .Distinct()
                .ToList();

            var productName = product.Name;

            foreach (var userId in wishlistUsers)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = $"🔥 {productName} has been sold!",
                    Type = "Wishlist",
                    RedirectUrl = $"/Products/Details/{productId}",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                _context.Notifications.Add(notification);

                // 🔔 REAL-TIME PUSH
                await _hub.Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    title = notification.Title,
                    url = notification.RedirectUrl,
                    time = notification.CreatedAt.ToString("hh:mm tt")
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product marked as sold successfully!";

            return RedirectToAction(nameof(MyProducts));
        }

        // ================= GET CHAT USERS =================
        public IActionResult GetChatUsers(int productId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = _context.ChatMessages
                .Where(m => m.ProductId == productId &&
                    (m.SenderId == currentUser || m.ReceiverId == currentUser))
                .Select(m => m.SenderId == currentUser ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToList();

            var result = _context.Users
                .Where(u => users.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToList();

            return Json(result);
        }

        // ================= ADMIN SOLD PRODUCTS =================
        [Authorize(Roles = "Admin")]
        public IActionResult SoldProducts()
        {
            var soldProducts = _context.Products
                .Where(p => p.IsSold)
                .Include(p => p.Category)
                .Include(p => p.User)
                .ToList()
                .Select(p => new
                {
                    p.Name,
                    Category = p.Category.Name,
                    p.Price,
                    Seller = p.User.UserName,
                    Buyer = _context.Users
                        .Where(u => u.Id == p.BuyerId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    SoldAt = p.SoldAt
                })
                .ToList();

            return View(soldProducts);
        }

        // =================  REJECTED PRODUCTS =================
        public IActionResult RejectedProducts()
        {
            var rejectedProducts = _context.Products
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => !p.IsApproved)
                .ToList();

            return View(rejectedProducts);
        }

        // ================= ADMIN PRODUCTS =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminProducts(string searchString, string sortOrder)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .AsQueryable(); // ✅ NO FILTER → get ALL products

            // 🔍 SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchString) ||
                    p.User.Email.Contains(searchString) ||
                    p.Category.Name.Contains(searchString));
            }

            // 🔃 SORT
            switch (sortOrder)
            {
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                default:
                    products = products.OrderByDescending(p => p.CreatedAt); // 🔥 latest first
                    break;
            }

            return View(await products.ToListAsync());
        }

        // ================= APPROVAL =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => !p.IsApproved && !p.IsRejected)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                product.IsApproved = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ApproveProducts));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                product.IsRejected = true;
                product.IsApproved = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ApproveProducts));
        }

        // ================= DETAILS =================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            ViewBag.ProductReviews = _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            product.Views += 1;
            await _context.SaveChangesAsync();

            return View(product);
        }

        // ================= ADMIN PRODUCT DETAILS =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            product.UserId = userId;
            product.IsApproved = false;
            product.IsSold = false;

            if (product.ImageFile != null)
            {
                string folder = "productImages";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, folder);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + "_" + product.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                product.ImagePath = "/" + folder + "/" + fileName;
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyProducts));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ================= EDIT =================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            // 🔒 SECURITY: Only owner can edit
            if (product.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            // ❌ OPTIONAL: Block editing if sold
            if (product.IsSold)
            {
                TempData["Error"] = "Sold product cannot be edited!";
                return RedirectToAction(nameof(MyProducts));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null) return NotFound();

            // 🔒 SECURITY
            if (existingProduct.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            if (existingProduct.IsSold)
            {
                TempData["Error"] = "Sold product cannot be edited!";
                return RedirectToAction(nameof(MyProducts));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(MyProducts));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ================= DELETE =================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            if (product.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                if (product.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                    return Unauthorized();

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyProducts));
        }
    }
}