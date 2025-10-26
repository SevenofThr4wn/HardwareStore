using HardwareStore.Extensions.Extensions;
using HardwareStore.WebClient.Middleware;
using HardwareStore.WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services
    .AddRepositories(builder.Configuration)
    .AddServices(builder.Configuration)
    .AddScoped<IUserService, UserService>()
    .AddScoped<IProductService, ProductService>()
    .AddScoped<IOrderService, OrderService>();

builder.Services.AddIdentityConfig();
builder.Services.ConfigureSQLDatabase(builder.Configuration);
builder.Services.ConfigureKeycloakAuthentication(builder.Configuration);
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
//app.UseSecurityHeaders();

//app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
