using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Models;

namespace Resturant_Backend.Interfaces
{
    public interface IUser : IRepository<Appuser>
    {

        Task<(string address, bool found)> GetUserAddressAsync(string userId);


        Task<Appuser?> GetUserInformationAsync(string userId);
        Task<int> GetTotalUsersCountAsync();


        Task<(List<Appuser>, int totalcount)> GetUsersAsync(FiltersUsers filters);
    }
}
