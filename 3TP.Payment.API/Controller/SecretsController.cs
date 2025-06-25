using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using ThreeTP.Payment.Application.DTOs.aws;
using ThreeTP.Payment.Application.Interfaces.aws;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Queries.AwsSecrets;

namespace ThreeTP.Payment.API.Controller;

[ApiController]
[Route("api/secrets")]
public class SecretsController : ControllerBase
{
    private readonly IAwsSecretManagerService _awsSecretManagerService;
    private readonly ILogger<SecretsController> _logger;
    private readonly IAwsSecretsProvider _awsSecretsProvider;
    private readonly ISecretValidationService _secretValidationService;

    public SecretsController(
        IAwsSecretManagerService awsSecretManagerService,
        ILogger<SecretsController> logger,
        IAwsSecretsProvider awsSecretsProvider,
        ISecretValidationService secretValidationService)
    {
        _awsSecretManagerService = awsSecretManagerService;
        _logger = logger;
        _awsSecretsProvider = awsSecretsProvider;
        _secretValidationService = secretValidationService;
    }

    /// <summary>
    /// Retrieves a secret value from AWS Secrets Manager.
    /// </summary>
    [HttpGet("{secretId}")]
    [ProducesResponseType(typeof(GetSecretValueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSecret([FromRoute] GetSecretValueQuery secretQueryvalue)
    {
        var result = await _awsSecretManagerService.GetSecretValueAsync(secretQueryvalue);

        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Creates a new secret in AWS Secrets Manager.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateSecretResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSecret([FromBody] CreateSecretCommand command)
    {
        var result = await _awsSecretManagerService.CreateSecretAsync(command);
        return CreatedAtAction(nameof(GetSecret), new { secretId = result.ARN }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SecretSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSecrets(CancellationToken cancellationToken)
    {
        try
        {
            var entries = await _awsSecretsProvider.ListSecretsAsync(cancellationToken);

            var summaries = entries.Select(e => new SecretSummary
            {
                SecretId = e.ARN, // ← Se asigna el ARN como identificador
                Name = e.Name,
                Description = e.Description,
                LastModified = e.LastChangedDate
            }).ToList();

            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secrets from AWS");
            return StatusCode(500, "An error occurred while retrieving secrets.");
        }
    }

    [HttpPost("decrypt")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DecryptSecret([FromBody] string encryptedSecret)
    {
        if (string.IsNullOrWhiteSpace(encryptedSecret))
        {
            return BadRequest("Encrypted secret cannot be empty.");
        }

        try
        {
            var decryptedSecret = await _secretValidationService.DecryptSecretAsync(encryptedSecret);
            return Ok(decryptedSecret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting secret.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while decrypting the secret.");
        }
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateSecret([FromBody] SecretValidationRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SecretId) || string.IsNullOrWhiteSpace(request.SecretToValidate))
        {
            return BadRequest("Secret ID and secret to validate cannot be empty.");
        }

        try
        {
            var isValid = await _secretValidationService.ValidateSecretAsync(request.SecretId, request.SecretToValidate);
            if (!isValid)
            {
                // Consider if a specific status code for invalid secret (but operation successful) is more appropriate
                // For now, returning OK with false, or could be NotFound if the secretId implies a resource not matching.
                return Ok(false);
            }
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating secret for ID {SecretId}.", request.SecretId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while validating the secret.");
        }
    }
}

public class SecretValidationRequest
{
    public string SecretId { get; set; }
    public string SecretToValidate { get; set; }
}