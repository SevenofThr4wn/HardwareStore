using Microsoft.AspNetCore.SignalR;

namespace HardwareStore.WebClient.Hubs
{
    public class NotifHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();

        }

        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
