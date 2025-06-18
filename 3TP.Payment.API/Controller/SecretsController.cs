using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Queries.AwsSecrets;

namespace ThreeTP.Payment.API.Controller;

[ApiController]
[Route("api/secrets")]
public class SecretsController : ControllerBase
{
    private readonly IAwsSecretManagerService _awsSecretManagerService;

    public SecretsController(IAwsSecretManagerService awsSecretManagerService)
    {
        _awsSecretManagerService = awsSecretManagerService;
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
}