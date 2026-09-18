using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.Cart;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;
using System.Security.Claims;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]

    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]
        [HttpGet("GetCart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Ensure.Unauthorized(userId, message: "غير مصرح لك بالوصول، يرجى تسجيل الدخول.");

            var cartItems = _unitOfWork.CartRepo.GetAllCartItems(userId!);

            Ensure.NotNull(cartItems, "Cart was not found.");
            if(!cartItems.Any())
            {
                return this.Success(cartItems);

            }


            var cartItemsToShow = _mapper.Map<List<GetCartDto>>(cartItems);
            return this.Success(cartItemsToShow);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]
        [HttpPost("AddToCart/{productId:int}")]
        public async Task<IActionResult> AddToCart(int productId, EditCartItemDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Ensure.Unauthorized(userId, message: "غير مصرح لك بالوصول، يرجى تسجيل الدخول.");

            // check if the product exists in the database
            var product = await _unitOfWork.ProductsRepo.GetAsync(productId);
            Ensure.NotNull(product, message: "Product Not Found");

            // check if the item already exists in the cart for the user
            var exist = await _unitOfWork.CartRepo.IsItemExist(productId, userId!);
            Ensure.Check(!exist, "Item already exist in your cart.");

            var addDto = new AddToCartDto
            {
                AppuserId = userId!,
                ProductId = productId,
                Quantity = dto.Quantity
            };

            var cartItem = _mapper.Map<Cart_Item>(addDto);
            await _unitOfWork.CartRepo.AddAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return this.Success(addDto);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]
        [HttpPut("EditCartItem/{cartItemId:int}")]
        public async Task<IActionResult> EditCartItem(int cartItemId, EditCartItemDto dto)
        {
            var cartitem = await _unitOfWork.CartRepo.GetAsync(cartItemId);
            Ensure.NotNull(cartitem, "Cart Item Not Found");

            cartitem.Quantity = dto.Quantity;
            await _unitOfWork.SaveChangesAsync();

            return this.Success(dto);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]
        [HttpDelete("DeleteCartItem/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cartItem = await _unitOfWork.CartRepo.DeleteAsync(id);
            Ensure.NotNull(cartItem, "Failed to delete cart item");

            await _unitOfWork.SaveChangesAsync();

            return this.SuccessMessage("Cart item deleted successfully.");
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.User}")]
        [HttpDelete("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Ensure.Unauthorized(userId, "غير مصرح لك بالوصول، يرجى تسجيل الدخول.");

            var deleted = await _unitOfWork.CartRepo.ClearCart(userId!);
            await _unitOfWork.SaveChangesAsync();
            Ensure.Check(deleted, "cart is already empty");

            return this.SuccessMessage("Cart cleared successfully.");
        }
    }
}