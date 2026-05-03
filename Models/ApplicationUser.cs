using Microsoft.AspNetCore.Identity;

namespace CampusConnect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}