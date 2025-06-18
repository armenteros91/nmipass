// 3TP.Payment.API/Installers/InfrastructureInstaller.cs

using ThreeTP.Payment.API.Extensions;

namespace ThreeTP.Payment.API.Installers;

/// <summary>
/// Provides extension methods for registering payment infrastructure services in the dependency injection container.
/// </summary>
public static class InfrastructureInstaller
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPaymentInfrastructure(configuration);
    }
}