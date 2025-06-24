using System.Collections.Concurrent;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces.aws;

namespace ThreeTP.Payment.Application.Services;

public class AwsSecretCacheService : IAwsSecretCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AwsSecretCacheService> _logger;
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public AwsSecretCacheService(IMemoryCache cache, ILogger<AwsSecretCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
    }

    public async Task<GetSecretValueResponse> GetOrFetchAsync(string secretId, string? versionId, string? versionStage, Func<Task<GetSecretValueResponse>> fetchFunc, bool forceRefresh = false)
    {
        var cacheKey = $"secret_{secretId}_{versionId}_{versionStage}";

        if (!forceRefresh && _cache.TryGetValue(cacheKey, out GetSecretValueResponse cached))
        {
            _logger.LogDebug("Retrieved secret {SecretId} from cache", secretId);
            return cached;
        }

        var result = await fetchFunc();
        _cache.Set(cacheKey, result, _cacheOptions);
        _cacheKeys.TryAdd(cacheKey, true);
        _logger.LogInformation("Fetched secret {SecretId} from AWS and cached it", secretId);
        return result;
    }

    public void Invalidate(string secretId)
    {
        var keysToRemove = _cacheKeys.Keys
            .Where(k => k.StartsWith($"secret_{secretId}_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            _logger.LogDebug("Invalidated cache for key {CacheKey}", key);
        }
    }
}
