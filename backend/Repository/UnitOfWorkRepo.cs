using Resturant_Backend.Data;
using Resturant_Backend.Interfaces;

namespace Resturant_Backend.Repository
{
    public class UnitOfWorkRepo : IUnitOfWork
    {
        private readonly AppDbContext _context;

        // private field for each repository



        private ICategoreis _categoreisRepo;
        private IProducts _productsRepo;
        private ICart _cartRepo;
        private ICoupon _couponRepo;
        private IReview _reviewRepo;
        private IDetails _detailsRepo;
        private IUser _userRepo;
        private IOrder _orderRepo;

        // context injection 

        public UnitOfWorkRepo(AppDbContext context)
        {
            _context = context;
        }

        public ICategoreis CategoreisRepo => _categoreisRepo ?? new CategoreisRepo(_context);
        public IProducts ProductsRepo => _productsRepo ?? new ProductsRepo(_context);
        public ICart CartRepo => _cartRepo ?? new CartRepo(_context);
        public ICoupon CouponRepo => _couponRepo ?? new CouponRepo(_context);

        public IReview ReviewRepo => _reviewRepo ?? new ReviewRepo(_context);
        public IDetails DetailsRepo => _detailsRepo ?? new DetailsRepo(_context);

        public IUser UserRepo => _userRepo ?? new UserRepo(_context);

        public IOrder OrderRepo => _orderRepo ?? new OrderRepo(_context);


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
