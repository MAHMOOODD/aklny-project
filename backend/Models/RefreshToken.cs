using Microsoft.EntityFrameworkCore;

namespace Resturant_Backend.Models
{
    [Owned]
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;

        public DateTime CreatedOn { get; set; }
        public DateTime? RevokeOn { get; set; }

        public bool IsActive => RevokeOn is null && !IsExpired;
    }
}