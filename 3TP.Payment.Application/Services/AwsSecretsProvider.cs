using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces.aws;

namespace ThreeTP.Payment.Application.Services;

public class AwsSecretsProvider : IAwsSecretsProvider
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly ILogger<AwsSecretsProvider> _logger;

    public AwsSecretsProvider(IAmazonSecretsManager secretsManager, ILogger<AwsSecretsProvider> logger)
    {
        _secretsManager = secretsManager;
        _logger = logger;
    }

    public async Task<GetSecretValueResponse> GetSecretAsync(string secretId, string? versionId = null,
        string? versionStage = null, CancellationToken cancellationToken = default)
    {
        var request = new GetSecretValueRequest
        {
            SecretId = secretId,
            VersionId = versionId,
            VersionStage = versionStage
        };

        return await _secretsManager.GetSecretValueAsync(request, cancellationToken);
    }

    public async Task<CreateSecretResponse> CreateSecretAsync(string name, string secretString, string? description,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateSecretRequest
        {
            Name = name,
            SecretString = secretString,
            Description = description
        };

        return await _secretsManager.CreateSecretAsync(request, cancellationToken);
    }

    public async Task<UpdateSecretResponse> UpdateSecretAsync(string secretId, string newSecretString,
        string? description, CancellationToken cancellationToken = default)
    {
        var request = new UpdateSecretRequest
        {
            SecretId = secretId,
            SecretString = newSecretString,
            Description = description
        };

        return await _secretsManager.UpdateSecretAsync(request, cancellationToken);
    }

    /// <summary>
    /// Metodo para listar secretos disponibles 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<SecretListEntry>> ListSecretsAsync(CancellationToken cancellationToken = default)
    {
        var secrets = new List<SecretListEntry>();
        string? nextToken = null;

        do
        {
            var request = new ListSecretsRequest
            {
                MaxResults = 100,
                NextToken = nextToken
            };

            try
            {
                var response = await _secretsManager.ListSecretsAsync(request, cancellationToken);
                if (response.SecretList != null)
                {
                    secrets.AddRange(response.SecretList);
                }

                nextToken = response.NextToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing secrets from AWS");
                throw;
            }
        } while (!string.IsNullOrEmpty(nextToken));

        _logger.LogInformation("Fetched {Count} secrets from AWS", secrets.Count);
        return secrets;
    }
}