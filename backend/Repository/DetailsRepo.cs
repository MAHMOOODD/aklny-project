using Resturant_Backend.Data;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class DetailsRepo : Repository<OrderDetails>, IDetails
    {
        public DetailsRepo(AppDbContext context) : base(context)
        {
        }
    }
}
