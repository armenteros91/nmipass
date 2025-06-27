using MediatR;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;

namespace ThreeTP.Payment.Application.Commands.Payments
{
    public class ProcessPaymentCommand : IRequest<NmiResponseDto>
    {
        public string ApiKey { get; }
        public SaleTransactionRequestDto PaymentRequest { get; }

        public ProcessPaymentCommand(string apiKey, SaleTransactionRequestDto paymentRequest)
        {
            ApiKey = apiKey;
            PaymentRequest = paymentRequest;
        }
    }
}
