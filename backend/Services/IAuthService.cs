using Resturant_Backend.DTO.User;

namespace Resturant_Backend.Services;

public interface IAuthService
{
    Task<UserCreatedModel> RegisterAsync(RegisterModel model, string origin);
    Task<UserCreatedModel> GetTokenAsync(TokenRequestModel model);
    Task<string> AddRoleAsync(AddRoleDto model);
    Task<UserCreatedModel> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);

    Task<string> ConfirmEmailAsync(ConfirmEmailDto model);
    Task<string> ForgetPasswordAsync(ForgetPasswordDto model, string origin);
    Task<string> ResetPasswordAsync(ResetPasswordDto model);
    Task<string> UpdateProfileAsync(string userId, UpdateProfileDto model);
    Task<string> RemoveRoleAsync(AddRoleDto model);
}