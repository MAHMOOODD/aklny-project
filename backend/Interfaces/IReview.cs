using Resturant_Backend.Models;

namespace Resturant_Backend.Interfaces
{
    public interface IReview : IRepository<Review>
    {


        Task<List<Review>> GetAllReviews(int productId);

        Task<List<Review>> GetTop10Review();

    }
}
