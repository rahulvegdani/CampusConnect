using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace CampusConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= ADMIN DASHBOARD =================
        public IActionResult Dashboard()
        {
            // ✅ Get Admin Role Id
            var adminRoleId = _context.Roles
                .First(r => r.Name == "Admin").Id;

            // ✅ TOTAL USERS (excluding admin)
            ViewBag.TotalUsers = _context.Users
                .Where(u => !_context.UserRoles
                    .Any(r => r.UserId == u.Id && r.RoleId == adminRoleId))
                .Count();

            // ✅ PRODUCT STATS
            ViewBag.TotalProducts = _context.Products.Count();

            ViewBag.ApprovedProducts =
                _context.Products.Count(p => p.IsApproved);

            ViewBag.RejectedProducts =
                _context.Products.Count(p => p.IsRejected);

            ViewBag.PendingProducts =
                _context.Products.Count(p => !p.IsApproved && !p.IsRejected);

            ViewBag.SoldProducts =
                _context.Products.Count(p => p.IsSold);

            // ✅ 🔥 DYNAMIC CATEGORY STATS (FIXED)
            var categoryStats = _context.Products
                .Include(p => p.Category)
                .GroupBy(p => p.Category.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.CategoryNames = categoryStats.Select(c => c.Category).ToList();
            ViewBag.CategoryCounts = categoryStats.Select(c => c.Count).ToList();

            // ✅ REAL USER GROWTH
            var users = _context.Users.ToList();

            var monthlyUsers = users
                .GroupBy(u => u.CreatedAt.Month)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    Count = g.Count()
                })
                .ToList();

            ViewBag.UserMonths = monthlyUsers.Select(x => x.Month).ToList();
            ViewBag.UserCounts = monthlyUsers.Select(x => x.Count).ToList();

            // ✅ 🔔 NOTIFICATION COUNT
            ViewBag.PendingCount = ViewBag.PendingProducts;

            return View();
        }

        // ================= ADMIN USER LIST =================
        public IActionResult Users()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // ================= ADMIN USER DETAILS =================
        public IActionResult UserDetails(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            var profile = _context.UserProfiles
                .Include(p => p.College)   // 🔥 THIS LINE IS MISSING
                .FirstOrDefault(p => p.UserId == id);

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.UserId == id)
                .ToList();

            var soldProducts = products.Where(p => p.IsSold).ToList();

            var boughtProducts = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.BuyerId == id)
                .ToList();

            var reviews = _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Product)
                .Where(r => r.SellerId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var model = new
            {
                Email = user?.Email,
                Phone = profile?.PhoneNumber,
                College = profile?.College?.Name,

                TotalProducts = products.Count,
                ApprovedProducts = products.Count(p => p.IsApproved),
                RejectedProducts = products.Count(p => p.IsRejected),
                PendingProducts = products.Count(p => !p.IsApproved && !p.IsRejected),
                SoldProducts = soldProducts.Count,

                Rating = 5, // keep your logic

                UploadedList = products,
                SoldList = soldProducts,
                BoughtList = boughtProducts,

                Reviews = reviews
            };

            return View(model);
        }

        // ================= ADMIN USER PROFILES =================
        public IActionResult UserProfiles()
        {
            var profiles = _context.UserProfiles.ToList();
            return View(profiles);
        }

        // =========== Restore User =========
        [HttpPost]
        public async Task<IActionResult> RestoreUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            // ✅ RESTORE USER
            user.IsDeleted = false;
            user.DeletedAt = null;

            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }
    }

}
