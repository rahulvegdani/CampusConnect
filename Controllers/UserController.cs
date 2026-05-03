using Microsoft.AspNetCore.Mvc;
using CampusConnect.Data;
using System.Linq;

namespace CampusConnect.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= USER PROFILE =================
        public IActionResult Profile(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            var profile = _context.UserProfiles
                .FirstOrDefault(p => p.UserId == id);

            // ✅ VERIFIED LOGIC
            bool isVerified = profile != null &&
                              !string.IsNullOrEmpty(profile.FirstName) &&
                              !string.IsNullOrEmpty(profile.LastName) &&
                              !string.IsNullOrEmpty(profile.PhoneNumber) &&
                              !string.IsNullOrEmpty(profile.ProfileImagePath) &&
                              !string.IsNullOrEmpty(profile.College?.Name);

            // 📊 PROFILE COMPLETION
            int completion = 0;

            if (!string.IsNullOrEmpty(profile?.FirstName)) completion += 20;
            if (!string.IsNullOrEmpty(profile?.LastName)) completion += 20;
            if (!string.IsNullOrEmpty(profile?.PhoneNumber)) completion += 20;
            if (!string.IsNullOrEmpty(profile?.ProfileImagePath)) completion += 20;
            if (!string.IsNullOrEmpty(profile?.College?.Name)) completion += 20;

            // 📦 SOLD ITEMS
            var sold = _context.Products
                .Where(p => p.UserId == id && p.IsSold)
                .ToList();

            // 🛒 BOUGHT ITEMS
            var bought = _context.Products
                .Where(p => p.BuyerId == id && p.IsSold)
                .ToList();

            ViewBag.User = user;
            ViewBag.Profile = profile;
            ViewBag.IsVerified = isVerified;
            ViewBag.Completion = completion;
            ViewBag.Sold = sold;
            ViewBag.Bought = bought;

            var avgRating = _context.Reviews
                .Where(r => r.SellerId == id)
                .Select(r => (double?)r.Rating)
                .Average() ?? 0;

            ViewBag.AvgRating = Math.Round(avgRating, 1);

            return View();
        }
    }
}