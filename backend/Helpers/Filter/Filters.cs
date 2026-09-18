using Resturant_Backend.Helpers.Pagination;

namespace Resturant_Backend.Helpers.Filter
{
    public class Filters
    {

        public PaginationFilter Pagination { get; set; } = new PaginationFilter();

        public string? SearchTerm { get; set; } = null;

        public bool? SortByPrice { get; set; } = null;

        public bool? SortBySelling { get; set; } = null;

        public bool Ascending { get; set; } = true;

        public decimal? MinPrice { get; set; } = null;

        public decimal? MaxPrice { get; set; } = null;
    }
    public class FiltersOrders
    {

        public PaginationFilter Pagination { get; set; } = new PaginationFilter();

        public string? SearchTerm { get; set; } = null;


        public bool? SortByPrice { get; set; } = null;

        public bool? SortByDate { get; set; } = null;

        public bool Ascending { get; set; } = true;

        public int? OrderStatus { get; set; } = null;
        public int? PaymentState { get; set; } = null;
    }
    public class FiltersUsers
    {

        public PaginationFilter Pagination { get; set; } = new PaginationFilter();


        public bool? UserName { get; set; } = null;

        public string? SearchTerm { get; set; } = null;
        public bool Ascending { get; set; } = true;
    }
}
