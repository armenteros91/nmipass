using Amazon.SecretsManager.Model;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using ThreeTP.Payment.Application.Queries.AwsSecrets;

namespace ThreeTP.Payment.Application.Interfaces.aws;

public interface IAwsSecretManagerService
{
    Task<GetSecretValueResponse> GetSecretValueAsync(
        GetSecretValueQuery query,
        CancellationToken cancellationToken = default);

    Task<CreateSecretResponse> CreateSecretAsync(
        CreateSecretCommand command,
        CancellationToken cancellationToken = default);

    Task<UpdateSecretResponse> UpdateSecretAsync(
        UpdateSecretCommand command,
        CancellationToken cancellationToken = default);
}