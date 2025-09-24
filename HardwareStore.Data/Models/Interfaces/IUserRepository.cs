using HardwareStore.Core.Models;

namespace HardwareStore.Data.Models.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<ApplicationUser> GetByUsernameAsync(string username);
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task DeleteAsync(ApplicationUser user);

    }
}
