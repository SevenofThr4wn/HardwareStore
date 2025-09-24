using HardwareStore.Core.Models;
using HardwareStore.Data.Context;
using HardwareStore.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HardwareStore.Services.Tasks
{
    public class KeyCloakSync
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly string _realm;
        private readonly string _adminUser;
        private readonly string _adminPassword;

        public KeyCloakSync(AppDbContext context,
            HttpClient httpClient,
            string serverUrl,
            string realm,
            string adminUser,
            string adminPassword)
        {
            _context = context;
            _httpClient = httpClient;
            _serverUrl = serverUrl.TrimEnd('/');
            _realm = realm;
            _adminUser = adminUser;
            _adminPassword = adminPassword;
        }

        private async Task<string> GetAdminTokenAsync()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","password"),
                new KeyValuePair<string,string>("client_id","admin-cli"),
                new KeyValuePair<string,string>("username", _adminUser),
                new KeyValuePair<string,string>("password", _adminPassword)
            });

            // Requests Admin API for a new JWT token.
            var response = await _httpClient.PostAsync($"{_serverUrl}/realms/master/protocol/openid-connect/token", content);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);
            return json.RootElement.GetProperty("access_token").GetString()!;
        }

        private async Task<List<string>> GetUserRolesAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("Cannot get roles - user ID is empty");
                return new List<string>();
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_serverUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var roles = JsonSerializer.Deserialize<List<KeycloakRole>>(content) ?? new List<KeycloakRole>();

                    // Returns the role names
                    return roles.Select(r => r.Name).ToList();
                }
                else
                {
                    Console.WriteLine($"Failed to get roles for user {userId}: {response.StatusCode}");
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching roles for user {userId}: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task SyncUsersAsync()
        {
            try
            {
                // Retrieves a fresh JWT Token
                var token = await GetAdminTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Call Keycloak Admin API
                var response = await _httpClient.GetStringAsync($"{_serverUrl}/admin/realms/{_realm}/users");
                var users = JsonSerializer.Deserialize<List<KeycloakUser>>(response) ?? new List<KeycloakUser>();

                Console.WriteLine($"Found {users.Count} users in Keycloak");

                foreach (var kcUser in users)
                {
                    // Checks if the Id Field is null/empty
                    if (string.IsNullOrEmpty(kcUser.Id))
                    {
                        Console.WriteLine($"Skipping user {kcUser.Username} - Empty ID");
                        continue;
                    }
                    // If the id is NOT empty, then write to console.
                    Console.WriteLine($"Processing user: {kcUser.Username} (ID: {kcUser.Id})");

                    // Get user roles from Keycloak
                    var userRoles = await GetUserRolesAsync(kcUser.Id, token);
                    Console.WriteLine($"User {kcUser.Username} has roles: {string.Join(", ", userRoles)}");

                    // Determine the highest priority role or use a default
                    var primaryRole = DeterminePrimaryRole(userRoles) ?? "Staff";
                    Console.WriteLine($"Primary role determined: {primaryRole}");

                    var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.KeyCloakId == kcUser.Id);
                    if (user == null)
                    {
                        Console.WriteLine($"Creating new user: {kcUser.Username}");
                        user = new ApplicationUser
                        {
                            KeyCloakId = kcUser.Id,
                            UserName = kcUser.Username,
                            FirstName = kcUser.FirstName,
                            LastName = kcUser.LastName,
                            Email = kcUser.Email,
                            IsActive = kcUser.Enabled,
                            Role = primaryRole
                        };

                        _context.AppUsers.Add(user);
                    }
                    else
                    {
                        Console.WriteLine($"Updating existing user: {kcUser.Username}");

                        // Updates existing user
                        user.UserName = kcUser.Username;
                        user.FirstName = kcUser.FirstName;
                        user.LastName = kcUser.LastName;
                        user.Email = kcUser.Email;
                        user.IsActive = kcUser.Enabled;
                        user.Role = primaryRole;
                    }
                }

                // Retrieves the changes to the database and writes it to the console.
                var changes = await _context.SaveChangesAsync();
                Console.WriteLine($"Saved {changes} changes to database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SyncUsersAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private string? DeterminePrimaryRole(List<string> roles)
        {
            if (roles.Contains("admin")) return "Admin";
            if (roles.Contains("manager")) return "Manager";
            if (roles.Contains("staff")) return "Staff";

            return roles.FirstOrDefault();
        }
    }
}