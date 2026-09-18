namespace Resturant_Backend.DTO.Cart
{
    public class GetCartDto
    {
        public int Id { get; set; }
        public string AppuserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }



        public string? ProductName { get; set; }
        public string? ProductNameAr { get; set; }
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public int? ProductPreparingTime { get; set; }
        public int? ProductSellCount { get; set; }
        public bool ProductIsAvailable { get; set; }
    }
}
