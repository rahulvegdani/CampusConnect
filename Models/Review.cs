using System;
using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Models
{
    public class Review
    {
        public int Id { get; set; }

        // PRODUCT
        public int ProductId { get; set; }
        public Product? Product { get; set; }   // ✅ nullable

        // BUYER
        public string ReviewerId { get; set; } = string.Empty;   // ✅ FIX
        public ApplicationUser? Reviewer { get; set; }

        // SELLER
        public string SellerId { get; set; } = string.Empty;     // ✅ FIX
        public ApplicationUser? Seller { get; set; }

        // RATING
        [Range(1, 5)]
        public int Rating { get; set; }

        // COMMENT
        public string? Comment { get; set; }   // ✅ nullable allowed

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}