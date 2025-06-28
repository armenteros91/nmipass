using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.aws;
using ThreeTP.Payment.Application.Interfaces.aws;

namespace ThreeTP.Payment.Application.Queries.AwsSecrets
{
    public class ListSecretsQueryHandler : IRequestHandler<ListSecretsQuery, List<SecretSummary>>
    {
        private readonly IAwsSecretsProvider _awsSecretsProvider;
        private readonly ILogger<ListSecretsQueryHandler> _logger;

        public ListSecretsQueryHandler(IAwsSecretsProvider awsSecretsProvider, ILogger<ListSecretsQueryHandler> logger)
        {
            _awsSecretsProvider = awsSecretsProvider;
            _logger = logger;
        }

        public async Task<List<SecretSummary>> Handle(ListSecretsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var entries = await _awsSecretsProvider.ListSecretsAsync(cancellationToken);

                var summaries = entries.Select(e => new SecretSummary
                {
                    SecretId = e.ARN, // Using ARN as the unique identifier
                    Name = e.Name,
                    Description = e.Description,
                    LastModified = e.LastChangedDate
                }).ToList();

                return summaries;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secrets from AWS");
                // Depending on requirements, you might throw a custom exception or return an empty list.
                // For now, rethrowing to let the global exception handler catch it.
                throw;
            }
        }
    }
}
