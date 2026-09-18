using AutoMapper;
using Resturant_Backend.DTO.Cart;
using Resturant_Backend.DTO.Categories;
using Resturant_Backend.DTO.Coupon;
using Resturant_Backend.DTO.Order;
using Resturant_Backend.DTO.OrderDetails;
using Resturant_Backend.DTO.Products;
using Resturant_Backend.DTO.Review;
using Resturant_Backend.DTO.User;
using Resturant_Backend.Models;

namespace Resturant_Backend.Mapper
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<UserCreatedModel, ResponseRegister>().ReverseMap();
            CreateMap<UserCreatedModel, ResponseLogin>().ReverseMap();


            // categories Mapping
            CreateMap<Category, GetCategoriesDto>().ReverseMap();
            CreateMap<Category, AddCategoriesDto>().ReverseMap();
            CreateMap<Category, EditCategoriesDto>();
            CreateMap<EditCategoriesDto, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // طنش مابينج الصورة وسيب الكنترولر يهندلها
                .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));

            // Products Mapping
            CreateMap<Product, GetProductDto>();
            CreateMap<Product, GetAllProductDto>();
            CreateMap<Product, AddProductDto>().ReverseMap();

            // نقوم بإنشاء الـ Mapping لـ EditProductDto بشكل منفصل وبدون ReverseMap لمنع التضارب
            CreateMap<EditProductDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));



            //cart Mapping
            CreateMap<Cart_Item, AddToCartDto>().ReverseMap();
            CreateMap<Cart_Item, GetCartDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : null))
                .ForMember(d => d.ProductNameAr, opt => opt.MapFrom(s => s.Product != null ? s.Product.NameAr : null))
                .ForMember(d => d.ProductImageUrl, opt => opt.MapFrom(s => s.Product != null ? s.Product.ImageUrl : null))
                .ForMember(d => d.ProductPrice, opt => opt.MapFrom(s => s.Product != null ? s.Product.Price : 0))
                .ForMember(d => d.ProductPreparingTime, opt => opt.MapFrom(s => s.Product != null ? s.Product.PreparingTime : (int?)null))
                .ForMember(d => d.ProductSellCount, opt => opt.MapFrom(s => s.Product != null ? s.Product.SellCount : (int?)null))
                .ForMember(d => d.ProductIsAvailable, opt => opt.MapFrom(s => s.Product != null && s.Product.IsAvailable));

            CreateMap<GetCartDto, Cart_Item>();

            //Coupon Mapping
            CreateMap<Coupon, GetCouponDto>().ReverseMap();
            CreateMap<Coupon, EditCouponDto>().ReverseMap();
            CreateMap<Coupon, AddCouponDto>().ReverseMap();

            //Review Mapping

            CreateMap<Review, GetReviewDto>().ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Appuser.UserName))
                .ForMember(dest => dest.UserImage, opt => opt.MapFrom(src => src.Appuser.ImageUrl));
            CreateMap<GetReviewDto, Review>();
            CreateMap<Review, AddReviewDto>().ReverseMap();
            CreateMap<Review, EditReviewDto>().ReverseMap();


            // Order Mapping


            CreateMap<ResponseAddDto, Order>().ReverseMap();
            CreateMap<Order, GetOrderDto>();


            CreateMap<Order, GetOrderDto>()
    .ForMember(d => d.Change, opt => opt.MapFrom(s => s.AmountPaid - s.TotalPrice));

            // Order Details Mapping
            CreateMap<OrderDetails, GetDetailsDto>()
     .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.ProductName))
     .ForMember(d => d.ProductNameAr, opt => opt.MapFrom(s => s.ProductNameAr))
     .ForMember(d => d.ProductImageUrl, opt => opt.MapFrom(s => s.ProductImageUrl));

            //user Mapping


            CreateMap<Appuser, GetUserInfo>();
        }


    }
}
