using CampusConnect.Data;
using CampusConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace CampusConnect.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ❤️ ADD TO WISHLIST
        public IActionResult Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exists = _context.Wishlists
                .FirstOrDefault(w => w.ProductId == productId && w.UserId == userId);

            if (exists == null)
            {
                var item = new Wishlist
                {
                    ProductId = productId,
                    UserId = userId
                };

                _context.Wishlists.Add(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Products");
        }

        // 📄 VIEW WISHLIST
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var items = _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.Product)
                .ToList();

            return View(items);
        }

        // ❌ REMOVE
        public IActionResult Remove(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = _context.Wishlists
                .FirstOrDefault(w => w.ProductId == productId && w.UserId == userId);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}