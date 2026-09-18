using Resturant_Backend.DTO.Order;

namespace Resturant_Backend.DTO.Coupon
{
    public class GetCouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal Discount { get; set; }

        public decimal MinimumAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }

        public List<GetOrderDto> Orders { get; set; } = new();
    }
}
