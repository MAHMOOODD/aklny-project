using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.User;

public class TokenRequestModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
