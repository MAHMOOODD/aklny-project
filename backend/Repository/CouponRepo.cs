using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Data;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class CouponRepo : Repository<Coupon>, ICoupon
    {
        private readonly AppDbContext _context;

        public CouponRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<Coupon?> GetAsync(int id)
        {
            return await _dbSet.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id);
        }
        public override async Task<List<Coupon>?> GetAllAsync()
        {
            return await _dbSet.Include(c => c.Orders).Where(c => c.ExpiryDate > DateTime.Now).ToListAsync();
        }



        public async Task<(string message, bool isvalid, decimal Discount)> ValidateCoupon(string coupon, decimal amount)
        {

            var couponFromDb = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == coupon);
            if(couponFromDb is null)
            {
                return ("Coupon Not Found", false, 0);

            }
            if(couponFromDb.ExpiryDate < DateTime.Now)
            {
                return ("Coupon Expired", false, 0);
            }
            if(couponFromDb.MinimumAmount > amount)
            {
                return ("Coupon Minimum Amount Not Met", false, 0);
            }

            return ("Coupon is valid", true, couponFromDb.Discount);
        }


        public async Task<Coupon?> GetCoupon(string code)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
        }
    }
}
