using Resturant_Backend.DTO.Dashboard;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Models;

namespace Resturant_Backend.Interfaces
{
    public interface IOrder : IRepository<Order>
    {
        Task<(List<Order> Orders, int TotalCount)> GetAllAsync(FiltersOrders filters);

        List<Order> GetUserOrders(string id);
        Task<int> GetProductCountAsync();





        Task<RevenueSummaryDto> GetRevenueSummaryAsync();
        Task<OrdersSummaryDto> GetOrdersSummaryAsync();
        Task<List<RevenuePointDto>> GetRevenueTrendAsync(int days);




        Task<string?> LastModifiedBy(string userId);

    }
}
