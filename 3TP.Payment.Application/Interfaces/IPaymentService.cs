using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;

namespace ThreeTP.Payment.Application.Interfaces;

public interface IPaymentService
{
    Task<NmiResponseDto> ProcessPaymentAsync(string apiKey, BaseTransactionRequestDto paymentRequest);

    Task<QueryResponseDto> QueryProcessPaymentAsync(string apiKey,
        QueryTransactionRequestDto queryTransactionRequest);
}