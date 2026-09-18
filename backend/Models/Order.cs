namespace Resturant_Backend.Models
{

    public class Order
    {

        public int Id { get; set; }

        public string? AppuserId { get; set; }
        public Appuser Appuser { get; set; }

        public string UserAddress { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; }



        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CouponId { get; set; }
        public string? LastModifiedBy { get; set; }

        public Coupon? Coupon { get; set; }


        public decimal? Discount { get; set; }


        public int? PaymobOrderId { get; set; }
        public string? TransactionId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public decimal AmountPaid { get; set; }
        public OrderSource Source { get; set; } = OrderSource.Online;





        public List<OrderDetails> OrderDetails { get; set; }




    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded,

    }
    public enum OrderSource
    {
        Online = 0,
        POS = 1
    }
}
