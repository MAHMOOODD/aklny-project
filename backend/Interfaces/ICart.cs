using Resturant_Backend.Models;

namespace Resturant_Backend.Interfaces
{
    public interface ICart : IRepository<Cart_Item>
    {


        List<Cart_Item> GetAllCartItems(string userId);
        Task<bool> ClearCart(string userId);
        Task<bool> IsItemExist(int productId, string userId);

    }
}
