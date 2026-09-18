using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Data;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class CartRepo : Repository<Cart_Item>, ICart
    {
        private AppDbContext _context;

        public CartRepo(AppDbContext context) : base(context)
        {
            this._context = context;
        }


        public List<Cart_Item> GetAllCartItems(string userId)
        {

            var cart = _context.CartItems.Include(c => c.Product).Where(c => c.AppuserId == userId);
            return cart.ToList();


        }
        public async Task<bool> ClearCart(string userId)
        {
            var items = GetAllCartItems(userId);
            if(items is null || items.Count < 1)
            {
                return false;
            }

            await _context.CartItems.ExecuteDeleteAsync();
            return true;

        }

        public async Task<bool> IsItemExist(int productId, string userId)
        {
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.AppuserId == userId);

            if(item is null)
            {
                return false;
            }
            return true;
        }



    }
}
