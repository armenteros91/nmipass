using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces
{
    public interface ITerminalRepository: IGenericRepository<Terminal>
    {
        Task<Terminal?> GetByIdAsync(Guid id);
        Task<Terminal?> GetBySecretKeyAsync(string encryptedSecretKey);
        Task<IEnumerable<Terminal>> GetByTenantIdAsync(Guid tenantId);
        Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId);
        Task UpdateSecretKeyAsync(Guid terminalId, string newPlainSecretKey);
        Task<Terminal?> FindBySecretNameAsync(string plainSecretName);
        void Update(Terminal terminal);
    }


}
