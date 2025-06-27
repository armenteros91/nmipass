using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;

namespace ThreeTP.Payment.Application.Interfaces.Payment;

public interface INmiPaymentGateway
{
    Task<NmiResponseDto> SendAsync<TRequest>(TRequest dto) where TRequest : SaleTransactionRequestDto;   
    Task<QueryResponseDto> QueryAsync(QueryTransactionRequestDto dto);
}