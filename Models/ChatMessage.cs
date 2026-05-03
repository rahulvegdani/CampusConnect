using System;

namespace CampusConnect.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string SenderId { get; set; }

        public string ReceiverId { get; set; }

        public string Message { get; set; }

        public bool IsSeen { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsDeletedBySender { get; set; } = false;

        public bool IsDeletedByReceiver { get; set; } = false; 
    }
}