using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.User;
using Resturant_Backend.Helpers;
using Resturant_Backend.Helpers.PhotosHandle;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Resturant_Backend.Services;

public class Authservice : IAuthService
{
    private readonly UserManager<Appuser> _userManager;
    private readonly JwtHelper _jwtHelper;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<Appuser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IPhotoService _photoService;
    public Authservice(
        UserManager<Appuser> userManager,
        IOptions<JwtHelper> options,
        RoleManager<IdentityRole> roleManager,
        SignInManager<Appuser> signInManager,
        IEmailService emailService,
        IPhotoService photoService
        )
    {
        _userManager = userManager;
        _jwtHelper = options.Value;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _photoService = photoService;
    }

    public async Task<UserCreatedModel> RegisterAsync(RegisterModel model, string origin)
    {
        if(await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            Ensure.FieldError(nameof(model.Email), "Email is already registered.");
        }

        if(await _userManager.FindByNameAsync(model.UserName) is not null)
        {
            Ensure.FieldError(nameof(model.UserName), "Username is already registered.");
        }

        var user = new Appuser
        {
            FullName = model.FullName,
            Address = model.Address,
            UserName = model.UserName,
            Email = model.Email,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if(!result.Succeeded)
        {
            //gather all errors and group them by field for better clarity
            var errorsDict = new Dictionary<string, List<string>>();
            foreach(var error in result.Errors)
            {
                string? fieldKey = error.Code.Contains("Password") ? "Password" :
                               error.Code.Contains("Email") ? "Email" :
                               error.Code.Contains("UserName") ? "UserName" : "General";

                if(!errorsDict.ContainsKey(fieldKey))
                    errorsDict[fieldKey] = new List<string>();

                errorsDict[fieldKey].Add(error.Description);
            }
            Ensure.Validation(errorsDict, "Registration failed due to validation errors.");
        }

        await _userManager.AddToRoleAsync(user, "User");

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var confirmationUrl = $"{origin}/auth/confirm-email?userId={user.Id}&token={encodedCode}";

        var messageBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                <h2>مرحباً بك في موقعنا! 👋</h2>
                <p>شكراً لتسجيلك. يرجى الضغط على الزر أدناه لتأكيد بريدك الإلكتروني:</p>
                <a href='{confirmationUrl}' style='display: inline-block; padding: 10px 20px; color: #fff; background-color: #28a745; text-decoration: none; border-radius: 5px;'>تأكيد البريد الإلكتروني</a>
            </div>";

        await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", messageBody);

        return new UserCreatedModel
        {
            Email = user.Email,
            IsAuth = true,
            Message = "User registered successfully! Please check your email to confirm your account.",
            UserName = user.UserName,
            Roles = new List<string> { "User" }
        };
    }

    public async Task<UserCreatedModel> GetTokenAsync(TokenRequestModel model)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if(user is null)
        {
            Ensure.FieldError(nameof(model.Email), "Email or Password is incorrect!");
        }

        if(!user.EmailConfirmed)
        {
            Ensure.FieldError(nameof(model.Email), "Email is not confirmed yet. Please check your inbox.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

        if(result.IsLockedOut)
        {
            Ensure.BadRequest("Account is locked. Try again after 5 minutes.");
        }

        if(!result.Succeeded)
        {
            Ensure.FieldError(nameof(model.Password), "Email or Password is incorrect!");
        }

        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        var authModel = new UserCreatedModel
        {
            IsAuth = true,
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            Email = user.Email,
            UserName = user.UserName,
            ExpiredOn = jwtSecurityToken.ValidTo,
            Roles = rolesList.ToList()
        };

        // دايمًا نعمل refresh token جديد بمدة كاملة عند تسجيل الدخول
        // (بدل ما نرجّع توكن قديم قرب ينتهي ونضطر نعمل refresh بعده بلحظات)
        var refreshToken = GenerateRefreshToken();
        authModel.RefreshToken = refreshToken.Token;
        authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return authModel;
    }
    public async Task<string> ConfirmEmailAsync(ConfirmEmailDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        Ensure.NotNull(user, "User not found");
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        Ensure.Check(result.Succeeded, "Failed to confirm email");

        return string.Empty;
    }

    public async Task<string> ForgetPasswordAsync(ForgetPasswordDto model, string origin)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        Ensure.NotNull(user, "Email does not exist");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = $"{origin}/auth/reset-password?email={user.Email}&token={encodedToken}";

        var messageBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                <h2>طلب إعادة تعيين كلمة المرور 🔐</h2>
                <p>لقد أرسلت طلباً لإعادة تعيين كلمة المرور الخاصة بك. اضغط على الزر للتغيير:</p>
                <a href='{resetUrl}' style='display: inline-block; padding: 10px 20px; color: #fff; background-color: #dc3545; text-decoration: none; border-radius: 5px;'>إعادة تعيين كلمة المرور</a>
            </div>";

        await _emailService.SendEmailAsync(user.Email, "Reset Your Password", messageBody);

        return string.Empty;
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        Ensure.NotNull(user, "User not found");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

        Ensure.Check(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));

        return string.Empty;
    }

    public async Task<string> UpdateProfileAsync(string userId, UpdateProfileDto model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        Ensure.NotNull(user, "User not found");

        user.FullName = model.FullName ?? user.FullName;
        user.Address = model.Address ?? user.Address;

        // handle profile image update
        if(model.ImageUrl != null && model.ImageUrl.Length > 0)
        {
            // delete the old image from cloudinary if it exists
            if(!string.IsNullOrEmpty(user.ImagePublicId))
            {
                await _photoService.DeletePhotoAsync(user.ImagePublicId);
            }

            var uploadResult = await _photoService.AddPhotoAsync(model.ImageUrl);
            Ensure.Check(uploadResult.Error == null, uploadResult.Error?.Message ?? "Image upload failed");

            user.ImageUrl = uploadResult.SecureUrl.ToString();
            user.ImagePublicId = uploadResult.PublicId;
        }

        var result = await _userManager.UpdateAsync(user);
        Ensure.Check(result.Succeeded, "Failed to update profile");

        if(model.PhoneNumber is not null)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            Ensure.Check(setPhoneResult.Succeeded, "Failed to update phone number");
        }

        return string.Empty;
    }

    public async Task<string> AddRoleAsync(AddRoleDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        Ensure.NotNull(user, "Invalid user ID");

        var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
        Ensure.Check(roleExists, "Role does not exist");

        var isInRole = await _userManager.IsInRoleAsync(user!, model.RoleName);
        Ensure.Check(!isInRole, "User already assigned to this role");

        var result = await _userManager.AddToRoleAsync(user!, model.RoleName);
        Ensure.Check(result.Succeeded, "Something went wrong");

        return string.Empty;
    }

    public async Task<string> RemoveRoleAsync(AddRoleDto model)
    {
        if(model.RoleName == Role.Admin)
        {
            var admins = await _userManager.GetUsersInRoleAsync(Role.Admin);
            Ensure.Check(admins.Count > 1, "Cannot remove the last remaining Admin");
        }
        var user = await _userManager.FindByIdAsync(model.UserId);
        Ensure.NotNull(user, "Invalid user ID");

        var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
        Ensure.Check(roleExists, "Role does not exist");

        var isInRole = await _userManager.IsInRoleAsync(user!, model.RoleName);
        Ensure.Check(isInRole, "User is not assigned to this role");

        var result = await _userManager.RemoveFromRoleAsync(user!, model.RoleName);
        Ensure.Check(result.Succeeded, "Something went wrong");

        return string.Empty;
    }

    public async Task<UserCreatedModel> RefreshTokenAsync(string token)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        Ensure.NotNull(user, "Invalid token");

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
        Ensure.Check(refreshToken.IsActive, "Inactive token");

        refreshToken.RevokeOn = DateTime.UtcNow;

        var newRefreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        var jwtToken = await CreateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new UserCreatedModel
        {
            IsAuth = true,
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            Email = user.Email,
            UserName = user.UserName,
            Roles = roles.ToList(),
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiration = newRefreshToken.ExpiresOn
        };
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if(user == null)
            return false;

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
        if(!refreshToken.IsActive)
            return false;

        refreshToken.RevokeOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return true;
    }

    private async Task<JwtSecurityToken> CreateJwtToken(Appuser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
        }
        .Union(userClaims)
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtHelper.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _jwtHelper.Issuer,
            audience: _jwtHelper.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtHelper.DurationInMinutes),
            signingCredentials: signingCredentials);
    }

    private RefreshToken GenerateRefreshToken()
    {
        var token = RandomNumberGenerator.GetBytes(32);
        var now = DateTimeOffset.UtcNow;

        return new RefreshToken
        {
            Token = Convert.ToBase64String(token),
            ExpiresOn = now.AddDays(10).UtcDateTime,
            CreatedOn = now.UtcDateTime
        };
    }
}