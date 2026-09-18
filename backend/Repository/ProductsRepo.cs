using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Data;
using Resturant_Backend.DTO.Dashboard;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class ProductsRepo : Repository<Product>, IProducts
    {
        private readonly AppDbContext _context;
        public ProductsRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<Product> Products, int TotalCount)> GetAllAsync(Filters filters)
        {
            var query = _context.Products.AsQueryable();

            query = ApplySearch(query, filters.SearchTerm);
            query = ApplyPriceRange(query, filters.MinPrice, filters.MaxPrice);

            var totalCount = await query.CountAsync();

            var sortedProducts = SortProductBy(query, filters.SortByPrice, filters.SortBySelling, filters.Ascending);

            var pagedProducts = await sortedProducts
                .Skip(( filters.Pagination.PageNumber - 1 ) * filters.Pagination.PageSize)
                .Take(filters.Pagination.PageSize)
                .ToListAsync();

            return (pagedProducts, totalCount);
        }

        public override async Task<Product?> GetAsync(int id)
        {
            return await _context.Products.Include(c => c.Details).Include(c => c.CartItems).Include(c => c.Reviews)
                .ThenInclude(r => r.Appuser)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<int> GetProductCountAsync()
        {
            return _context.Products.CountAsync();
        }

        public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategory(string categoryName, Filters filters)
        {
            var query = _context.Products.Where(p => p.Category.Name == categoryName);

            query = ApplySearch(query, filters.SearchTerm);
            query = ApplyPriceRange(query, filters.MinPrice, filters.MaxPrice);

            var totalCount = await query.CountAsync();

            var sortedProducts = SortProductBy(query, filters.SortByPrice, filters.SortBySelling, filters.Ascending);

            var pagedProducts = await sortedProducts
                .Skip(( filters.Pagination.PageNumber - 1 ) * filters.Pagination.PageSize)
                .Take(filters.Pagination.PageSize)
                .ToListAsync();

            return (pagedProducts, totalCount);
        }

        public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategory(int categoryId, Filters filters)
        {
            var query = _context.Products.Where(p => p.Category.Id == categoryId);

            query = ApplySearch(query, filters.SearchTerm);
            query = ApplyPriceRange(query, filters.MinPrice, filters.MaxPrice);

            var totalCount = await query.CountAsync();

            var sortedProducts = SortProductBy(query, filters.SortByPrice, filters.SortBySelling, filters.Ascending);

            var pagedProducts = await sortedProducts
                .Skip(( filters.Pagination.PageNumber - 1 ) * filters.Pagination.PageSize)
                .Take(filters.Pagination.PageSize)
                .ToListAsync();

            return (pagedProducts, totalCount);
        }

        public async Task<(List<Product> Products, int TotalCount)> GetProductsByName(string name, Filters filters)
        {
            var query = _context.Products.Where(p => p.Name.Contains(name) || p.NameAr.Contains(name));

            query = ApplyPriceRange(query, filters.MinPrice, filters.MaxPrice);

            var totalCount = await query.CountAsync();

            var sortedProducts = SortProductBy(query, filters.SortByPrice, filters.SortBySelling, filters.Ascending);

            var pagedProducts = await sortedProducts
                .Skip(( filters.Pagination.PageNumber - 1 ) * filters.Pagination.PageSize)
                .Take(filters.Pagination.PageSize)
                .ToListAsync();

            return (pagedProducts, totalCount);
        }

        private static IQueryable<Product> ApplySearch(IQueryable<Product> query, string? searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
                return query;

            var term = searchTerm.Trim();
            return query.Where(p => p.Name.Contains(term) || p.NameAr.Contains(term));
        }

        private static IQueryable<Product> ApplyPriceRange(IQueryable<Product> query, decimal? minPrice, decimal? maxPrice)
        {
            if(minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if(maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return query;
        }

        public IQueryable<Product> SortProductBy(IQueryable<Product> products, bool? price, bool? selling, bool ascending)
        {
            if(price == true && selling == true)
            {
                return ascending
                    ? products.OrderBy(p => p.SellCount).ThenBy(p => p.Price)
                    : products.OrderByDescending(p => p.SellCount).ThenBy(p => p.Price);
            }
            if(price == true)
            {
                return ascending ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price);
            }
            if(selling == true)
            {
                return ascending ? products.OrderBy(p => p.SellCount) : products.OrderByDescending(p => p.SellCount);
            }

            return products;
        }









        /// <summary>
        /// most sold products based on the SellCount property, limited to the specified count.
        ///
        /// </summary>
        public async Task<List<TopProductDto>> GetTopSellingProductsAsync(int count)
        {
            if(count < 1)
                count = 5;

            return await _context.Products
                .OrderByDescending(p => p.SellCount)
                .Take(count)
                .Select(p => new TopProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameAr = p.NameAr,
                    ImageUrl = p.ImageUrl,
                    SellCount = p.SellCount,
                    Price = p.Price,
                })
                .ToListAsync();
        }
    }
}