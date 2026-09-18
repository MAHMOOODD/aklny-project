namespace Resturant_Backend.DTO.Products
{
    public class GetAllProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; } = "";

        public string Description { get; set; }
        public string DescriptionAr { get; set; } = "";

        public decimal Price { get; set; }


        public int PreparingTime { get; set; } // in minutes



        public string ImageUrl { get; set; }
        public int SellCount { get; set; }


        public bool IsAvailable { get; set; }

        public int CategoryId { get; set; }

    }
}
