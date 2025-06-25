using System.Reflection;
using AutoMapper; // Added
using FluentValidation;
using MediatR;
using ThreeTP.Payment.Application.Behaviors;
using ThreeTP.Payment.Application.Commands.AwsSecrets; // For CreateSecretCommandHandler
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.aws;
using ThreeTP.Payment.Application.Interfaces.Payment;
using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Application.Interfaces.Terminals;
using ThreeTP.Payment.Application.Services;
using ThreeTP.Payment.Infrastructure.Events;

namespace ThreeTP.Payment.API.Installers;

/// <summary>
/// Provides extension methods for registering application services in the dependency injection container.
/// </summary>
public static class ApplicationInstaller
{
    /// <summary>
    /// Registers all necessary services for the application layer, including MediatR, FluentValidation,
    /// pipeline behaviors, repositories, and domain event dispatching.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 1. Configure MediatR and register handlers from the Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));

        //register mediaTR
        services.AddMediatR(cfg =>
        {
           cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });



        // 2. Configure AutoMapper and register profiles from the Application assembly
        services.AddAutoMapper(typeof(Application.AssemblyReference).Assembly); // Added

        // 3. Configure FluentValidation and register validators from the Application assembly
        services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly);
        
        // 4. Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // Note: TransactionBehavior<,> Comentado  x si se llegara a utilizar 
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // 5. Register domain event dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // 6. Register Application Services
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITenantService, TenantService>(); // Added based on existence
        services.AddScoped<ITerminalService, TerminalService>(); // Added
        services.AddScoped<IAwsSecretManagerService, AwsSecretManagerService>(); // Added based on existence
        
        return services;
    }
}