using HardwareStore.WebClient.Models;
using HardwareStore.WebClient.Services;
using HardwareStore.WebClient.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{
    /// <summary>
    /// Handles dashboard views for different user roles (Admin, Manager).
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public DashboardController(
            IUserService userService,
            IProductService productService,
            IOrderService orderService)
        {
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
        }

        /// <summary>
        /// Displays the admin dashboard with system-wide statistics,
        /// quick actions, and recent activity.
        /// Accessible by Admins and Managers.
        /// </summary>
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
                        Url = "/Users/ManageUsers"
                    },
                    new QuickAction
                    {
                        Title = "Manage Products",
                        Description = "Add, edit, or remove products",
                        Icon = "bi bi-box-seam",
                        Url = "/Products/ManageProducts"
                    },
                    new QuickAction
                    {
                        Title = "Manage Orders",
                        Description = "Process and track orders",
                        Icon = "bi bi-cart-check",
                        Url = "/Orders/ManageOrders"
                    },
                    new QuickAction
                    {
                        Title = "Admin Settings",
                        Description = "System configuration",
                        Icon = "bi bi-gear-fill",
                        Url = "/Settings/AdminSettings"
                    }
                },

                RecentActivity = await _orderService.GetRecentActivities(5)
            };
            return View(dashboardData);
        }

        /// <summary>
        /// Displays the manager dashboard with product, stock, staff,
        /// and order-related metrics.
        /// Accessible by Admins and Managers.
        /// </summary>
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManagerDashboard()
        {
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