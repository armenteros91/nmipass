using Amazon.SecretsManager.Model;

namespace ThreeTP.Payment.Application.Interfaces.aws;

public interface IAwsSecretCacheService
{
   
    Task<GetSecretValueResponse> GetOrFetchAsync(string secretId, string? versionId, string? versionStage, Func<Task<GetSecretValueResponse>> fetchFunc, bool forceRefresh = false);
    void Invalidate(string secretId);
 
}