using Amazon.SecretsManager.Model;
using MediatR;

namespace ThreeTP.Payment.Application.Queries.AwsSecrets
{
    public sealed record GetSecretValueQuery(
        string SecretId,
        string? VersionId = null,
        string? VersionStage = null,
        bool ForceRefresh = false
    ) : IRequest<GetSecretValueResponse>;
}
