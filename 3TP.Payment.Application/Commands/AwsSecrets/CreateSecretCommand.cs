using MediatR;
using Amazon.SecretsManager.Model;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets
{
    public sealed record CreateSecretCommand(
        string Name,
        string SecretString,
        string? Description=null,
        Guid? TerminalId=null
    ) : IRequest<CreateSecretResponse>;
}