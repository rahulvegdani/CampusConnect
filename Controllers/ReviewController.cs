using Microsoft.AspNetCore.Mvc;
using CampusConnect.Data;
using CampusConnect.Models;
using System.Security.Claims;

namespace CampusConnect.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= CREATE PAGE =================
        public IActionResult Create(int productId)
        {
            ViewBag.ProductId = productId;
            return View();
        }

        // ================= SAVE REVIEW =================
        [HttpPost]
        public IActionResult Create(int productId, int rating, string? comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                return NotFound();

            // 🔥 STEP 1: ONLY BUYER CAN REVIEW
            if (product.BuyerId != userId)
            {
                TempData["Error"] = "Only buyer can review this product.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // 🔥 STEP 2: PREVENT MULTIPLE REVIEWS
            var alreadyReviewed = _context.Reviews
                .Any(r => r.ProductId == productId && r.ReviewerId == userId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You have already reviewed this product.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // 🔥 STEP 3: VALIDATE RATING
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Invalid rating.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (string.IsNullOrEmpty(product.UserId))
            {
                TempData["Error"] = "Seller not found.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // 🔥 STEP 4: SAVE REVIEW
            var review = new Review
            {
                ProductId = productId,
                ReviewerId = userId,
                SellerId = product.UserId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            // 🔥 REDIRECT BACK TO PRODUCT (BETTER UX)
            return RedirectToAction("Details", "Products", new { id = productId });
        }
    }
}