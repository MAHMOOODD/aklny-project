using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.User;

public class AddRoleDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RoleName { get; set; } = string.Empty;
}
