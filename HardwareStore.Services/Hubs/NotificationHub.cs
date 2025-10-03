using Microsoft.AspNetCore.SignalR;

namespace HardwareStore.Services.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin-notifications");
        }

        public async Task JoinManagerGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "manager-notifications");
        }
    }
}