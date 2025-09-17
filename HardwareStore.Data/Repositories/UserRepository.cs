using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.AppUsers.ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            return user!;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            return user!;
        }

        public async Task AddAsync(User user)
        {
            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
