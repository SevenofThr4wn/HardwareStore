using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Column("user_id")]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [Required, Column("keycloak_id")]
        public string KeyCloakId { get; set; } = string.Empty;

        [Column("full_name"), StringLength(200)]
        public string? FullName{ get; set; }

        [Column("first_name"), StringLength(100)]
        public string? FirstName { get; set; } = string.Empty;

        [Column("last_name"), StringLength(100)]
        public string? LastName { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [Column("date_created")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}