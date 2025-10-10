using HardwareStore.WebClient.Models;
using HardwareStore.WebClient.Services;
using HardwareStore.WebClient.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{
    /// <summary>
    /// Handles dashboard rendering and logic for administrative and managerial users.
    /// Provides access to system-wide statistics, metrics, and quick actions.
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="userService">The service responsible for user data retrieval.</param>
        /// <param name="productService">The service responsible for product data retrieval.</param>
        /// <param name="orderService">The service responsible for order data retrieval and statistics.</param>
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
        /// Displays the Admin Dashboard containing global statistics, 
        /// quick access links, and recent activity logs.
        /// </summary>
        /// <remarks>
        /// This action is accessible only to users with the <c>Admin</c> or <c>Manager</c> roles.
        /// </remarks>
        /// <returns>
        /// A view populated with an <see cref="AdminDashboardVM"/> instance containing dashboard data.
        /// </returns>
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
        /// Displays the Manager Dashboard containing staff, product, and order-related insights.
        /// </summary>
        /// <remarks>
        /// This action provides key operational metrics such as stock levels, 
        /// pending orders, and staff counts. Accessible by users with 
        /// <c>Admin</c> or <c>Manager</c> roles.
        /// </remarks>
        /// <returns>
        /// A view populated with a <see cref="ManagerDashboardVM"/> instance containing managerial data.
        /// </returns>
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