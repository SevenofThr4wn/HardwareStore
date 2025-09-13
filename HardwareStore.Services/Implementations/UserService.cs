using HardwareStore.Core.Models;
using HardwareStore.Data.Repositories.Interfaces;
using HardwareStore.Services.Interfaces;

namespace HardwareStore.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task CreateUserAsync(User user, string password)
        {
            await _userRepository.AddAsync(user);
        }

        public async Task DeleteUserAsync(User user)
        {
            await _userRepository.DeleteAsync(user);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }
    }
}