using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;

namespace ThreeTP.Payment.Application.Interfaces;

public interface INmiPaymentGateway
{
    Task<NmiResponseDto> SendAsync<TRequest>(TRequest dto) where TRequest : BaseTransactionRequestDto;   
    Task<QueryResponseDto> QueryAsync(QueryTransactionRequestDto dto);
}