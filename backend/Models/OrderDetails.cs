namespace Resturant_Backend.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string? ProductName { get; set; }
        public string? ProductNameAr { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}
