using MediatR;
using System.Collections.Generic;
using ThreeTP.Payment.Application.DTOs.aws;

namespace ThreeTP.Payment.Application.Queries.AwsSecrets
{
    public class ListSecretsQuery : IRequest<List<SecretSummary>>
    {
        // No parameters needed for listing all secrets
    }
}
