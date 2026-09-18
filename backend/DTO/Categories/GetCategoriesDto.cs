using Resturant_Backend.DTO.Products;

namespace Resturant_Backend.DTO.Categories
{
    public class GetCategoriesDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string NameAr { get; set; }

        public string ImageUrl { get; set; }
        public List<GetProductDto> Products { get; set; } = new();
    }
}
