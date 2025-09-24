namespace HardwareStore.WebClient.ViewModels.Account
{
    public class UserManageVM
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";

        public string FormattedLastLogin => LastLogin.HasValue
            ? LastLogin.Value.ToString("MMM dd, yyyy HH:mm")
            : "Never";

        public string FormattedCreatedDate => CreatedDate.ToString("MMM dd, yyyy");

        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";

        public string StatusText => IsActive ? "Active" : "Inactive";

        public string RoleBadgeClass => Role switch
        {
            "Admin" => "bg-danger",
            "Manager" => "bg-warning",
            "Staff" => "bg-primary",
            _ => "bg-secondary"
        };
    }
}