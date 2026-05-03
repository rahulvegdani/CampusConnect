using Microsoft.AspNetCore.SignalR;
using CampusConnect.Data;
using CampusConnect.Models;
using System.Security.Claims;

namespace CampusConnect.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= USER CONNECT =================
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                UserTracker.UserConnected(userId);

                // 🔔 Notify all users (online status)
                await Clients.All.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        // ================= USER DISCONNECT =================
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                UserTracker.UserDisconnected(userId);

                // 🔔 Notify all users (offline status)
                await Clients.All.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ================= SEND MESSAGE =================
        public async Task SendMessage(string senderId, string receiverId, string message, int productId)
        {
            var msg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                ProductId = productId,
                Timestamp = DateTime.Now,
                IsSeen = false
            };

            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            // 🔥 Send ONLY ONCE with ID
            await Clients.User(receiverId).SendAsync("ReceiveMessage", new
            {
                messageId = msg.Id,
                senderId,
                message,
                productId
            });

            await Clients.User(senderId).SendAsync("ReceiveMessage", new
            {
                messageId = msg.Id,
                senderId,
                message,
                productId
            });

            // 🔥 Delivered tick
            await Clients.User(senderId).SendAsync("MessageDelivered", new
            {
                messageId = msg.Id
            });

            // 🔔 CREATE NOTIFICATION
            var senderUser = _context.Users.FirstOrDefault(u => u.Id == senderId);

            var notification = new Notification
            {
                UserId = receiverId,
                Title = senderUser.Email + " has messaged you",
                Type = "Chat",
                RedirectUrl = $"/Chat?userId={senderId}&productId={productId}",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // 🔥 REAL-TIME NOTIFICATION
            await Clients.User(receiverId).SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                title = notification.Title,
                url = notification.RedirectUrl,
                time = notification.CreatedAt.ToString("hh:mm tt")
            });
        }

        // ================= TYPING INDICATOR =================
        public async Task Typing(string senderId, string receiverId)
        {

            await Clients.User(receiverId).SendAsync("UserTyping", senderId);
        }

        // ================= MESSAGE SEEN =================
        public async Task MarkAsSeen(int messageId, string senderId)
        {
            var msg = _context.ChatMessages.FirstOrDefault(m => m.Id == messageId);

            if (msg != null && !msg.IsSeen)
            {
                msg.IsSeen = true;
                await _context.SaveChangesAsync();

                // 🔥 Send seen update instantly
                await Clients.User(senderId).SendAsync("MessageSeen", new
                {
                    messageId = messageId
                });
            }
        }
    }

}
