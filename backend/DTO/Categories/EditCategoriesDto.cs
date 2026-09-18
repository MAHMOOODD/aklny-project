using System.ComponentModel.DataAnnotations;

namespace Resturant_Backend.DTO.Categories
{
    public class EditCategoriesDto
    {

        [Required(ErrorMessage = "Name Is Required")]
        [MinLength(3, ErrorMessage = "Min Length Is 3 Letters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Name Is Required")]
        [MinLength(3, ErrorMessage = "Min Length Is 3 Letters")]
        public string NameAr { get; set; } = "";

        public IFormFile? ImageUrl { get; set; }

    }
}
