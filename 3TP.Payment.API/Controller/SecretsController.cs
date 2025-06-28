using Amazon.SecretsManager.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using ThreeTP.Payment.Application.DTOs.aws;
using ThreeTP.Payment.Application.Queries.AwsSecrets;

namespace ThreeTP.Payment.API.Controller;

[ApiController]
[Route("api/secrets")]
public class SecretsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SecretsController> _logger;

    public SecretsController(IMediator mediator, ILogger<SecretsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a secret value from AWS Secrets Manager.
    /// </summary>
    [HttpGet("{secretId}")]
    [ProducesResponseType(typeof(GetSecretValueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSecret(string secretId, [FromQuery] string? versionId, [FromQuery] string? versionStage, [FromQuery] bool forceRefresh = false)
    {
        try
        {
            var query = new GetSecretValueQuery(secretId, versionId, versionStage, forceRefresh);
            var result = await _mediator.Send(query);
            return result != null ? Ok(result) : NotFound();
        }
        catch (ResourceNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret {SecretId}", secretId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the secret.");
        }
    }

    /// <summary>
    /// Creates a new secret in AWS Secrets Manager.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateSecretResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSecret([FromBody] CreateSecretCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSecret), new { secretId = result.ARN }, result);
        }
        catch (InvalidOperationException ex) // Catch specific exception for existing secret
        {
            _logger.LogWarning(ex, "Attempted to create an already existing secret {SecretName}", command.Name);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secret {SecretName}", command.Name);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the secret.");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SecretSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSecrets(CancellationToken cancellationToken)
    {
        try
        {
            var query = new ListSecretsQuery();
            var summaries = await _mediator.Send(query, cancellationToken);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secrets from AWS");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving secrets.");
        }
    }

    [HttpPost("decrypt")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DecryptSecret([FromBody] DecryptSecretRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.EncryptedSecret))
        {
            return BadRequest("Encrypted secret cannot be empty.");
        }

        try
        {
            var command = new DecryptSecretCommand(request.EncryptedSecret);
            var decryptedSecret = await _mediator.Send(command);
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
    // Removed 404 as the command handler should deal with not found logic if necessary,
    // or it's a validation failure (false) rather than resource not found.
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateSecret([FromBody] SecretValidationRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SecretId) ||
            string.IsNullOrWhiteSpace(request.SecretToValidate))
        {
            return BadRequest("Secret ID and secret to validate cannot be empty.");
        }

        try
        {
            var command = new ValidateSecretCommand(request.SecretId, request.SecretToValidate);
            var isValid = await _mediator.Send(command);
            return Ok(isValid); // Directly return the boolean result from the handler
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating secret for ID {SecretId}.", request.SecretId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while validating the secret.");
        }
    }
}

// Request DTOs for actions that take simple parameters in the body
public class DecryptSecretRequest
{
    public string? EncryptedSecret { get; set; }
}

public class SecretValidationRequest
{
    public string? SecretId { get; set; }
    public string? SecretToValidate { get; set; }
}