using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ProfileImagePath { get; set; }

        public int? CollegeId { get; set; }

        public College? College { get; set; }
    }
}