using HardwareStore.Core.Enums;
using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.WebClient.Models;
using HardwareStore.WebClient.ViewModels;
using HardwareStore.WebClient.ViewModels.Account;
using HardwareStore.WebClient.ViewModels.Account.Management;
using HardwareStore.WebClient.ViewModels.Account.Operations;
using HardwareStore.WebClient.ViewModels.Account.Settings;
using HardwareStore.WebClient.ViewModels.Orders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HardwareStore.WebClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AccountController(HttpClient httpClient, IConfiguration configuration, AppDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
        }

        #region Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var token = await GetKeycloakTokenAsync(model.Username, model.Password);

                if (token == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                    return View(model);
                }

                // Validate JWT and extract claims
                var claims = await ValidateJwtAndExtractClaims(token);
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                // Debug: roles
                var userRoles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                Console.WriteLine($"User {model.Username} logged in with roles: {string.Join(", ", userRoles)}");

                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Login failed: {ex.Message}");
                return View(model);
            }
        }


        #endregion

        #region Register

        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var success = await CreateKeycloakUserAsync(viewModel);
                if (success) return RedirectToAction("Login", new { message = "Registration successful. Please login." });

                ModelState.AddModelError(string.Empty, "Registration failed");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                return View(viewModel);
            }
        }

        #endregion

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        #region Profile

        [Authorize]
        public IActionResult Profile()
        {
            var userProfile = new ProfileViewModel
            {
                DisplayName = User.Identity!.Name!,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email")!,
                FullName = $"{User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue("given_name")} " +
                $"{User.FindFirstValue(ClaimTypes.Surname) ?? User.FindFirstValue("family_name")}",
            };
            return View(userProfile);
        }

        #region Dashboards

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AdminDashboard()
        {
            var dashboardData = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == DateTime.Today),
                TotalRevenue = await _context.Orders
                    .Where(o => o.OrderDate.Date == DateTime.Today)
                    .SumAsync(o => o.TotalAmount),

                QuickActions = new List<QuickAction>
                {
                    new QuickAction { Title = "Manage Users", Description = "View and manage user accounts", Icon = "bi bi-people-fill", Url = Url.Action("ManageUsers")! },
                    new QuickAction { Title = "Manage Products", Description = "Add, edit, or remove products", Icon = "bi bi-box-seam", Url = Url.Action("ManageProducts")! },
                    new QuickAction { Title = "Manage Orders", Description = "Process and track orders", Icon = "bi bi-cart-check", Url = Url.Action("ManageOrders")! },
                    new QuickAction { Title = "Admin Settings", Description = "System configuration", Icon = "bi bi-gear-fill", Url = Url.Action("AdminSettings")! }
                },
                RecentActivity = await _context.ActivityLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .Select(a => new ActivityLog
                {
                    Icon = a.Icon,
                    Text = a.Text,
                    Timestamp = a.Timestamp,
                }).ToListAsync()
            };
            return View(dashboardData);

        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManagerDashboard()
        {
            var dashboardData = new ManagerDashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                LowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity < 10),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                TotalStaff = await _context.Users.CountAsync(u => u.Role == "Staff"),
                LowStockItems = await _context.Products
                    .Where(p => p.StockQuantity < 10)
                    .OrderBy(p => p.StockQuantity)
                    .Take(10)
                    .Select(p => new ProductStockInfo
                    {
                        Name = p.Name,
                        Stock = p.StockQuantity
                    })
                    .ToListAsync()
            };

            return View(dashboardData);
        }

        #endregion

        #region Users
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManageUsers(string searchString, string roleFilter, int page = 1, int pageSize = 10)
        {
            var usersQuery = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName!.Contains(searchString) ||
                    u.Email!.Contains(searchString) ||
                    u.FirstName!.Contains(searchString) ||
                    u.LastName!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                usersQuery = usersQuery.Where(u => u.Role == roleFilter);
            }

            var totalUsers = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserManagementViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    Email = u.Email!,
                    FirstName = u.FirstName!,
                    LastName = u.LastName!,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin,
                    CreatedDate = u.DateCreated
                })
                .ToListAsync();

            var availableRoles = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Role))
                .Select(u => u.Role)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.AvailableRoles = availableRoles;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            return View(users);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserEditModel
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    Email = u.Email!,
                    FirstName = u.FirstName!,
                    LastName = u.LastName!,
                    Role = u.Role,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            // Get available roles from database
            var availableRoles = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Role))
                .Select(u => u.Role)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            return Ok(new { user, availableRoles });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> EditUser(UserEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.DateCreated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "User updated successfully" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting own account
            if (user.UserName == User.Identity!.Name)
            {
                return BadRequest(new { success = false, message = "Cannot delete your own account" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "User deleted successfully" });
        }
            
        #endregion

        #region Products

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManageProducts(string searchString, string categoryFilter)
        {
            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchString) ||
                    p.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All")
            {
                productsQuery = productsQuery.Where(p => p.Category == categoryFilter);
            }

            var products = await productsQuery
                .OrderBy(p => p.Name)
                .Select(p => new ProductManagementViewModel
                {
                    Id = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CategoryFilter = categoryFilter;
            ViewBag.AvailableCategories = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            return View(products);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductEditModel
                {
                    Id = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    IsActive = p.IsActive
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.Id == 0)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    Category = model.Category,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Products.Add(product);
            }
            else
            {
                var product = await _context.Products.FindAsync(model.Id);
                if (product == null)
                {
                    return NotFound();
                }

                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.Category = model.Category;
                product.IsActive = model.IsActive;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Product saved successfully" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Product deleted successfully" });
        }

        #endregion

        #region Orders

        [Authorize]
        public async Task<IActionResult> MyOrders(OrderStatus? statusFilter = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);

            if (statusFilter.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == statusFilter.Value);
            }
            var orders = await ordersQuery
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNo,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ItemCount = o.OrderItems.Sum(oi => oi.Quantity),
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = (double)oi.UnitPrice,
                    }).ToList()
                })
                .ToListAsync();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.AvailableStatuses = Enum.GetValues<OrderStatus>();

            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = new OrderDetailsViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNo,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                Items = order.OrderItems.Select(oi => new OrderItemDetailsViewModel
                {
                    ProductName = oi.Product.Name,
                    ProductId = oi.Product.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.UnitPrice,
                    ImageUrl = oi.Product.ImageUrl,
                    Description = oi.Product.Description
                }).ToList(),
                User = new OrderUserViewModel
                {
                    UserName = order.User.Username,
                    Email = order.User.Email,
                    FirstName = order.User.FirstName,
                    LastName = order.User.LastName
                }
            };

            return View(orderDetails);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManageOrders(string statusFilter, DateTime? dateFilter)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                ordersQuery = ordersQuery.Where(o => o.Status.ToString() == statusFilter);
            }

            if (dateFilter.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date == dateFilter.Value.Date);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderManagementViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNo,
                    CustomerName = o.User.Username,
                    CustomerEmail = o.User.Email,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = (double)oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.DateFilter = dateFilter;
            ViewBag.AvailableStatuses = new[] { "All", "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            return View(orders);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Pending;
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Order status updated to {status}" });
        }

        #endregion

        #region Settings

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AdminSettings()
        {
            var settings = new AdminSettingsViewModel
            {
                StoreName = _configuration["StoreSettings:Name"] ?? "Hardware Store",
                StoreEmail = _configuration["StoreSettings:Email"] ?? "admin@hardwarestore.com",
                Currency = _configuration["StoreSettings:Currency"] ?? "USD",
                SessionTimeout = int.Parse(_configuration["SecuritySettings:SessionTimeout"] ?? "60"),
                PasswordPolicy = _configuration["SecuritySettings:PasswordPolicy"] ?? "Standard"
            };

            return View(settings);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult SaveAdminSettings(AdminSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AdminSettings", model);
            }

            TempData["SuccessMessage"] = "Settings saved successfully";
            return RedirectToAction("AdminSettings");
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult ManagerSettings()
        {
            var settings = new ManagerSettingsViewModel
            {
                LowStockNotifications = bool.Parse(_configuration["InventorySettings:LowStockNotifications"] ?? "true"),
                CriticalStockNotifications = bool.Parse(_configuration["InventorySettings:CriticalStockNotifications"] ?? "true"),
                LowStockThreshold = int.Parse(_configuration["InventorySettings:LowStockThreshold"] ?? "10"),
                AutoConfirmOrders = bool.Parse(_configuration["OrderSettings:AutoConfirmOrders"] ?? "true"),
                RequireManagerApproval = bool.Parse(_configuration["OrderSettings:RequireManagerApproval"] ?? "false")
            };

            return View(settings);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult SaveManagerSettings(ManagerSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ManagerSettings", model);
            }

            // Save settings logic here

            TempData["SuccessMessage"] = "Preferences saved successfully";
            return RedirectToAction("ManagerSettings");
        }

        #endregion

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = User.Identity!.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            // In a real application, you'd verify the current password and hash the new one
            // For now, we'll simulate password change

            TempData["SuccessMessage"] = "Password changed successfully";
            return RedirectToAction("Profile");
        }

        #endregion

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Authentication Methods

        private async Task<string?> GetKeycloakTokenAsync(string username, string password)
        {
            var keycloakUrl = _configuration["Keycloak:ServerUrl"];
            var realm = _configuration["Keycloak:Realm"];
            var clientId = _configuration["Keycloak:ClientId"];

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","password"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("username", username),
                new KeyValuePair<string,string>("password", password)
            });

            var response = await _httpClient.PostAsync($"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token", content);
            if (!response.IsSuccessStatusCode) return null;

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<bool> CreateKeycloakUserAsync(RegisterViewModel viewModel)
        {
            var token = await GetAdminTokenAsync();
            var keycloakUrl = _configuration["Keycloak:ServerUrl"];
            var realm = _configuration["Keycloak:Realm"];

            var userData = new
            {
                username = viewModel.Username,
                email = viewModel.Email,
                firstName = viewModel.FirstName,
                lastName = viewModel.LastName,
                enabled = true,
                credentials = new[]
                {
                    new { type = "password", value = viewModel.Password, temporary = false }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"{keycloakUrl}/admin/realms/{realm}/users", content);
            return response.IsSuccessStatusCode;
        }

        private async Task<List<Claim>> ValidateJwtAndExtractClaims(string token)
        {
            var authority = _configuration["Keycloak:Authority"];
            var clientId = _configuration["Keycloak:ClientId"];
            if (string.IsNullOrEmpty(authority)) throw new InvalidOperationException("Keycloak:Authority not configured");
            if (!authority.EndsWith("/")) authority += "/";

            var wellKnownUrl = new Uri(new Uri(authority), ".well-known/openid-configuration").AbsoluteUri;
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownUrl,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = false });

            var openIdConfig = await configManager.GetConfigurationAsync();
            var signingKeys = openIdConfig.SigningKeys;

            var handler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority.TrimEnd('/'),
                ValidateAudience = false,
                AudienceValidator = (audiences, token, param) =>
                {
                    var jwt = token as JwtSecurityToken;
                    if (jwt == null) return false;

                    var tokenAudiences = jwt.Audiences?.ToList() ?? new List<string>();

                    if (tokenAudiences.Contains(clientId!)) return true;
                    var azp = jwt.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
                    if (!string.IsNullOrEmpty(azp) && azp == clientId) return true;

                    var resourceAccess = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
                    if (!string.IsNullOrEmpty(resourceAccess))
                    {
                        using var doc = JsonDocument.Parse(resourceAccess);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(clientId!, out _))
                            return true;
                    }

                    return false;
                },
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,   // <--- this was false before
                IssuerSigningKeys = signingKeys,
                NameClaimType = "preferred_username",
                RoleClaimType = ClaimTypes.Role
            };


            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            var claims = principal.Claims.ToList();

            if (principal.Identity is ClaimsIdentity identity)
            {
                // Remove existing role claims to avoid duplicates
                var existingRoles = identity.FindAll(ClaimTypes.Role).ToList();
                foreach (var r in existingRoles) identity.RemoveClaim(r);

                // Extract realm_access roles
                var realmClaim = claims.FirstOrDefault(c => c.Type == "realm_access");
                if (realmClaim != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(realmClaim.Value);
                        if (doc.RootElement.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var r in roles.EnumerateArray())
                            {
                                var rn = r.GetString();
                                if (!string.IsNullOrEmpty(rn)) identity.AddClaim(new Claim(ClaimTypes.Role, rn));
                            }
                        }
                    }
                    catch { }
                }

                // Extract resource_access roles
                var resourceClaim = claims.FirstOrDefault(c => c.Type == "resource_access");
                if (resourceClaim != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(resourceClaim.Value);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var client in doc.RootElement.EnumerateObject())
                            {
                                if (client.Value.TryGetProperty("roles", out var clientRoles) && clientRoles.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var r in clientRoles.EnumerateArray())
                                    {
                                        var rn = r.GetString();
                                        if (!string.IsNullOrEmpty(rn)) identity.AddClaim(new Claim(ClaimTypes.Role, rn));
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return claims;
        }

        private async Task<string> GetAdminTokenAsync()
        {
            var keycloakUrl = _configuration["Keycloak:ServerUrl"];
            var adminUser = _configuration["Keycloak:AdminUser"];
            var adminPassword = _configuration["Keycloak:AdminPassword"];

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","password"),
                new KeyValuePair<string,string>("client_id","admin-cli"),
                new KeyValuePair<string,string>("username", adminUser!),
                new KeyValuePair<string,string>("password", adminPassword!)
            });

            var response = await _httpClient.PostAsync($"{keycloakUrl}/realms/master/protocol/openid-connect/token", content);
            response.EnsureSuccessStatusCode();

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString()!;
        }

        #endregion
    }
}