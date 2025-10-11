using HardwareStore.Data.Context;
using HardwareStore.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Data.Models.Repositories
{
    /// <summary>
    /// Base repository providing basic CRUD operations for any entity type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity the repository manages. Must be a class.</typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The database context to use</param>
        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Retrieves an entity by its integer ID asynchronously.
        /// </summary>
        /// <param name="id">The integer ID of the entity.</param>
        /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
        public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

        /// <summary>
        /// Retrieves an entity by its string ID asynchronously.
        /// </summary>
        /// <param name="id">The string ID of the entity.</param>
        /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
        public async Task<T?> GetByIdAsync(string id) =>
        await _dbSet.FindAsync(id);

        /// <summary>
        /// Retrieves all entites of type <typeparamref name="T"/> asynchronously.
        /// </summary>
        /// <returns>A list of all entities.</returns>
        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _dbSet.ToListAsync();

        /// <summary>
        /// Adds a new entity to the repository asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public async Task AddAsync(T entity) =>
            await _dbSet.AddAsync(entity);

        /// <summary>
        /// Updates an existing entity by ID using a provided update action.
        /// </summary>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="updateAction">An action taht applies the updates to the entity.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the entity with the specified ID is not found.</exception>
        public async Task UpdateAsync(int id, Action<T> updateAction)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with id {id} not found.");

            updateAction(entity);
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Deletes an entity by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the entity with the specified ID is not found.</exception>
        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with id {id} not found.");

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Deletes the specified entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided entity is null.</exception>
        public async Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }
    }
}