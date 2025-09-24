using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.WebClient.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<ApplicationUser> GetUsersQuery()
        {
            return _context.AppUsers.AsQueryable();
        }

        public IQueryable<ApplicationUser> ApplyRoleFilter(IQueryable<ApplicationUser> query, string? roleFilter)
        {
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                query = query.Where(u => u.Role == roleFilter);
            }
            return query;
        }

        public async Task<List<string>> GetAvailableRolesAsync()
        {
            return await _context.AppUsers
                .Where(u => !string.IsNullOrEmpty(u.Role))
                .Select(u => u.Role)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public async Task<int> GetTotalUsers()
        {
            return await _context.AppUsers.CountAsync();
        }

        public async Task<int> GetStaffCount()
        {
            var staffRoles = new[] { "Staff", "Admin", "Manager" };
            return await _context.AppUsers.CountAsync(u => staffRoles.Contains(u.Role));
        }
    }
}