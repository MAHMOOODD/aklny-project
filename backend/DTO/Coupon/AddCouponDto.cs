using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.Coupon
{
    public class AddCouponDto
    {
        public string Code { get; set; }
        [Range(0, 99, ErrorMessage = ( "cannot be negative" ))]
        public decimal Discount { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = ( "Minimum amount should be greater than 0" ))]
        public decimal MinimumAmount { get; set; }


    }
}
