using System.Text.Json.Serialization;

namespace HardwareStore.Data.Identity
{
    public class KeycloakUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("createdTimestamp")]
        public long CreatedTimestamp { get; set; }

    }

    public class KeycloakRole
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("composite")]
        public bool Composite { get; set; }

        [JsonPropertyName("clientRole")]
        public bool ClientRole { get; set; }

        [JsonPropertyName("containerId")]
        public string ContainerId { get; set; } = string.Empty;
    }
}
