namespace HardwareStore.Services.Interfaces
{
    public interface INotificationPublisher
    {
        Task PublishAsync(string message, string? userId);
    }
}
