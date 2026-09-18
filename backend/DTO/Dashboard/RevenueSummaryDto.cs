namespace Resturant_Backend.DTO.Dashboard
{
    public class RevenueSummaryDto
    {
        // total revenue before applying any coupons or discounts, i.e., the sum of all completed orders' total amounts
        public decimal TotalRevenue { get; set; }

        // net revenue after applying coupons and discounts, i.e., the sum of all completed orders' total amounts minus any discounts applied
        public decimal NetRevenue { get; set; }

        // average revenue per order, calculated as NetRevenue divided by the number of completed orders
        public decimal AverageOrderValue { get; set; }
    }
}
