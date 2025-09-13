using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;

namespace HardwareStore.Data.Helper
{
    public class KeycloakHelper
    {
        private readonly KeycloakClient _client;
        private readonly string _realm;

        public KeycloakHelper(IOptions<KeycloakOptions> options)
        {
            var opts = options.Value;
            _realm = opts.Realm;
            _client = new KeycloakClient(opts.ServerUrl, opts.AdminUser, opts.AdminPassword);
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
