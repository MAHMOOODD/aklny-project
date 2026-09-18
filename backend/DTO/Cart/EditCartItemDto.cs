using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.Cart
{
    public class EditCartItemDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
