using CampusConnect.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace CampusConnect.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var data = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToList();

            return Json(data);
        }

        public IActionResult MarkAsRead(int id)
        {
            var notif = _context.Notifications.Find(id);
            if (notif != null)
            {
                notif.IsRead = true;
                _context.SaveChanges();
            }

            return Ok();
        }

        public IActionResult Delete(int id)
        {
            var notif = _context.Notifications.Find(id);
            if (notif != null)
            {
                _context.Notifications.Remove(notif);
                _context.SaveChanges();
            }

            return Ok();
        }

        public IActionResult ClearAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifs = _context.Notifications
                .Where(n => n.UserId == userId)
                .ToList();

            _context.Notifications.RemoveRange(notifs);
            _context.SaveChanges();

            return Ok();
        }
    }
}