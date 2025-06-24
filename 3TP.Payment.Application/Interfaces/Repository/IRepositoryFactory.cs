using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Application.Interfaces.Terminals;

namespace ThreeTP.Payment.Application.Interfaces.Repository;

public interface IRepositoryFactory
{
    ITerminalRepository CreateTerminalRepository();
    ITenantRepository CreateTenantRepository();
    IGenericRepository<T> CreateGenericRepository<T>() where T : class;
}