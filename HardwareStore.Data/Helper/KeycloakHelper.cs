using Keycloak.Net;
using Keycloak.Net.Models.Users;

namespace HardwareStore.Data.Helper
{
    public class KeycloakHelper
    {
        private readonly KeycloakClient _client;
        private readonly string _realm;

        public KeycloakHelper(string serverUrl, string realm, string adminUser, string adminPassword)
        {
            _realm = realm;
            _client = new KeycloakClient(serverUrl, adminUser, adminPassword);
        }

        public async Task<bool> CreateUserAsync(User kcUser, string password)
        {
            var created = await _client.CreateUserAsync(_realm, kcUser);
            if (!created) return false;

            await _client.SetUserPasswordAsync(_realm, kcUser.Id, password);
            return true;
        }
    }
}
