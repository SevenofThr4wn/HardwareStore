using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<ApplicationUser> GetByUsernameAsync(string username);
        Task<ApplicationUser> GetByEmailAsync(string email);
    }
}