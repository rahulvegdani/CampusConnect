using System;

namespace CampusConnect.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } // receiver

        public string Title { get; set; } // "vegdanirahul has messaged you"

        public string Type { get; set; } // Chat, Wishlist, System

        public string RedirectUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}