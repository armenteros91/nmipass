using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.Responses.BIN_Checker;
using ThreeTP.Payment.Application.Interfaces.Services; // Changed to use interface

namespace ThreeTP.Payment.Application.Queries.Payments
{
    public class GetBinLookupQueryHandler : IRequestHandler<GetBinLookupQuery, BinlookupResponse>
    {
        private readonly IBinLookupService _binLookupService; // Depend on the interface

        public GetBinLookupQueryHandler(IBinLookupService binLookupService) // Inject the interface
        {
            _binLookupService = binLookupService;
        }

        public async Task<BinlookupResponse> Handle(GetBinLookupQuery request, CancellationToken cancellationToken)
        {
            return await _binLookupService.GetBinLookupAsync(request.Bin);
        }
    }
}
