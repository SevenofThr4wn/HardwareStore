using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using HardwareStore.WebClient.Services;
using HardwareStore.WebClient.Services.Extensions;
using HardwareStore.WebClient.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.WebClient.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepo;
        private readonly IUserService _userService;

        public UsersController
           (
            AppDbContext context,
            IUserRepository userRepo,
            IUserService userService)
        {
            _context = context;
            _userService = userService;
            _userRepo = userRepo;
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManageUsers(string searchString,
            string roleFilter, int page = 1, int pageSize = 10)
        {
            var usersQuery = _userService.GetUsersQuery()
                .ApplySearch(searchString);

            usersQuery = _userService.ApplyRoleFilter(usersQuery, roleFilter);

            var totalUsers = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .SelectUsers()
                .ToListAsync();

            var availableRoles = await _userService.GetAvailableRolesAsync();

            ViewBag.SearchString = searchString;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.AvailableRoles = availableRoles!;
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
            var user = await _context.AppUsers
                .Where(u => u.Id == id)
                .SelectEditUsers()
                .FirstOrDefaultAsync();

            // Null Check: If ID is empty, return Not Found(404) response.
            if (user == null)
            {
                return NotFound();
            }

            // Get available roles from database
            var availableRoles = _userService.GetAvailableRolesAsync();

            return Ok(new { user, availableRoles });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> EditUser(UserEditVM model)
        {
            // Check: If the Model State is invalid, return Bad Request (400) response.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Finds the User ID in the system.

            var user = await _userRepo.GetByIdAsync(model.Id);

            // Null Check: If ID is empty, return Not Found(404) response.
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

            // Finds the user in the system by it's Id.
            var user = await _userRepo.GetByIdAsync(id);

            // Null Check: If ID is empty, return Not Found(404) response.
            if (user == null)
            {
                return NotFound();
            }

            // Check: Prevents user from deleting their own account
            if (user.UserName == User.Identity!.Name)
            {
                return BadRequest(new { success = false, message = "Cannot delete your own account" });
            }

            await _userRepo.DeleteAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "User deleted successfully" });
        }
    }
}