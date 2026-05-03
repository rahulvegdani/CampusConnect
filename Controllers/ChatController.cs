using CampusConnect.Data;
using CampusConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace CampusConnect.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= CHAT PAGE =================
        public IActionResult Index(string userId, int productId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ✅ GET OTHER USER
            var otherUser = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (otherUser == null || otherUser.IsDeleted)
            {
                ViewBag.Messages = new List<ChatMessage>();
                ViewBag.ProductName = "User not available";
                return View();
            }

            // ✅ GET MESSAGES
            var messages = _context.ChatMessages
                .Where(m =>
                    m.ProductId == productId &&
                    (
                        (m.SenderId == currentUser && m.ReceiverId == userId) ||
                        (m.SenderId == userId && m.ReceiverId == currentUser)
                    )
                )
                .OrderBy(m => m.Timestamp)
                .ToList();

            // ✅ MARK AS SEEN
            var unseenMessages = _context.ChatMessages
                .Where(m =>
                    m.ProductId == productId &&
                    m.ReceiverId == currentUser &&
                    m.SenderId == userId &&
                    !m.IsSeen
                );

            foreach (var msg in unseenMessages)
            {
                msg.IsSeen = true;
            }

            _context.SaveChanges();

            // ✅ GET PRODUCT
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                ViewBag.ProductName = "Unknown Product";
                ViewBag.IsSold = false;
                ViewBag.IsBuyer = false;
                ViewBag.AlreadyReviewed = false;
            }
            else
            {
                ViewBag.ProductName = product.Name;
                ViewBag.IsSold = product.IsSold;
                ViewBag.IsBuyer = product.BuyerId == currentUser;

                var alreadyReviewed = _context.Reviews
                    .Any(r => r.ProductId == productId && r.ReviewerId == currentUser);

                ViewBag.AlreadyReviewed = alreadyReviewed;
            }

            // 🔥 IMPORTANT FIX (MISSING BEFORE)
            ViewBag.ReceiverEmail = otherUser.Email;

            // ✅ SEND DATA
            ViewBag.Messages = messages;
            ViewBag.ReceiverId = userId;
            ViewBag.ProductId = productId;

            return View();
        }

        // ================= DELETE SINGLE MESSAGE =================
        [HttpPost]
        public IActionResult DeleteMessage(int messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var message = _context.ChatMessages.FirstOrDefault(m => m.Id == messageId);

            if (message == null)
                return NotFound();

            if (message.SenderId == userId)
                message.IsDeletedBySender = true;

            if (message.ReceiverId == userId)
                message.IsDeletedByReceiver = true;

            _context.SaveChanges();

            return Ok();
        }

        // ================= DELETE FULL CHAT =================
        [HttpPost]
        public IActionResult DeleteChat(string otherUserId, int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = _context.ChatMessages
                .Where(m => m.ProductId == productId &&
                       (
                           (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == userId)
                       ))
                .ToList();

            foreach (var msg in messages)
            {
                if (msg.SenderId == userId)
                    msg.IsDeletedBySender = true;

                if (msg.ReceiverId == userId)
                    msg.IsDeletedByReceiver = true;
            }

            _context.SaveChanges();

            return Ok();
        }

        // ================= CHAT INBOX =================
        public IActionResult Inbox(string search)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chats = _context.ChatMessages
                .Where(c => c.ReceiverId == userId || c.SenderId == userId)
                .GroupBy(c => new
                {
                    c.ProductId,
                    OtherUserId = c.SenderId == userId ? c.ReceiverId : c.SenderId
                })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    OtherUserId = g.Key.OtherUserId,

                    ProductName = _context.Products
                        .Where(p => p.Id == g.Key.ProductId)
                        .Select(p => p.Name)
                        .FirstOrDefault(),

                    BuyerId = _context.Products
                        .Where(p => p.Id == g.Key.ProductId)
                        .Select(p => p.BuyerId)
                        .FirstOrDefault(),

                    IsSold = _context.Products
                        .Where(p => p.Id == g.Key.ProductId)
                        .Select(p => p.IsSold)
                        .FirstOrDefault(),

                    AlreadyReviewed = _context.Reviews
                        .Any(r => r.ProductId == g.Key.ProductId && r.ReviewerId == userId),

                    BuyerName = _context.Users
                        .Where(u => u.Id == g.Key.OtherUserId)
                        .Select(u => u.Email)
                        .FirstOrDefault(),

                    ProfileImage = _context.UserProfiles
                        .Where(p => p.UserId == g.Key.OtherUserId)
                        .Select(p => p.ProfileImagePath)
                        .FirstOrDefault(),

                    Count = g.Count(), // ✅ RESTORED (THIS FIXES YOUR ERROR)

                    Unread = g.Count(m => !m.IsSeen && m.ReceiverId == userId),

                    LastMessageTime = g.Max(m => m.Timestamp)
                })
                .OrderByDescending(x => x.LastMessageTime)
                .ToList();

            // 🔍 SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                chats = chats.Where(x =>
                    (x.BuyerName != null && x.BuyerName.ToLower().Contains(search)) ||
                    (x.ProductName != null && x.ProductName.ToLower().Contains(search))
                ).ToList();
            }

            ViewBag.Search = search;

            return View(chats);
        }
    }
}