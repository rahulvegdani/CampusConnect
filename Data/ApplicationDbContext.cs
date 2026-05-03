using CampusConnect.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; } 
         
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Wishlist> Wishlists { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<College> Colleges { get; set; }
    }
}