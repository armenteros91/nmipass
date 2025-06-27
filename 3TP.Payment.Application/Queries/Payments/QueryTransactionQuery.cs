using MediatR;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;

namespace ThreeTP.Payment.Application.Queries.Payments
{
    public class QueryTransactionQuery : IRequest<QueryResponseDto>
    {
        public string ApiKey { get; }
        public QueryTransactionRequestDto QueryRequest { get; }

        public QueryTransactionQuery(string apiKey, QueryTransactionRequestDto queryRequest)
        {
            ApiKey = apiKey;
            QueryRequest = queryRequest;
        }
    }
}
