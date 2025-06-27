using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;

namespace ThreeTP.Payment.Application.Interfaces.Payment;

public interface IPaymentService
{
    Task<NmiResponseDto> ProcessPaymentAsync(string apiKey, SaleTransactionRequestDto paymentRequest);

    Task<QueryResponseDto> QueryProcessPaymentAsync(string apiKey, QueryTransactionRequestDto queryTransactionRequest);
}