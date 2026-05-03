using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CampusConnect.Models
{
    public class Product
    {
        public int Id { get; set; }

    // Product name
    [Required]
        public string? Name { get; set; }

        // Product description
        public string? Description { get; set; }

        // Product price
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Image path stored in database
        public string? ImagePath { get; set; }

        // Product approval system
        public bool IsApproved { get; set; } = false;

        public bool IsRejected { get; set; } = false;

        // Sold status
        public bool IsSold { get; set; } = false;

        public string? BuyerId { get; set; }

        public DateTime? SoldAt { get; set; }

        // Product views counter
        public int Views { get; set; } = 0;

        // Upload date
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Category relationship
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // User relationship (seller)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Image upload (not stored in database)
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }

}
