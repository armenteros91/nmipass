using Amazon.SecretsManager.Model;
using MediatR;
using ThreeTP.Payment.Application.Interfaces;

namespace ThreeTP.Payment.Application.Queries.AwsSecrets
{
    internal sealed class GetSecretValueQueryHandler
         : IRequestHandler<GetSecretValueQuery, GetSecretValueResponse>
    {
        private readonly IAwsSecretManagerService _service;

        public GetSecretValueQueryHandler(IAwsSecretManagerService service)
        {
            _service = service;
        }

        public Task<GetSecretValueResponse> Handle(
            GetSecretValueQuery request,
            CancellationToken cancellationToken)
        {
            return _service.GetSecretValueAsync(request, cancellationToken);
        }
    }
}
