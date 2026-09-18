using Resturant_Backend.DTO.Dashboard;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Models;

namespace Resturant_Backend.Interfaces
{
    public interface IProducts : IRepository<Product>
    {
        Task<(List<Product> Products, int TotalCount)> GetAllAsync(Filters filters);


        IQueryable<Product> SortProductBy(IQueryable<Product> products, bool? price, bool? selling, bool ascending);
        Task<int> GetProductCountAsync();
        Task<(List<Product> Products, int TotalCount)> GetProductsByCategory(string categoryName, Filters filters);
        Task<(List<Product> Products, int TotalCount)> GetProductsByCategory(int categoryId, Filters filters);

        Task<List<TopProductDto>> GetTopSellingProductsAsync(int count);
        Task<(List<Product> Products, int TotalCount)> GetProductsByName(string name, Filters filters);
    }
}
