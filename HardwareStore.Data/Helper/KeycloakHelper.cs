using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HardwareStore.Data.Helper
{
    public class KeycloakHelper
    {
        private readonly KeycloakClient _client;
        private readonly string _realm;
        private readonly KeycloakOptions _options;
        private readonly IConfiguration _configuration;


        public KeycloakHelper(IOptions<KeycloakOptions> options, IConfiguration configuration)
        {
            _options = options.Value;
            _realm = _options.Realm;
            _client = new KeycloakClient(_options.ServerUrl, _options.AdminUser, _options.AdminPassword);
            _configuration = configuration;
        }

        public async Task<bool> CreateUserAsync(User kcUser, string password)
        {
            var created = await _client.CreateUserAsync(_realm, kcUser);
            if (!created) return false;

            await _client.SetUserPasswordAsync(_realm, kcUser.Id, password);
            return true;
        }

        public string GetKeycloakAuthority()
        {
            var authority = _configuration["Keycloak:Authority"];
            if (string.IsNullOrEmpty(authority))
                throw new InvalidOperationException("Keycloak Authority is not configured.");
            return authority.TrimEnd('/');
        }

        public string BuildLogoutUrl(string idToken, string redirectUri)
        {
            return $"{GetKeycloakAuthority()}/protocol/openid-connect/logout?" +
                   $"id_token_hint={Uri.EscapeDataString(idToken)}&" +
                   $"post_logout_redirect_uri={Uri.EscapeDataString(redirectUri)}";
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var users = await _client.GetUsersAsync(_realm, username: username);
            return users?.FirstOrDefault()!;
        }

        // Optional: Helper method to delete user
        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await _client.DeleteUserAsync(_realm, userId);
        }

        public string GetClientId()
        {
            return _configuration["Keycloak:ClientId"];
        }

    }
}
