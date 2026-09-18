using System.Collections.Generic;

namespace Resturant_Backend.DTO.Dashboard
{
    public class DashboardOverviewDto
    {
        public RevenueSummaryDto Revenue { get; set; } = new();
        public OrdersSummaryDto Orders { get; set; } = new();
        public UsersSummaryDto Users { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<RevenuePointDto> RevenueTrend { get; set; } = new();
    }
}
