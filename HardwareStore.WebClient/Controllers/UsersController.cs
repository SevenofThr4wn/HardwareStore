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
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        public UsersController(AppDbContext context,
            IUserRepository userRepository,
            IUserService userService)
        {
            _userService = userService;
            _context = context;
            _userRepository = userRepository;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Finds the User ID in the system.

            var user = await _userRepository.GetByIdAsync(model.Id);

            // If the ID is empty, then return Response Code 404(Not Found)
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
            var user = await _userRepository.GetByIdAsync(id);

            // If the Id is null, then return a response code 404(Not Found).
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting own account
            if (user.UserName == User.Identity!.Name)
            {
                return BadRequest(new { success = false, message = "Cannot delete your own account" });
            }

            await _userRepository.DeleteAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "User deleted successfully" });
        }
    }
}