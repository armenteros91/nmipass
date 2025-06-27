using ThreeTP.Payment.Application.Interfaces.Repository;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces.Terminals
{
    public interface ITerminalRepository: IGenericRepository<Terminal>
    {
        Task<Terminal?> GetByIdAsync(Guid id);
        Task<Terminal?> GetBySecretKeyAsync(string encryptedSecretKey); // This method's name might now be misleading if "encryptedSecretKey" is an ARN. Consider renaming in future.
        Task<Terminal?> GetByTenantIdAsync(Guid tenantId); // Changed from IEnumerable<Terminal>
        Task<string?> GetSecretIdentifierAsync(Guid terminalId); // Renamed from GetDecryptedSecretKeyAsync
        Task UpdateSecretKeyAsync(Guid terminalId, string newPlainSecretKey); // newPlainSecretKey is now a secret identifier (ARN)
        Task<Terminal?> FindBySecretNameAsync(string plainSecretName);
        void Update(Terminal terminal);
    }


}
