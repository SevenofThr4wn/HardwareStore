using HardwareStore.Data.Context;
using HardwareStore.WebClient.Models;
using HardwareStore.WebClient.Services;
using HardwareStore.WebClient.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public DashboardController(AppDbContext context,
            IUserService userService,
            IProductService productService,
            IOrderService orderService)
        {
            _context = context;
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AdminDashboard()
        {
            var dashboardData = new AdminDashboardVM
            {
                TotalUsers = await _userService.GetTotalUsers(),
                TotalProducts = await _productService.GetTotalProducts(),
                TotalOrders = await _orderService.GetTodaysOrders(),
                TotalRevenue = await _orderService.GetTotalRevenue(),

                QuickActions = new List<QuickAction>
                {
                    new QuickAction
                    {
                        Title = "Manage Users",
                        Description = "View and manage user accounts",
                        Icon = "bi bi-people-fill",
                        Url = Url.Action("ManageUsers")!
                    },
                    new QuickAction
                    {
                        Title = "Manage Products",
                        Description = "Add, edit, or remove products",
                        Icon = "bi bi-box-seam",
                        Url = Url.Action("ManageProducts")!
                    },
                    new QuickAction
                    {
                        Title = "Manage Orders",
                        Description = "Process and track orders",
                        Icon = "bi bi-cart-check",
                        Url = Url.Action("ManageOrders")!
                    },
                    new QuickAction
                    {
                        Title = "Admin Settings",
                        Description = "System configuration",
                        Icon = "bi bi-gear-fill",
                        Url = Url.Action("AdminSettings")!
                    }
                },

                RecentActivity = await _orderService.GetRecentActivities(5)
            };
            return View(dashboardData);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManagerDashboard()
        {
            // List of staff roles used to count the total staff members in the system
            var staffRoles = new[] { "Staff", "Admin", "Manager" };

            var dashboardData = new ManagerDashboardVM
            {
                // Counts the total products in the system 
                TotalProducts = await _productService.GetTotalProducts(),
                LowStockProducts = await _productService.GetLowStockCount(10),
                PendingOrders = await _orderService.GetPendingOrdersCount(),

                TotalStaff = await _userService.GetStaffCount(),
                LowStockItems = await _productService.GetLowStockItems(10),
            };
            return View(dashboardData);
        }
    }
}