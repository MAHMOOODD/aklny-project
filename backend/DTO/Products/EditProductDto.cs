using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.Products
{
    public class EditProductDto
    {

        [Required(ErrorMessage = "Name Is Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Name Is Required")]
        public string NameAr { get; set; } = "";

        [Required(ErrorMessage = "Description Is Required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Description Is Required")]
        public string DescriptionAr { get; set; } = "";


        [Required(ErrorMessage = "Price Is Required")]
        public decimal Price { get; set; }

        public int PreparingTime { get; set; } // in minutes



        public IFormFile? ImageUrl { get; set; }
        [Required(ErrorMessage = "Availability Status Is Required")]
        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "Category ID Is Required")]
        public int CategoryId { get; set; }
    }
}
