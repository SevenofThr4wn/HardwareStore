using HardwareStore.Services.Hubs;
using HardwareStore.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HardwareStore.WebClient.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationPublisher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishAsync(string? id, string? title, string message)
        {
            string safeTitle = title ?? string.Empty;

            if (!string.IsNullOrEmpty(id))
            {
                await _hubContext.Clients.User(id)
                    .SendAsync("ReceiveNotification", safeTitle, message);
            }
            else
            {
                await _hubContext.Clients.All
                    .SendAsync("ReceiveNotification", safeTitle, message);
            }
        }

    }
}
