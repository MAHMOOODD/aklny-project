using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Data;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class UserRepo : Repository<Appuser>, IUser
    {
        private AppDbContext _context;
        public UserRepo(AppDbContext context) : base(context)
        {
            this._context = context;
        }



        public async Task<(string address, bool found)> GetUserAddressAsync(string userId)
        {

            var user = await _context.Users.FindAsync(userId);

            if(user is null)
            {
                return ("", false);
            }

            var address = user.Address;
            if(address is null)
            {
                return ("", false);
            }
            return (address, true);
        }

        public async Task<Appuser?> GetUserInformationAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);


            return user is null ? null : user;
        }

        public async Task<(List<Appuser>, int totalcount)> GetUsersAsync(FiltersUsers filters)
        {

            var query = SortOrdersBy(_context.Users, filters.UserName, filters.Ascending);

            if(!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(u => u.UserName.Contains(filters.SearchTerm) || u.PhoneNumber.Contains(filters.SearchTerm));
            }
            var count = await query.CountAsync();

            query = query.Skip(( filters.Pagination.PageNumber - 1 ) * filters.Pagination.PageSize)
                .Take(filters.Pagination.PageSize);

            return (await query.ToListAsync(), count);
        }
        public IQueryable<Appuser> SortOrdersBy(IQueryable<Appuser> users, bool? SortByUsername, bool ascending)
        {


            if(SortByUsername == true)
            {
                return ascending ? users.OrderBy(p => p.UserName) : users.OrderByDescending(p => p.UserName);
            }



            return users;



        }

        /// <summary>
        /// return the total count of users in the database. 
        /// </summary>
        public Task<int> GetTotalUsersCountAsync()
        {
            return _context.Users.CountAsync();
        }
    }
}
