using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.User;

public class ForgetPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
