namespace Resturant_Backend.DTO.OrderDetails
{
    public class GetDetailsDto
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }



        // Additional properties for product details
        public string? ProductName { get; set; }
        public string? ProductNameAr { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}
