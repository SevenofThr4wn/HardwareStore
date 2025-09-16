using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareStore.Core.Models
{
    public class User
    {
        [Key, Column("user_id")]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(100), Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(256), Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("email_confirmed")]
        public bool EmailConfirmed { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>;
    }
}