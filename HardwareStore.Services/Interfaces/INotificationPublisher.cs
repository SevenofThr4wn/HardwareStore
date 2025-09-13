namespace HardwareStore.Services.Interfaces
{
    public interface INotificationPublisher
    {
        Task PublishAsync(string userId, string message);
    }
}
