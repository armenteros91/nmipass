using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Interfaces;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets
{
    public class DecryptSecretCommandHandler : IRequestHandler<DecryptSecretCommand, string>
    {
        private readonly ISecretValidationService _secretValidationService;

        public DecryptSecretCommandHandler(ISecretValidationService secretValidationService)
        {
            _secretValidationService = secretValidationService;
        }

        public async Task<string> Handle(DecryptSecretCommand request, CancellationToken cancellationToken)
        {
            return await _secretValidationService.DecryptSecretAsync(request.EncryptedSecret);
        }
    }
}
