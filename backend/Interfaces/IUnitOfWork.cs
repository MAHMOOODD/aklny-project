namespace Resturant_Backend.Interfaces
{
    public interface IUnitOfWork
    {
        // property for each repository


        ICategoreis CategoreisRepo { get; }
        IProducts ProductsRepo { get; }
        ICart CartRepo { get; }
        ICoupon CouponRepo { get; }
        IReview ReviewRepo { get; }
        IDetails DetailsRepo { get; }
        IUser UserRepo { get; }
        IOrder OrderRepo { get; }

        Task SaveChangesAsync();

    }
}
