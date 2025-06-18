using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces
{
    public interface ITerminalService
    {
        Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId);
        Task<Terminal?> FindBySecretNameAsync(string plainSecretName);
    }
}
