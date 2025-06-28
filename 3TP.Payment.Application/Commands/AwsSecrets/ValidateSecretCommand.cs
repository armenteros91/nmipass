using MediatR;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets
{
    public class ValidateSecretCommand : IRequest<bool>
    {
        public string SecretId { get; }
        public string SecretToValidate { get; }

        public ValidateSecretCommand(string secretId, string secretToValidate)
        {
            SecretId = secretId;
            SecretToValidate = secretToValidate;
        }
    }
}
