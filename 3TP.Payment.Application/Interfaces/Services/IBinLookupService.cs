using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.Responses.BIN_Checker;

namespace ThreeTP.Payment.Application.Interfaces.Services
{
    public interface IBinLookupService
    {
        Task<BinlookupResponse> GetBinLookupAsync(string binNumber);
    }
}
