using HardwareStore.Core.Models;

namespace HardwareStore.WebClient.Services
{
    public interface IUserService
    {
        IQueryable<ApplicationUser> GetUsersQuery();
        IQueryable<ApplicationUser> ApplyRoleFilter(IQueryable<ApplicationUser> query, string? roleFilter);
        Task<List<string>> GetAvailableRolesAsync();
        Task<int> GetStaffCount();
        Task<int> GetTotalUsers();
    }
}