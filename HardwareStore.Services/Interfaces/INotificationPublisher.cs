namespace HardwareStore.Services.Interfaces
{
    public interface INotificationPublisher
    {
        Task PublishAsync(string? id, string? title, string message);
    }
}