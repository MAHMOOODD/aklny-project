using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.DTO.Dashboard;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Role.Admin},{Role.Manager}")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Appuser> _userManager;

        public DashboardController(IUnitOfWork unitOfWork, UserManager<Appuser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: /api/Dashboard/Overview?trendDays=14
        [HttpGet("Overview")]
        public async Task<IActionResult> GetOverview([FromQuery] int trendDays = 14)
        {
            var revenue = await _unitOfWork.OrderRepo.GetRevenueSummaryAsync();
            var orders = await _unitOfWork.OrderRepo.GetOrdersSummaryAsync();
            var revenueTrend = await _unitOfWork.OrderRepo.GetRevenueTrendAsync(trendDays);
            var topProducts = await _unitOfWork.ProductsRepo.GetTopSellingProductsAsync(5);

            var totalUsers = await _unitOfWork.UserRepo.GetTotalUsersCountAsync();
            var admins = await _userManager.GetUsersInRoleAsync(Role.Admin);
            var managers = await _userManager.GetUsersInRoleAsync(Role.Manager);
            var regularUsers = await _userManager.GetUsersInRoleAsync(Role.User);

            var dto = new DashboardOverviewDto
            {
                Revenue = revenue,
                Orders = orders,
                Users = new UsersSummaryDto
                {
                    TotalUsers = totalUsers,
                    Admins = admins.Count,
                    Managers = managers.Count,
                    RegularUsers = regularUsers.Count,
                },
                TopProducts = topProducts,
                RevenueTrend = revenueTrend,
            };

            return this.Success(dto);
        }
    }
}
