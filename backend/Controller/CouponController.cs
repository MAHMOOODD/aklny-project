using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.Coupon;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CouponController(IUnitOfWork unitOfWork, IMapper mapper)

        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllCoupons()
        {

            var coupons = await _unitOfWork.CouponRepo.GetAllAsync();

            Ensure.NotNull(coupons, "No Coupons Found");

            var couponsToShow = _mapper.Map<List<GetCouponDto>>(coupons);

            return this.Success(couponsToShow);

        }
        [Authorize(Roles = $"{Role.Admin},{Role.Manager}")]

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCouponById(int id)
        {
            var coupon = await _unitOfWork.CouponRepo.GetAsync(id);

            Ensure.NotNull(coupon, "No Coupon Found");
            var couponsToShow = _mapper.Map<GetCouponDto>(coupon);

            return this.Success(couponsToShow);
        }


        [Authorize(Roles = $"{Role.Admin}")]

        [HttpPost("Add")]
        public async Task<IActionResult> AddCoupon(AddCouponDto dto)

        {

            var coupon = _mapper.Map<Coupon>(dto);

            var addedCoupon = await _unitOfWork.CouponRepo.AddAsync(coupon);
            Ensure.NotNull(addedCoupon, "Cannot Add This Coupon");
            await _unitOfWork.SaveChangesAsync();




            var couponShow = _mapper.Map<GetCouponDto>(addedCoupon);

            return this.Success(couponShow);

        }
        [Authorize(Roles = $"{Role.Admin}")]


        [HttpPut("Edit/{id:int}")]
        public async Task<IActionResult> EditCoupon(int id, EditCouponDto dto)
        {
            var Coupon = await _unitOfWork.CouponRepo.GetAsync(id);



            Ensure.NotNull(Coupon, "No Coupon Found");
            _mapper.Map(dto, Coupon);

            await _unitOfWork.SaveChangesAsync();

            var couponShow = _mapper.Map<GetCouponDto>(Coupon);
            return this.Success(couponShow);
        }
        [Authorize(Roles = $"{Role.Admin}")]

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await _unitOfWork.CouponRepo.DeleteAsync(id);
            Ensure.NotNull(coupon, "Can not Delete This Coupon");
            await _unitOfWork.SaveChangesAsync();



            return this.SuccessMessage("Coupon deleted successfully.");

        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]

        [HttpPost("Validate")]
        public async Task<IActionResult> ValidateCoupon(string code, decimal Amount)
        {
            var (message, isvalid, discount) = await _unitOfWork.CouponRepo.ValidateCoupon(code, Amount);


            Ensure.Check(isvalid, message);

            return this.Success(new { Message = message, Discount = discount });
        }
    }
}
