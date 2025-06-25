namespace ThreeTP.Payment.Application.Interfaces
{
    public interface ISecretValidationService
    {
        Task<string> DecryptSecretAsync(string encryptedSecret);
        Task<bool> ValidateSecretAsync(string secretId, string secretToValidate);
    }
}
