using HardwareStore.Extensions.Extensions;
using HardwareStore.Services.Interfaces;
using HardwareStore.WebClient.Hubs;
using HardwareStore.WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRepositories();
builder.Services.AddServices(builder.Configuration);

// Notification Publisher
builder.Services.AddScoped<INotificationPublisher, NotificationPublisher>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapHub<NotifHub>("/notifications");


app.Run();
