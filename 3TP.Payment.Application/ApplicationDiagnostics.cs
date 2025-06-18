using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;

namespace ThreeTP.Payment.Application;

public static class ApplicationDiagnostics
{
    public static void VerifyServices(IServiceProvider provider)
    {
        var mediator = provider.GetService<IMediator>();
        if (mediator == null)
            throw new InvalidOperationException("MediatR is not registered.");

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<BaseTransactionRequestDto>>();
        if (validator == null)
            throw new InvalidOperationException("Validators are not registered.");
        
        //todo: Implementar en el futuro el comportamiento
        // var behaviors = provider.GetServices<IPipelineBehavior<BasePayment, PaymentResponse>>();
        // if (!behaviors.Any())
        //     throw new InvalidOperationException("Pipeline behaviors are not registered.");
    }
}