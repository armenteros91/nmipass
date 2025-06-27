using MediatR;
using ThreeTP.Payment.Application.DTOs.Responses.BIN_Checker;

namespace ThreeTP.Payment.Application.Queries.Payments
{
    public class GetBinLookupQuery : IRequest<BinlookupResponse>
    {
        public string Bin { get; }

        public GetBinLookupQuery(string bin)
        {
            Bin = bin;
        }
    }
}
