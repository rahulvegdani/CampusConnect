using CampusConnect.Data;
using CampusConnect.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampusConnect.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ================= CREATE / EDIT =================
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = _context.UserProfiles
                .Include(p => p.College) // ✅ IMPORTANT
                .FirstOrDefault(p => p.UserId == userId);

            return View(profile ?? new UserProfile());
        }

        // ================= SAVE PROFILE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserProfile profile, IFormFile? ProfileImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingProfile = _context.UserProfiles
                .FirstOrDefault(p => p.UserId == userId);

            string? imagePath = null;

            // ✅ IMAGE SAVE
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                string folderPath = Path.Combine(_environment.WebRootPath, "profileImages");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);
                string fullPath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                imagePath = "/profileImages/" + fileName;

                // delete old image
                if (existingProfile != null && !string.IsNullOrEmpty(existingProfile.ProfileImagePath))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath,
                        existingProfile.ProfileImagePath.TrimStart('/'));

                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
            }

            // ✅ UPDATE PROFILE
            if (existingProfile != null)
            {
                existingProfile.FirstName = profile.FirstName;
                existingProfile.LastName = profile.LastName;
                existingProfile.DateOfBirth = profile.DateOfBirth;
                existingProfile.PhoneNumber = profile.PhoneNumber;

                // 🔥 MAIN FIX
                existingProfile.CollegeId = profile.CollegeId;

                if (imagePath != null)
                    existingProfile.ProfileImagePath = imagePath;

                _context.Update(existingProfile);
            }
            else
            {
                profile.UserId = userId;
                profile.ProfileImagePath = imagePath;

                // 🔥 MAIN FIX
                profile.CollegeId = profile.CollegeId;

                _context.UserProfiles.Add(profile);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("MyProfile");
        }

        // ================= MY PROFILE =================
        public IActionResult MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = _context.UserProfiles
                .Include(p => p.College) // ✅ IMPORTANT
                .FirstOrDefault(p => p.UserId == userId);

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var sold = _context.Products
                .Where(p => p.UserId == userId && p.IsSold)
                .ToList();

            var bought = _context.Products
                .Where(p => p.BuyerId == userId && p.IsSold)
                .ToList();

            var reviews = _context.Reviews
                .Where(r => r.SellerId == userId)
                .Include(r => r.Reviewer)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            double avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            int completion = profile == null ? 0 : GetProfileCompletion(profile);

            ViewBag.User = user;
            ViewBag.Profile = profile;
            ViewBag.Sold = sold;
            ViewBag.Bought = bought;
            ViewBag.AvgRating = avgRating;
            ViewBag.Completion = completion;
            ViewBag.Reviews = reviews;

            return View(profile);
        }

        // ================= OTHER USER PROFILE =================
        public IActionResult ViewProfile(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var profile = _context.UserProfiles
                .Include(p => p.College) // ✅ IMPORTANT
                .FirstOrDefault(p => p.UserId == userId);

            if (user == null)
                return NotFound();

            var sold = _context.Products
                .Where(p => p.UserId == userId && p.IsSold)
                .ToList();

            var bought = _context.Products
                .Where(p => p.BuyerId == userId)
                .ToList();

            var reviews = _context.Reviews
                .Where(r => r.SellerId == userId)
                .Include(r => r.Reviewer)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            double avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            ViewBag.User = user;
            ViewBag.Profile = profile;
            ViewBag.Sold = sold;
            ViewBag.Bought = bought;
            ViewBag.Reviews = reviews;
            ViewBag.AvgRating = avgRating;
            ViewBag.Completion = profile == null ? 0 : GetProfileCompletion(profile);
            ViewBag.IsVerified = profile != null && !string.IsNullOrEmpty(profile.PhoneNumber);

            return View("~/Views/User/Profile.cshtml");
        }

        // ================= COMPLETION =================
        private int GetProfileCompletion(UserProfile profile)
        {
            int total = 6;
            int filled = 0;

            if (!string.IsNullOrEmpty(profile.FirstName)) filled++;
            if (!string.IsNullOrEmpty(profile.LastName)) filled++;
            if (profile.DateOfBirth != null) filled++;
            if (!string.IsNullOrEmpty(profile.PhoneNumber)) filled++;

            // 🔥 FIXED
            if (profile.CollegeId != null) filled++;

            if (!string.IsNullOrEmpty(profile.ProfileImagePath)) filled++;

            return (filled * 100) / total;
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;

            await _userManager.UpdateAsync(user);
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Account");
        }

        // ================= SEARCH COLLEGE =================
        [HttpGet]
        public IActionResult SearchCollege(string term)
        {
            var colleges = _context.Colleges
                .Where(c => c.Name.Contains(term))
                .Select(c => new
                {
                    id = c.Id,
                    text = c.Name + ", " + c.City
                })
                .Take(10)
                .ToList();

            return Json(colleges);
        }
    }
}