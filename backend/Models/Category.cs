namespace Resturant_Backend.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string NameAr { get; set; } = "";

        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }

        public List<Product> Products { get; set; }

    }
}
