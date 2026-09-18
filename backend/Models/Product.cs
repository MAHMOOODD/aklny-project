namespace Resturant_Backend.Models
{
    public class Product
    {

        public int Id { get; set; }


        public string Name { get; set; }

        public string NameAr { get; set; } = "";

        public string Description { get; set; }
        public string DescriptionAr { get; set; } = "";



        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }


        public int PreparingTime { get; set; } = 18;// in minutes

        public bool IsAvailable { get; set; }

        public int SellCount { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<OrderDetails> Details { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Cart_Item> CartItems { get; set; }


    }
}
