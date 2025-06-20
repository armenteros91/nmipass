﻿using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Queries.AwsSecrets;

namespace ThreeTP.Payment.Application.Services
{
    public sealed class AwsSecretManagerService : IAwsSecretManagerService
    {
        private readonly IAmazonSecretsManager _secretsManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AwsSecretManagerService> _logger;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public AwsSecretManagerService(
            IAmazonSecretsManager secretsManager,
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<AwsSecretManagerService> logger,
            TimeSpan? cacheDuration = null)
        {
            _secretsManager = secretsManager ?? throw new ArgumentNullException(nameof(secretsManager));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration ?? TimeSpan.FromMinutes(15)
            };
        }

        public async Task<GetSecretValueResponse> GetSecretValueAsync(
            GetSecretValueQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var cacheKey = $"secret_{query.SecretId}_{query.VersionId}_{query.VersionStage}";

            if (!query.ForceRefresh && _cache.TryGetValue(cacheKey, out GetSecretValueResponse cachedResponse))
            {
                _logger.LogDebug("Retrieved secret {SecretId} from cache", query.SecretId);
                return cachedResponse;
            }

            var request = new GetSecretValueRequest
            {
                SecretId = query.SecretId,
                VersionId = query.VersionId,
                VersionStage = query.VersionStage
            };

            try
            {
                var response = await _secretsManager.GetSecretValueAsync(request, cancellationToken);
                _cache.Set(cacheKey, response, _cacheOptions);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret {SecretId}", query.SecretId);
                throw;
            }
        }

        public async Task<CreateSecretResponse> CreateSecretAsync(
            CreateSecretCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var request = new CreateSecretRequest
            {
                Name = command.Name,
                SecretString = command.SecretString,
                Description = command.Description
            };

            try
            {
                //1.Crear el secreto en aws 
                var response = await _secretsManager.CreateSecretAsync(request, cancellationToken);


                //2.vincular el secreto a un terminal 
                if (command.TerminalId.HasValue)
                {
                    var terminal = await _unitOfWork.TenantRepository.GetByIdAsync(command.TerminalId.Value);

                    if (terminal is not null)
                    {
                        await _unitOfWork.TerminalRepository
                            .UpdateSecretKeyAsync(
                                command.TerminalId.Value,
                                command.SecretString); // La encriptación se maneja en el repositorio
                    }
                }

                _unitOfWork.CommitAsync(cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating secret {SecretName}", command.Name);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }


        public async Task<UpdateSecretResponse> UpdateSecretAsync(
            UpdateSecretCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            try
            {
                _logger.LogInformation("Updating secret {SecretId} in AWS", command.SecretId);

                // 1. Actualizar en AWS
                var updateRequest = new UpdateSecretRequest
                {
                    SecretId = command.SecretId,
                    SecretString = command.NewSecretString,
                    Description = command.Description
                };

                var response = await _secretsManager.UpdateSecretAsync(updateRequest, cancellationToken);

                // 2. Actualizar en base de datos si corresponde
                if (command.TerminalId.HasValue)
                {
                    var terminalId = command.TerminalId.Value;

                    var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
                    if (terminal != null)
                    {
                        await _unitOfWork.TerminalRepository.UpdateSecretKeyAsync(
                            terminalId,
                            command.NewSecretString
                        );
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                InvalidateCacheForSecret(command.SecretId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating secret {SecretId}", command.SecretId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }


        private void InvalidateCacheForSecret(string secretId)
        {
            var cacheKeyPrefix = $"secret_{secretId}_";

            // Alternativa para .NET Core 3.0+
            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetProperty("EntriesCollection",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field?.GetValue(memoryCache) is ICollection<KeyValuePair<object, object>> entries)
                {
                    var keysToRemove = entries
                        .Where(e => e.Key.ToString()?.StartsWith(cacheKeyPrefix) == true)
                        .Select(e => e.Key)
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        _cache.Remove(key);
                    }
                }
            }
        }
    }
}