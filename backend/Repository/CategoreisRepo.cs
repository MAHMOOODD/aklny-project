using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Data;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class CategoreisRepo : Repository<Category>, ICategoreis
    {
        private readonly AppDbContext _context;
        public CategoreisRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public override async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.Include(c => c.Products).ToListAsync();
        }

        public override async Task<Category?> GetAsync(int id)
        {
            return await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
