using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Data;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class ReviewRepo : Repository<Review>, IReview
    {
        private readonly AppDbContext _context;
        public ReviewRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetAllReviews(int productId)
        {
            var reviews = await _context.Reviews.Include(c => c.Appuser).Where(r => r.ProductId == productId).ToListAsync();
            return reviews;
        }

        public async Task<List<Review>> GetTop10Review()
        {
            var reviews = await _context.Reviews.Include(c => c.Appuser).OrderByDescending(r => r.Rating).Take(10).ToListAsync();
            return reviews;
        }
    }
}
