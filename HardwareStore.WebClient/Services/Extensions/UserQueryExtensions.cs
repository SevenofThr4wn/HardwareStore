using HardwareStore.Core.Models;
using HardwareStore.WebClient.ViewModels.Account;

namespace HardwareStore.WebClient.Services.Extensions
{
    public static class UserQueryExtensions
    {
        public static IQueryable<ApplicationUser> ApplySearch(
            this IQueryable<ApplicationUser> query, string? searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return query;

            return query.Where(u =>
                u.UserName!.Contains(searchString) ||
                u.Email!.Contains(searchString) ||
                u.FirstName!.Contains(searchString) ||
                u.LastName!.Contains(searchString));
        }

        public static IQueryable<UserManageVM> SelectUsers(
            this IQueryable<ApplicationUser> query)
        {
            return query.Select(u => new UserManageVM
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                FirstName = u.FirstName!,
                LastName = u.LastName!,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLogin = u.LastLogin,
                CreatedDate = u.DateCreated ?? DateTime.MinValue
            });
        }

        public static IQueryable<UserEditVM> SelectEditUsers(
            this IQueryable<ApplicationUser> query)
        {
            return query.Select(u => new UserEditVM
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                FirstName = u.FirstName!,
                LastName = u.LastName!,
                Role = u.Role,
                IsActive = u.IsActive
            });
        }
    }
}