using Amazon.SecretsManager.Model;

namespace ThreeTP.Payment.Application.Interfaces.aws;

public interface IAwsSecretsProvider
{
    Task<GetSecretValueResponse> GetSecretAsync(string secretId, string? versionId = null, string? versionStage = null,
        CancellationToken cancellationToken = default);

    Task<CreateSecretResponse> CreateSecretAsync(string name, string secretString, string? description,
        CancellationToken cancellationToken = default);

    Task<UpdateSecretResponse> UpdateSecretAsync(string secretId, string newSecretString, string? description,
        CancellationToken cancellationToken = default);

    Task<List<SecretListEntry>> ListSecretsAsync(CancellationToken cancellationToken = default);
}