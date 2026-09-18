using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.User;
using Resturant_Backend.Helpers;
using Resturant_Backend.Services;
using System.Security.Claims;

namespace Resturant_Backend.Controller;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly IOptions<JwtHelper> _jwtOptions;
    private readonly IWebHostEnvironment _env;

    public AccountController(
        IAuthService authService,
        IMapper mapper,
        IOptions<JwtHelper> jwtOptions,
        IWebHostEnvironment env)
    {
        _authService = authService;
        _mapper = mapper;
        _jwtOptions = jwtOptions;
        _env = env;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
    {
        var origin = _jwtOptions.Value.Audience?.TrimEnd('/');
        var result = await _authService.RegisterAsync(model, origin);

        Ensure.Check(result.IsAuth, result?.Message ?? "Registration failed.");

        if(!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return this.Success(_mapper.Map<ResponseRegister>(result));
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmailAsync([FromQuery] ConfirmEmailDto model)
    {
        await _authService.ConfirmEmailAsync(model);
        return this.SuccessMessage("Email confirmed successfully!");
    }

    [HttpPost("Login")]
    public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
    {
        var result = await _authService.GetTokenAsync(model);

        Ensure.Check(result.IsAuth, result?.Message ?? "Invalid email or password.");

        if(!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return this.Success(_mapper.Map<ResponseLogin>(result));
    }

    [HttpPost("ForgetPassword")]
    public async Task<IActionResult> ForgetPasswordAsync([FromBody] ForgetPasswordDto model)
    {
        var origin = _jwtOptions.Value.Audience;
        await _authService.ForgetPasswordAsync(model, origin);

        return this.SuccessMessage("Password reset link has been sent to your email.");
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto model)
    {
        await _authService.ResetPasswordAsync(model);
        return this.SuccessMessage("Password reset successfully!");
    }

    [Authorize]
    [HttpPut("UpdateProfile")]
    public async Task<IActionResult> UpdateProfileAsync([FromForm] UpdateProfileDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Ensure.Unauthorized(userId, "غير مصرح لك بالوصول، يرجى تسجيل الدخول.");

        await _authService.UpdateProfileAsync(userId!, model);
        return this.SuccessMessage("Profile updated successfully!");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("AddRole")]
    public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleDto model)
    {
        await _authService.AddRoleAsync(model);
        return this.SuccessMessage("Role added successfully!");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("RemoveRole")]
    public async Task<IActionResult> RemoveRoleAsync([FromBody] AddRoleDto model)
    {
        await _authService.RemoveRoleAsync(model);
        return this.SuccessMessage("Role removed successfully!");
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        Ensure.NotNullOrEmpty(refreshToken, "Refresh token is required!");

        var result = await _authService.RefreshTokenAsync(refreshToken!);
        Ensure.Check(result.IsAuth, result?.Message ?? "Invalid refresh token.");

        SetRefreshTokenInCookie(result.RefreshToken!, result.RefreshTokenExpiration);

        return this.Success(result);
    }

    [HttpPost("RevokeToken")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeToken? dto)
    {
        var token = dto?.Token ?? Request.Cookies["refreshToken"];
        Ensure.NotNullOrEmpty(token, "Token is required!");

        var isRevoked = await _authService.RevokeTokenAsync(token!);
        Ensure.Check(isRevoked, "Token is invalid!");

        // delete the refresh token cookie when revoking the token
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None
        });

        return this.SuccessMessage("Token revoked successfully.");
    }

    private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expires.ToLocalTime(),
            IsEssential = true,
            Secure = !_env.IsDevelopment(),
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}