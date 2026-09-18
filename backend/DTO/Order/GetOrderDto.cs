using Resturant_Backend.DTO.OrderDetails;
using Resturant_Backend.Models;
using System.Text.Json.Serialization;

namespace Resturant_Backend.DTO.Order
{
    public class GetOrderDto
    {
        public int Id { get; set; }

        [JsonPropertyName("appUserId")]
        public string AppuserId { get; set; }
        public string UserAddress { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; }


        public PaymentStatus PaymentStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? LastModifiedBy { get; set; }

        public int? CouponId { get; set; }

        public decimal? Discount { get; set; }





        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }        // AmountPaid - TotalPrice
        public OrderSource Source { get; set; }
        public List<GetDetailsDto> OrderDetails { get; set; } = new();

    }
}
