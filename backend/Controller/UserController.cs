using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.User;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Helpers.Pagination;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;
using System.Security.Claims;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        private readonly UserManager<Appuser> _userManager;



        public UserController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<Appuser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }


        [HttpGet("IsAuth")]
        [Authorize]
        public async Task<IActionResult> CheckAuth()
        {
            return this.SuccessMessage("Authenticated");

        }
        [Authorize]
        [HttpGet("UserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Ensure.Unauthorized(userId, "غير مصرح لك بالوصول، يرجى تسجيل الدخول.");
            var user = await _unitOfWork.UserRepo.GetUserInformationAsync(userId);
            var userToShow = _mapper.Map<GetUserInfo>(user);
            userToShow.Roles = ( await _userManager.GetRolesAsync(user!) ).ToList();

            return this.Success(userToShow);

        }





        [Authorize(Roles = $"{Role.Admin},{Role.Manager},{Role.Cashier}")]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] FiltersUsers filters)
        {
            var validFilter = new PaginationFilter(filters.Pagination.PageNumber, filters.Pagination.PageSize);
            var (users, totalCount) = await _unitOfWork.UserRepo.GetUsersAsync(filters);

            var usersToShow = new List<GetUserInfo>();
            foreach(var user in users)
            {
                var dto = _mapper.Map<GetUserInfo>(user);
                dto.Roles = ( await _userManager.GetRolesAsync(user) ).ToList();
                usersToShow.Add(dto);
            }

            var response = new PagedResponse<GetUserInfo>(usersToShow, validFilter.PageNumber, validFilter.PageSize, totalCount);
            return this.Success(response);
        }
        [Authorize(Roles = $"{Role.Admin},{Role.Manager}.{Role.Cashier}")]
        [HttpGet("GetUserbyId/{userId}")]
        public async Task<IActionResult> GetUserbyId(string userId)
        {
            var user = await _unitOfWork.UserRepo.GetUserInformationAsync(userId);

            if(user is null)
            {
                this.NotFoundEx("User not found");
            }


            var userToShow = _mapper.Map<GetUserInfo>(user);
            userToShow.Roles = ( await _userManager.GetRolesAsync(user!) ).ToList();
            return this.Success(userToShow);


        }


    }
}
