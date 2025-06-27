using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Interfaces.Payment;

namespace ThreeTP.Payment.Application.Queries.Payments
{
    public class QueryTransactionQueryHandler : IRequestHandler<QueryTransactionQuery, QueryResponseDto>
    {
        private readonly IPaymentService _paymentService;

        public QueryTransactionQueryHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<QueryResponseDto> Handle(QueryTransactionQuery request, CancellationToken cancellationToken)
        {
            return await _paymentService.QueryProcessPaymentAsync(request.ApiKey, request.QueryRequest);
        }
    }
}
