using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.Categories
{
    public class AddCategoriesDto
    {
        [Required(ErrorMessage = "Name Is Required")]
        [MinLength(3, ErrorMessage = "Min Length Is 3 Letters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Name Is Required")]
        [MinLength(3, ErrorMessage = "Min Length Is 3 Letters")]
        public string NameAr { get; set; } = "";


        [Required(ErrorMessage = "Image URL Is Required")]

        public IFormFile? ImageUrl { get; set; }
    }
}
