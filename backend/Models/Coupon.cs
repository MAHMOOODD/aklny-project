
namespace Resturant_Backend.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal Discount { get; set; }

        public decimal MinimumAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(7);
        public List<Order> Orders { get; set; }
    }
}
