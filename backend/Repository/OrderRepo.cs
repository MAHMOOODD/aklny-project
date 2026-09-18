using Microsoft.EntityFrameworkCore;
using Resturant_Backend.Data;
using Resturant_Backend.DTO.Dashboard;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;

namespace Resturant_Backend.Repository
{
    public class OrderRepo : Repository<Order>, IOrder
    {
        private readonly AppDbContext _context;
        public OrderRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<(List<Order> Orders, int TotalCount)> GetAllAsync(FiltersOrders filters)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            query = applyOrderState(query, filters.OrderStatus);
            query = applyPaymentState(query, filters.PaymentState);

            if(!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var term = filters.SearchTerm.Trim();
                query = query.Where(o =>
                    o.Id.ToString().Contains(term)
                    || o.UserAddress.Contains(term)
                    || _context.Users.Any(u => u.Id == o.AppuserId &&
                         ( u.FullName.Contains(term) || u.UserName.Contains(term) )));
            }

            var totalCount = await query.CountAsync();

            var sortedQuery = SortOrdersBy(query, filters.SortByPrice, filters.SortByDate, filters.Ascending);

            var pagedOrders = await sortedQuery
                .Skip(( filters.Pagination.PageNumber - 1 ) * filters.Pagination.PageSize)
                .Take(filters.Pagination.PageSize)
                .ToListAsync();

            return (pagedOrders, totalCount);
        }

        public override async Task<Order?> GetAsync(int id)
        {

            return await _context.Orders.Include(c => c.OrderDetails)
                .ThenInclude(od => od.Product).FirstOrDefaultAsync(o => o.Id == id);
        }


        public List<Order> GetUserOrders(string id)
        {

            return _context.Orders.Include(c => c.OrderDetails).ThenInclude(od => od.Product).Where(c => c.AppuserId == id).OrderByDescending(c => c.CreatedAt).ToList();

        }






        public IQueryable<Order> SortOrdersBy(IQueryable<Order> orders, bool? SortByPrice, bool? SortBydate, bool ascending)
        {

            if(SortByPrice == true && SortBydate == true)
            {
                return ascending ? orders.OrderBy(p => p.TotalPrice).ThenBy(p => p.CreatedAt)
                    : orders.OrderByDescending(p => p.TotalPrice).ThenBy(p => p.CreatedAt);



            }
            if(SortByPrice == true)
            {
                return ascending ? orders.OrderBy(p => p.TotalPrice) : orders.OrderByDescending(p => p.TotalPrice);
            }

            if(SortBydate == true)
            {
                return ascending ? orders.OrderBy(p => p.CreatedAt) : orders.OrderByDescending(p => p.CreatedAt);
            }

            return orders;



        }

        public Task<int> GetProductCountAsync()
        {
            return _context.Orders.CountAsync();
        }

        public async Task<string?> LastModifiedBy(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.FullName ?? user?.UserName ?? null;
        }




        public IQueryable<Order> applyOrderState(IQueryable<Order> query, int? orderState)
        {
            switch(orderState)
            {
                case 0:
                    return query.Where(c => c.Status == OrderStatus.Pending);
                case 1:
                    return query.Where(c => c.Status == OrderStatus.Processing);
                case 2:
                    return query.Where(c => c.Status == OrderStatus.Shipped);
                case 3:
                    return query.Where(c => c.Status == OrderStatus.Delivered);
                case 4:
                    return query.Where(c => c.Status == OrderStatus.Cancelled);
                default:
                    return query;



            }


        }
        public IQueryable<Order> applyPaymentState(IQueryable<Order> query, int? paymentState)
        {
            switch(paymentState)
            {
                case 0:
                    return query.Where(c => c.PaymentStatus == PaymentStatus.Pending);
                case 1:
                    return query.Where(c => c.PaymentStatus == PaymentStatus.Paid);
                case 2:
                    return query.Where(c => c.PaymentStatus == PaymentStatus.Failed);
                case 3:
                    return query.Where(c => c.PaymentStatus == PaymentStatus.Refunded);


                default:
                    return query;




            }


        }


        // ============ Dashboard Analytics — new ============

        /// <summary>
        /// used to get the total revenue, net revenue (after discounts),
        /// and average order value for all delivered orders.
        /// also calculates the total discount amount by summing up the discount applied to each order.
        /// 
        /// </summary>
        public async Task<RevenueSummaryDto> GetRevenueSummaryAsync()
        {
            var summary = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .GroupBy(o => 1)
                .Select(g => new
                {
                    DeliveredCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalPrice),
                    // transform the discount percentage into a decimal and multiply by the total price to get the discount 
                    TotalDiscountAmount = g.Sum(o => o.TotalPrice * ( ( o.Discount ?? 0 ) / 100m ))
                })
                .FirstOrDefaultAsync();

            if(summary == null || summary.DeliveredCount == 0)
            {
                return new RevenueSummaryDto
                {
                    TotalRevenue = 0,
                    NetRevenue = 0,
                    AverageOrderValue = 0
                };
            }

            var netRevenue = summary.TotalRevenue - summary.TotalDiscountAmount;
            var averageOrderValue = summary.TotalRevenue / summary.DeliveredCount;

            return new RevenueSummaryDto
            {
                TotalRevenue = summary.TotalRevenue,
                NetRevenue = netRevenue,
                AverageOrderValue = averageOrderValue
            };
        }

        /// <summary>
        /// number of orders in each status (Pending, Processing, Shipped, Delivered, Cancelled) and the total number of orders.
        /// </summary>
        public async Task<OrdersSummaryDto> GetOrdersSummaryAsync()
        {
            var counts = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int CountOf(OrderStatus status) =>
                counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

            return new OrdersSummaryDto
            {
                TotalOrders = counts.Sum(c => c.Count),
                Pending = CountOf(OrderStatus.Pending),
                Processing = CountOf(OrderStatus.Processing),
                Shipped = CountOf(OrderStatus.Shipped),
                Delivered = CountOf(OrderStatus.Delivered),
                Cancelled = CountOf(OrderStatus.Cancelled),
            };
        }

        /// <summary>
        /// order revenue trend for the last N days,
        /// including days with no orders (returns zero for those days to keep the chart continuous).
        ///
        /// </summary>
        public async Task<List<RevenuePointDto>> GetRevenueTrendAsync(int days)
        {
            if(days < 1)
                days = 14;

            var startDate = DateTime.Now.Date.AddDays(-( days - 1 ));

            var raw = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt.Date >= startDate)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalPrice),
                    OrdersCount = g.Count(),
                })
                .ToListAsync();

            var result = new List<RevenuePointDto>();
            for(var day = startDate ; day <= DateTime.Now.Date ; day = day.AddDays(1))
            {
                var match = raw.FirstOrDefault(r => r.Date == day);
                result.Add(new RevenuePointDto
                {
                    Date = day,
                    Revenue = match?.Revenue ?? 0,
                    OrdersCount = match?.OrdersCount ?? 0,
                });
            }

            return result;
        }

    }
}
