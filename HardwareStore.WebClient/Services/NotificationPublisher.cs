using HardwareStore.Services.Interfaces;
using HardwareStore.WebClient.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HardwareStore.WebClient.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotifHub> _hubContext;

        public NotificationPublisher(IHubContext<NotifHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishAsync(string message, string? userId = null)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                await _hubContext.Clients.User(userId)
                    .SendAsync("ReceiveNotification", message);
            }
            else
            {
                await _hubContext.Clients.All
                    .SendAsync("ReceiveNotification", message);
            }
        }
    }
}
