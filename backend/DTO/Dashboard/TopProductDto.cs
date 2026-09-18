namespace Resturant_Backend.DTO.Dashboard
{
    public class TopProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int SellCount { get; set; }
        public decimal Price { get; set; }
    }
}
