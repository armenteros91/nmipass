namespace ThreeTP.Payment.Application.Interfaces;

public interface IRepositoryFactory
{
    ITerminalRepository CreateTerminalRepository();
    ITenantRepository CreateTenantRepository();
    IGenericRepository<T> CreateGenericRepository<T>() where T : class;
}