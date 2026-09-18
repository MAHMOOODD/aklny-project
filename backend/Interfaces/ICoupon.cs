using Resturant_Backend.Models;

namespace Resturant_Backend.Interfaces
{
    public interface ICoupon : IRepository<Coupon>
    {

        Task<(string message, bool isvalid, decimal Discount)> ValidateCoupon(string coupon, decimal amount);
        Task<Coupon?> GetCoupon(string code);


    }
}
