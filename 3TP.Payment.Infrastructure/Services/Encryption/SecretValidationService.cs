using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.aws;

namespace ThreeTP.Payment.Infrastructure.Services.Encryption
{
    public class SecretValidationService : ISecretValidationService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IAwsSecretManagerService _awsSecretManagerService;
        private readonly ILogger<SecretValidationService> _logger;

        public SecretValidationService(
            IEncryptionService encryptionService,
            IAwsSecretManagerService awsSecretManagerService,
            ILogger<SecretValidationService> logger)
        {
            _encryptionService = encryptionService;
            _awsSecretManagerService = awsSecretManagerService;
            _logger = logger;
        }

        public async Task<string> DecryptSecretAsync(string encryptedSecret)
        {
            try
            {
                return await Task.FromResult(_encryptionService.Decrypt(encryptedSecret));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting secret.");
                throw;
            }
        }

        public async Task<bool> ValidateSecretAsync(string secretId, string secretToValidate)
        {
            try
            {
                var storedSecret = await _awsSecretManagerService.GetSecretValueAsync(secretId);
                if (storedSecret == null)
                {
                    _logger.LogWarning("Secret with ID {SecretId} not found in AWS Secrets Manager.", secretId);
                    return false;
                }
                // Assuming the secret stored in AWS is already encrypted and needs to be decrypted for comparison
                // Or, if the secretToValidate is plain text and needs to be compared against a decrypted stored secret
                var decryptedStoredSecret = _encryptionService.Decrypt(storedSecret.SecretString); // Adjust if stored secret is not encrypted or differently handled

                // Validate the decrypted stored secret against the provided secret to validate
                // This comparison logic might need adjustment based on how secrets are stored and managed.
                // For example, if secretToValidate is plaintext, it should be compared with decryptedStoredSecret.
                // If secretToValidate is also encrypted, it might need decryption before comparison,
                // or the plaintext version of storedSecret should be encrypted and then compared.
                // Assuming secretToValidate is plain text for this example:
                return decryptedStoredSecret == secretToValidate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating secret for ID {SecretId}.", secretId);
                throw; // Or return false, depending on desired error handling
            }
        }
    }
}
