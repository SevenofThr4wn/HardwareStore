using HardwareStore.Core.Models;

namespace HardwareStore.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(string id);
        Task<User> GetByEmailAsync(string email);
        Task CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
    }
}
