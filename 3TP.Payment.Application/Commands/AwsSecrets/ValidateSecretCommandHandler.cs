using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Interfaces;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets
{
    public class ValidateSecretCommandHandler : IRequestHandler<ValidateSecretCommand, bool>
    {
        private readonly ISecretValidationService _secretValidationService;

        public ValidateSecretCommandHandler(ISecretValidationService secretValidationService)
        {
            _secretValidationService = secretValidationService;
        }

        public async Task<bool> Handle(ValidateSecretCommand request, CancellationToken cancellationToken)
        {
            return await _secretValidationService.ValidateSecretAsync(request.SecretId, request.SecretToValidate);
        }
    }
}
