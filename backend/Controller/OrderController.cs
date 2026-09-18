using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.Order;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Helpers.Pagination;
using Resturant_Backend.Hubs;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;
using System.Security.Claims;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderController(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<OrderHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;

        }
        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]


        [HttpPost("Add")]
        public async Task<IActionResult> AddOrder(AddOrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Ensure.Unauthorized(userId, "غير مصرح لك بالوصول، يرجى تسجيل الدخول.");

            var (address, found) = await _unitOfWork.UserRepo.GetUserAddressAsync(userId);


            Ensure.Check(found || string.IsNullOrEmpty(dto.UserAddress), "You must provide an address.");



            var cartItems = _unitOfWork.CartRepo.GetAllCartItems(userId);

            Ensure.Check(cartItems.Any(), "Cart is Empty");


            var totalPrice = cartItems.Sum(c => c.Quantity * c.Product.Price);
            var priceAfterDiscount = 0M;

            decimal Discount = 0;
            if(dto.Coupon is not null)
            {
                var (message, coupon, discount) = await _unitOfWork.CouponRepo.ValidateCoupon(dto.Coupon, totalPrice);

                Ensure.Check(coupon, message);

                if(coupon)
                {
                    priceAfterDiscount = totalPrice - ( ( discount / 100 ) * totalPrice );
                    Discount = discount;
                }
            }

            var Coupon = await _unitOfWork.CouponRepo.GetCoupon(dto.Coupon);

            var Order = new Order
            {
                AppuserId = userId,
                UserAddress = string.IsNullOrEmpty(address) ? dto.UserAddress : address,
                TotalPrice = totalPrice,
                Discount = Discount,
                CouponId = Coupon?.Id ?? null,
                PaymentStatus = PaymentStatus.Pending,
                Status = OrderStatus.Pending,
                OrderDetails = cartItems.Select(c => new OrderDetails
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Product.Price,
                    ProductImageUrl = c.Product.ImageUrl,
                    ProductName = c.Product.Name,
                    ProductNameAr = c.Product.NameAr


                }).ToList()

            };
            var or = await _unitOfWork.OrderRepo.AddAsync(Order);


            foreach(var item in cartItems)
            {
                item.Product.SellCount += item.Quantity;
            }

            await _unitOfWork.CartRepo.ClearCart(userId);

            await _unitOfWork.SaveChangesAsync();
            var OrderToshow = _mapper.Map<ResponseAddDto>(Order);
            OrderToshow.PriceAfterDiscount = priceAfterDiscount == 0 ? totalPrice : priceAfterDiscount;
            OrderToshow.Coupon = dto.Coupon ?? "";

            return this.Success(OrderToshow);

        }


        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]

        [HttpGet("MyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Ensure.Unauthorized(userId, "يجب تسجيل الدخول اولا");

            var orders = _unitOfWork.OrderRepo.GetUserOrders(userId);

            Ensure.Check(orders is not null && orders.Count > 0, "Make an order First");

            var ordersToShow = _mapper.Map<List<GetOrderDto>>(orders);

            return this.Success(ordersToShow);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.Cashier}")]

        [HttpGet("Get{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _unitOfWork.OrderRepo.GetAsync(id);
            Ensure.NotNull(order, "Order does not Exist");

            var orderToShow = _mapper.Map<GetOrderDto>(order);

            return this.Success(orderToShow);
        }
        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.Cashier}")]

        [HttpGet("Get")]
        public async Task<IActionResult> GetOrders([FromQuery] FiltersOrders filters)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var validFilter = new PaginationFilter(filters.Pagination.PageNumber, filters.Pagination.PageSize);
            var (orders, totalCount) = await _unitOfWork.OrderRepo.GetAllAsync(filters);
            Ensure.NotNull(orders, "There is no orders in the system");

            var ordersfromDb = _mapper.Map<List<GetOrderDto>>(orders);

            var ordersToShow = new PagedResponse<GetOrderDto>(ordersfromDb, validFilter.PageNumber, validFilter.PageSize, totalCount);


            return this.Success(ordersToShow);
        }



        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.Cashier}")]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, StatusResponseDto dto)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var lastModifiedBy = await _unitOfWork.OrderRepo.LastModifiedBy(userId);

            var order = await _unitOfWork.OrderRepo.GetAsync(id);

            Ensure.NotNull(order, $"Order with ID {id} was not found.");

            order.LastModifiedBy = lastModifiedBy;

            bool wasOrderActive = order.Status != OrderStatus.Cancelled
                               && order.PaymentStatus != PaymentStatus.Failed;

            if(dto.Status.HasValue)
                order.Status = dto.Status.Value;

            if(dto.PaymentStatus.HasValue)
                order.PaymentStatus = dto.PaymentStatus.Value;

            bool isOrderNowCancelledOrFailed = order.Status == OrderStatus.Cancelled
                                            || order.PaymentStatus == PaymentStatus.Failed;

            if(wasOrderActive && isOrderNowCancelledOrFailed)
            {
                if(order.OrderDetails is not null)
                {
                    foreach(var detail in order.OrderDetails)
                    {
                        if(detail.Product is not null)
                        {
                            detail.Product.SellCount -= detail.Quantity;
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();


            var orderToShow = _mapper.Map<GetOrderDto>(order);


            // send a notification to the user if the order belongs to them

            await _hubContext.Clients.Group($"user-{order.AppuserId}")
     .SendAsync("OrderStatusUpdated", orderToShow);

            if(userId != order.AppuserId)
            {
                await _hubContext.Clients.Group("admins")
                    .SendAsync("OrderStatusUpdated", orderToShow);
            }


            return this.Success(orderToShow);
        }




        [Authorize(Roles = $"{Role.Cashier},{Role.Admin},{Role.Manager}")]
        [HttpPost("AddPosOrder")]
        public async Task<IActionResult> AddPosOrder(AddPosOrderDto dto)
        {
            var cashierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrEmpty(cashierId))
            {
                this.UnauthorizedEx("غير مصرح لك بالوصول، يرجى تسجيل الدخول.");
            }

            bool hasCustomerId = !string.IsNullOrWhiteSpace(dto.CustomerId);
            bool hasGuestInfo = !string.IsNullOrWhiteSpace(dto.GuestName);
            if(!hasGuestInfo && !hasCustomerId)
            {
                this.UnauthorizedEx(" ادخل بيانات العميل المسجل أو اسم الزائر.");
            }

            if(hasCustomerId)
            {
                var customer = await _unitOfWork.UserRepo.GetUserInformationAsync(dto.CustomerId!);
                Ensure.NotNull(customer, "العميل غير موجود.");
            }

            if(dto.Items is null || !dto.Items.Any())
            {
                this.BadRequestEx("يجب إدخال منتجات للفاتورة.");
            }

            var orderDetails = new List<OrderDetails>();
            var productsCache = new Dictionary<int, Product>();
            decimal totalPrice = 0;

            foreach(var item in dto.Items!)
            {
                var product = await _unitOfWork.ProductsRepo.GetAsync(item.ProductId);
                if(product is null)
                {
                    this.NotFoundEx($"المنتج بالمعرف {item.ProductId} غير موجود.");
                }

                productsCache[item.ProductId] = product!;
                orderDetails.Add(new OrderDetails
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product!.Price,
                    ProductImageUrl = product.ImageUrl,
                    ProductName = product.Name,
                    ProductNameAr = product.NameAr
                });
                totalPrice += item.Quantity * product.Price;
            }

            decimal discount = 0;
            decimal finalTotal = totalPrice;
            Coupon? coupon = null;

            if(!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var (message, isValid, discountPercent) = await _unitOfWork.CouponRepo.ValidateCoupon(dto.CouponCode, totalPrice);
                if(!isValid)
                {
                    this.BadRequestEx(message);
                }

                discount = discountPercent;
                finalTotal = totalPrice - ( discountPercent / 100 * totalPrice );
                coupon = await _unitOfWork.CouponRepo.GetCoupon(dto.CouponCode!);
            }

            if(dto.AmountPaid < finalTotal)
            {
                this.BadRequestEx("المبلغ المدفوع أقل من إجمالي الفاتورة.");
            }

            var cashierName = await _unitOfWork.OrderRepo.LastModifiedBy(cashierId!);

            var order = new Order
            {
                AppuserId = hasCustomerId ? dto.CustomerId : null,
                GuestName = hasCustomerId ? null : dto.GuestName,
                GuestPhone = hasCustomerId ? null : dto.GuestPhone,
                UserAddress = "استلام من الفرع",
                TotalPrice = finalTotal,
                Discount = discount,
                CouponId = coupon?.Id,
                AmountPaid = dto.AmountPaid,
                Source = OrderSource.POS,
                PaymentStatus = PaymentStatus.Paid,
                Status = OrderStatus.Delivered,
                LastModifiedBy = cashierName,
                OrderDetails = orderDetails
            };

            var addedOrder = await _unitOfWork.OrderRepo.AddAsync(order);

            foreach(var item in dto.Items)
                productsCache[item.ProductId].SellCount += item.Quantity;

            await _unitOfWork.SaveChangesAsync();

            var orderToShow = _mapper.Map<GetOrderDto>(addedOrder);

            await _hubContext.Clients.Group("admins").SendAsync("NewOrderCreated", orderToShow);

            return this.Success(orderToShow);
        }

    }
}
