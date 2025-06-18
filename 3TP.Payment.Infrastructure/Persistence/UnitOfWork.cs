using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Infrastructure.Persistence.Repositories;

namespace ThreeTP.Payment.Infrastructure.Persistence;

/// <summary>
/// Implements the Unit of Work pattern to coordinate database operations, transactions, and domain events.
/// </summary>
public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly NmiDbContext _sharedContext;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private IDbContextTransaction? _currentTransaction;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private bool _disposed;


    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="dispatcher">The domain event dispatcher for handling domain events.</param>
    /// <param name="logger">The logger for logging Unit of Work operations.</param>
    /// <param name="loggerFactory">The factory to create loggers for repositories.</param>
    /// <param name="sharedContext"></param>
    /// <param name="repositoryFactory"></param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
    public UnitOfWork(
        NmiDbContext sharedContext,
        IDomainEventDispatcher dispatcher,
        ILogger<UnitOfWork> logger,
        ILoggerFactory loggerFactory,
        IRepositoryFactory repositoryFactory)
    {
        _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
    }


    /// <summary>
    /// Gets the repository for tenant-related operations.
    /// </summary>
    public ITenantRepository TenantRepository =>
        GetRepository<ITenantRepository>(() => _repositoryFactory.CreateTenantRepository());

    /// <summary>
    /// Gets the repository for terminals tenants
    /// </summary>
    public ITerminalRepository TerminalRepository =>
        GetRepository<ITerminalRepository>(() => _repositoryFactory.CreateTerminalRepository());

    /// <summary>
    /// Gets a generic repository for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>An <see cref="IGenericRepository{T}"/> for the specified entity type.</returns>
    public IGenericRepository<T> Repository<T>() where T : class
    {
        return (IGenericRepository<T>)_repositories.GetOrAdd(
            typeof(IGenericRepository<T>),
            _ => new GenericRepository<T>(_sharedContext, _loggerFactory.CreateLogger<GenericRepository<T>>())
        );
    }

    /// <summary>
    /// Saves all changes made in the context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(_sharedContext, cancellationToken);
        return await _sharedContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves all changes made in the context to the database and indicates if any changes were made.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if changes were made, false otherwise.</returns>
    /// <exception cref="DbUpdateException">Thrown if the database operation fails.</exception>
    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DispatchDomainEventsAsync(_sharedContext, cancellationToken);
            var result = await _sharedContext.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error committing changes to database");
            throw;
        }
    }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown if a transaction is already in progress.</exception>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _sharedContext.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Transactions started");
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown if no transaction is active.</exception>
    /// <exception cref="DbUpdateException">Thrown if the database operation fails.</exception>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await DispatchDomainEventsAsync(_sharedContext, cancellationToken);
            await _sharedContext.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transactions committed");
        }
        catch (DbUpdateException)
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Rolls back the current transaction or resets entity states if no transaction is active.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Transactions rolled back");
            await DisposeTransactionAsync();
        }
        else
        {
            await ResetEntityStatesAsync();
        }
    }


    /// <summary>
    /// Dispatches all pending domain events associated with tracked entities.
    /// </summary>
    /// <param name="context">The database context to use for tracking entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private async Task DispatchDomainEventsAsync(NmiDbContext context, CancellationToken cancellationToken = default)
    {
        var domainEntities = context.ChangeTracker
            .Entries<BaseEntityWithEvents>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        if (!domainEntities.Any())
        {
            _logger.LogWarning("No domain events to dispatch");
            return;
        }

        _logger.LogInformation("Dispatching {EventCount} domain events",
            domainEntities.Sum(x => x.Entity.DomainEvents.Count));

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogInformation("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
                await _dispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event {EventType}", domainEvent.GetType().Name);
            }
        }
    }

    /// <summary>
    /// Resets the state of tracked entities to their original state.
    /// </summary>
    private async Task ResetEntityStatesAsync()
    {
        foreach (var entry in _sharedContext.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the current transaction and associated context, if any.
    /// </summary>
    private async ValueTask DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Asynchronously disposes of the Unit of Work and its resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await DisposeTransactionAsync();
            _disposed = true;
        }
    }

    /// <summary>
    /// Synchronously disposes of the Unit of Work and its resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Retrieves or creates a repository of the specified type.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <param name="factory">The factory method to create the repository.</param>
    /// <returns>An instance of the specified repository type.</returns>
    private TRepository GetRepository<TRepository>(Func<TRepository> factory) where TRepository : class
    {
        return (TRepository)_repositories.GetOrAdd(typeof(TRepository), _ => factory());
    }
}