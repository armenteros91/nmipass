using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.Repository;

namespace ThreeTP.Payment.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Provides generic repository functionality for performing CRUD operations on entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly NmiDbContext _dbContext;
        private readonly ILogger<GenericRepository<T>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{T}"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger">The logger for logging repository operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public GenericRepository(
            NmiDbContext context,
            ILogger<GenericRepository<T>> logger)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a single entity that matches the specified predicate, optionally including related data.
        /// </summary>
        /// <param name="predicate">The condition to filter the entity.</param>
        /// <param name="includes">Optional navigation properties to include in the query.</param>
        /// <returns>The first entity that matches the predicate, or null if none is found.</returns>
        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[]? includes)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            _logger.LogInformation("Executing GetOneAsync for entity {BaseEntity}", typeof(T).Name);
            return await query.FirstOrDefaultAsync(predicate);
        }


        /// <summary>
        /// Retrieves all entities, optionally filtered by a predicate and including related data.
        /// </summary>
        /// <param name="predicate">An optional condition to filter the entities.</param>
        /// <param name="includes">Optional navigation properties to include in the query.</param>
        /// <returns>A collection of entities that match the criteria.</returns>
        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            params Expression<Func<T, object>>[]? includes)
        {
            var query = _dbContext.Set<T>().AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            _logger.LogInformation("Executing GetAllAsync for entity {BaseEntity}", typeof(T).Name);
            return await query.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        public async Task AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _logger.LogInformation("Adding a new entity of type {BaseEntity}", typeof(T).Name);
            await _dbContext.Set<T>().AddAsync(entity);
           // await _dbContext.SaveChangesAsync(); //delegar al uow el trabajo de persistencia en la DB 
        }


        /// <summary>
        /// Updates an existing entity in the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        public async Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _logger.LogInformation("Updating an entity of type {BaseEntity}", typeof(T).Name);
            _dbContext.Set<T>().Update(entity);
           // await _dbContext.SaveChangesAsync(); //delegar al uow el trabajo de persistencia en la DB 
        }

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        public async Task DeleteAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _logger.LogInformation("Deleting an entity of type {BaseEntity}", typeof(T).Name);
            _dbContext.Set<T>().Remove(entity);
         //   await _dbContext.SaveChangesAsync(); //delegar al uow el trabajo de persistencia en la DB 
        }

        /// <summary>
        /// Checks if any entity matches the specified predicate.
        /// </summary>
        /// <param name="predicate">The condition to check for existence.</param>
        /// <returns>True if an entity exists that matches the predicate, false otherwise.</returns>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            _logger.LogInformation("Checking existence for entity {BaseEntity}", typeof(T).Name);
            return await _dbContext.Set<T>().AnyAsync(predicate);
        }
    }
}