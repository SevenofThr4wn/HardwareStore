using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Data.Models.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            var users = await _context.AppUsers.ToListAsync();
            return users;
        }

        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            return user!;
        }

        public async Task<ApplicationUser> GetByUsernameAsync(string username)
        {
            var user = await _context.AppUsers.FindAsync(username);
            return user!;
        }

        public async Task<ApplicationUser> GetByIdAsync(string id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            return user!;
        }

        public async Task AddAsync(ApplicationUser user)
        {
            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ApplicationUser user)
        {
            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}