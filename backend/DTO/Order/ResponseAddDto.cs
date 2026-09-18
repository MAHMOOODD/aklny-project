using Resturant_Backend.Models;

namespace Resturant_Backend.DTO.Order
{
    public class ResponseAddDto
    {

        public int Id { get; set; }

        public string AppuserId { get; set; }

        public string UserAddress { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal PriceAfterDiscount { get; set; }

        public OrderStatus Status { get; set; }


        public PaymentStatus PaymentStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Coupon { get; set; }

        public decimal? Discount { get; set; }



    }
}
