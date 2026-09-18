using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.User;

public class UpdateProfileDto
{
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Full name can only contain letters and spaces.")]
    public string? FullName { get; set; }
    public string? Address { get; set; }
    [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    public IFormFile? ImageUrl { get; set; }
}
