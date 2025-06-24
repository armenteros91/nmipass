using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.Repository;
using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Application.Interfaces.Terminals;

namespace ThreeTP.Payment.Infrastructure.Persistence.Repositories;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly NmiDbContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEncryptionService _encryptionService;

    public RepositoryFactory(
        NmiDbContext context,
        ILoggerFactory loggerFactory,
        IEncryptionService encryptionService)
    {
        _context = context;
        _loggerFactory = loggerFactory;
        _encryptionService = encryptionService;
    }

    public ITerminalRepository CreateTerminalRepository()
    {
        return new TerminalRepository(
            _loggerFactory.CreateLogger<TerminalRepository>(),
            _encryptionService,
            _context);
    }

    public ITenantRepository CreateTenantRepository()
    {
        return new TenantRepository(
            _context,
            _loggerFactory.CreateLogger<TenantRepository>());
    }

    public IGenericRepository<T> CreateGenericRepository<T>() where T : class
    {
        return new GenericRepository<T>(
            _context,
            _loggerFactory.CreateLogger<GenericRepository<T>>());
    }
}