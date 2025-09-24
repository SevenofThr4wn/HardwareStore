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
