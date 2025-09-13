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

        public async Task PublishAsync(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
