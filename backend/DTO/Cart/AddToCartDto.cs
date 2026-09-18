using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.Cart
{
    public class AddToCartDto
    {
        public string AppuserId { get; set; }
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
