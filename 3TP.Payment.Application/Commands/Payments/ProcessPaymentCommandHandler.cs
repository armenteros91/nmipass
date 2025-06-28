using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Interfaces.Payment;

namespace ThreeTP.Payment.Application.Commands.Payments
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, NmiResponseDto>
    {
        private readonly IPaymentService _paymentService;

        public ProcessPaymentCommandHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<NmiResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            return await _paymentService.ProcessPaymentAsync(request.ApiKey, request.PaymentRequest);
        }
    }
}
