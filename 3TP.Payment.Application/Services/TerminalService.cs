using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly ITerminalRepository _terminalRepository;
        private readonly IEncryptionService _encryptionService;

        public TerminalService(
            ITerminalRepository terminalRepository,
            IEncryptionService encryptionService)
        {
            _terminalRepository = terminalRepository;
            _encryptionService = encryptionService;
        }

        public async Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId)
        {
            var terminal = await _terminalRepository.GetByIdAsync(terminalId);
            return terminal != null
                ? _encryptionService.Decrypt(terminal.SecretKeyEncrypted)
                : null;
        }

        public async Task<Terminal?> FindBySecretNameAsync(string plainSecretName)
        {
            return await _terminalRepository.FindBySecretNameAsync(plainSecretName);
        }
    }
}