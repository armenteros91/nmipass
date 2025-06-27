using MediatR;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets
{
    public class DecryptSecretCommand : IRequest<string>
    {
        public string EncryptedSecret { get; }

        public DecryptSecretCommand(string encryptedSecret)
        {
            EncryptedSecret = encryptedSecret;
        }
    }
}
