namespace Resturant_Backend.DTO.Dashboard
{
    public class OrdersSummaryDto
    {
        public int TotalOrders { get; set; }
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Shipped { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
    }
}
