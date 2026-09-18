using Resturant_Backend.Models;

namespace Resturant_Backend.DTO.Order
{
    public class StatusResponseDto
    {
        public OrderStatus? Status { get; set; }


        public PaymentStatus? PaymentStatus { get; set; }
    }
}
