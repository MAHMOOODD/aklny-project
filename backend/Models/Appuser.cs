using Microsoft.AspNetCore.Identity;

namespace Resturant_Backend.Models
{
    public class Appuser : IdentityUser
    {

        public string? FullName { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }


        public string? Address { get; set; }


        public List<Order> Orders { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Cart_Item> CartItems { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
