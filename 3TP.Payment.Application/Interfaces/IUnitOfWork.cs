namespace ThreeTP.Payment.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the repository for tenant-related operations.
    /// </summary>
    ITenantRepository TenantRepository { get; }
    ITerminalRepository TerminalRepository { get; }

    /// <summary>
    /// Gets a generic repository for the specified entity type.
    /// </summary>
    IGenericRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Saves all changes made in the context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in the context to the database and indicates if any changes were made.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if changes were made, false otherwise.</returns>
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown if a transaction is already in progress.</exception>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown if no transaction is active.</exception>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">Thrown if the database operation fails.</exception>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction or resets entity states if no transaction is active.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}