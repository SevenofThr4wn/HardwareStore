using HardwareStore.Extensions.Extensions;
using HardwareStore.Services.Hubs;
using HardwareStore.WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Identity
builder.Services.AddIdentityConfig();

// Database 
builder.Services.ConfigureSQLDatabase(builder.Configuration);

// Authentication
builder.Services.ConfigureKeycloakAuthentication(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapHub<NotificationHub>("/notifications");

app.Run();
