using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.Review;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;
using System.Security.Claims;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReviewController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("Get/{ProductId:int}")]
        public async Task<IActionResult> GetReviews(int ProductId)
        {

            var product = await _unitOfWork.ProductsRepo.GetAsync(ProductId);

            Ensure.NotNull(product, message: "Product Not Found");

            var reviews = await _unitOfWork.ReviewRepo.GetAllReviews(ProductId);
            Ensure.NotNull(reviews, message: "No Reviews Found");



            var reviewToShow = _mapper.Map<List<GetReviewDto>>(reviews);
            return this.Success(reviewToShow);
        }

        [HttpGet("GetTop10")]
        public async Task<IActionResult> GetTop10Reviews()
        {
            var reviews = await _unitOfWork.ReviewRepo.GetTop10Review();

            if(reviews is null)
                this.NotFoundEx("No reviews found");

            var reviewToShow = _mapper.Map<List<GetReviewDto>>(reviews);
            return this.Success(reviewToShow);

        }


        [Authorize(Roles = $"{Role.Admin},{Role.Manager}")]

        [HttpGet("getById/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var review = await _unitOfWork.ReviewRepo.GetAsync(id);
            Ensure.NotNull(review, "Review Not Found");


            var reviewToShow = _mapper.Map<GetReviewDto>(review);

            return this.Success(reviewToShow);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]

        [HttpPost("Add")]
        public async Task<IActionResult> Add(AddReviewDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Ensure.Unauthorized(userId, " يجب تسجيل الدخول أولا");

            var reviewToAdd = _mapper.Map<Review>(dto);
            reviewToAdd.AppuserId = userId;
            var review = await _unitOfWork.ReviewRepo.AddAsync(reviewToAdd);


            Ensure.NotNull(review, "Review Not Added");


            await _unitOfWork.SaveChangesAsync();

            GetReviewDto reviewToShow = _mapper.Map<GetReviewDto>(review);
            return this.Success(reviewToShow);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Edit(int id, EditReviewDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Ensure.Unauthorized(userId, " يجب تسجيل الدخول أولا");

            var review = await _unitOfWork.ReviewRepo.GetAsync(id);
            Ensure.NotNull(review, "Review Not Found");


            _mapper.Map(dto, review);

            review.AppuserId = userId;
            await _unitOfWork.SaveChangesAsync();
            var reviewToShow = _mapper.Map<GetReviewDto>(review);
            return this.Success(reviewToShow);
        }
        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deletedReview = await _unitOfWork.ReviewRepo.DeleteAsync(id);
            Ensure.NotNull(deletedReview, "Review Not Found");
            await _unitOfWork.SaveChangesAsync();




            return this.SuccessMessage("Review deleted successfully");
        }
    }
}
