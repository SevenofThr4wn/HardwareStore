namespace HardwareStore.Data.Models.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task UpdateAsync(int id, Action<T> updateAction);
        Task DeleteAsync(int id);
        Task DeleteAsync(T entity);
    }
}