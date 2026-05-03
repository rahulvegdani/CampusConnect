namespace CampusConnect.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int ProductId { get; set; }

        public DateTime CreateAdt { get; set; } = DateTime.Now;

        public Product Product { get; set; }
    }
}
