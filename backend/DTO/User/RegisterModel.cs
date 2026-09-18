using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.User;

public class RegisterModel
{
    public string? FullName { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
