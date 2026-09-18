using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Models;

namespace Resturant_Backend.Data
{
    public class AppDbContext : IdentityDbContext<Appuser>
    {

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Cart_Item> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }




    }
}
