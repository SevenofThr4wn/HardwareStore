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


        public KeycloakHelper(IOptions<KeycloakOptions> options, IConfiguration configuration, KeycloakClient client)
        {
            _options = options.Value;
            _realm = _options.Realm;
            _configuration = configuration;
            _client = client;
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

        public string GetClientId()
        {
            return _configuration["Keycloak:ClientId"]!;
        }
    }
}
