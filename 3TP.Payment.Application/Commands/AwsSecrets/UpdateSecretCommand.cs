using Amazon.SecretsManager.Model;
using MediatR;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets;

public class UpdateSecretCommand : IRequest<string>, IRequest<UpdateSecretResponse>
{
    public string SecretId { get; set; } = null!;
    public string NewSecretString { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? TerminalId { get; set; } // Vinculación opcional
}
