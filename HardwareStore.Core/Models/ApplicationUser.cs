using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(256)]
        public string KeyCloakId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Role { get; set; } = string.Empty;
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}