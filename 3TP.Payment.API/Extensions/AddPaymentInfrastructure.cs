using System.Net;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Formatting.Compact;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.aws;
using ThreeTP.Payment.Application.Interfaces.Payment;
using ThreeTP.Payment.Application.Interfaces.Repository;
using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Application.Interfaces.Terminals;
using ThreeTP.Payment.Application.Mappings;
using ThreeTP.Payment.Application.Options;
using ThreeTP.Payment.Application.Services;
using ThreeTP.Payment.Infrastructure.Persistence;
using ThreeTP.Payment.Infrastructure.Persistence.Repositories;
using ThreeTP.Payment.Infrastructure.Services.Encryption;
using ThreeTP.Payment.Infrastructure.Services.Neutrino;
using ThreeTP.Payment.Infrastructure.Services.Nmi;

namespace ThreeTP.Payment.API.Extensions;

public static class PaymentInfrastructureExtensions
{
    /// <summary>
    /// Registers all necessary services for the payment infrastructure, including DbContext, automatic migrations,
    /// Unit of Work, repositories, AutoMapper, AWS Secrets Manager, NMI payment services, Neutrino services, and logging.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration provider for accessing connection strings and other settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if required configurations are missing.</exception>
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add database services and configure automatic migrations
        services.AddDatabaseServices(configuration);

        // Add repository and unit of work services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITerminalRepository, TerminalRepository>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        services.AddScoped<ITerminalService, TerminalService>();

        // Add encryption service
        services.AddScoped<IEncryptionService, AesEncryptionService>();
        services.AddScoped<ISecretValidationService, SecretValidationService>();

        // Add AutoMapper with NMI mapping profile
        services.AddAutoMapper(typeof(NmiMappingProfile).Assembly);

        // Add AWS Secrets Manager services
        services.AddAwsSecretManagerServices(configuration);

        // Add NMI payment gateway services
        services.AddNmiPaymentServices(configuration);

        // Add Neutrino services
        services.AddNeutrinoServices(configuration);

        //DBContextFactory
        //services.AddSingleton<INmiDbContextFactory, NmiDbContextFactory>();

        // Add logging services
        services.AddLoggingServices(configuration);

        return services;
    }

    private static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NmiDb")
                               ?? throw new InvalidOperationException(
                                   "The 'NmiDb' connection string is missing or not configured.");

        // Register NmiDbContext with dependency injection
        services.AddDbContext<NmiDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure()));

        // Register DbContextFactory for scenarios requiring factory-based instantiation
        // services.AddDbContextFactory<NmiDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("NmiDb")));

        // Register a startup service to apply migrations automatically
      //  services.AddHostedService<DatabaseMigrationService>();
    }

    private static void AddAwsSecretManagerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AwsSecretManagerOptions>(configuration.GetSection("AwsSecrets"));
        services.AddSingleton<IAmazonSecretsManager>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<AwsSecretManagerOptions>>().Value;
            var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);
            var region = Amazon.RegionEndpoint.GetBySystemName(config.DefaultRegion);
            return new AmazonSecretsManagerClient(credentials, region);
        });
        services.AddMemoryCache();
        services.AddScoped<IAwsSecretManagerService, AwsSecretManagerService>();
        services.AddScoped<IAwsSecretCacheService, AwsSecretCacheService>();
        
        //aws secret 
        services.AddScoped<IAwsSecretCacheService, AwsSecretCacheService>();
        services.AddScoped<IAwsSecretsProvider, AwsSecretsProvider>();
        services.AddScoped<IAwsSecretSyncService, AwsSecretSyncService>();
    }

    private static void AddNmiPaymentServices(this IServiceCollection services, IConfiguration configuration)
    {
        var nmiSettings = configuration.GetSection("NmiSettings").Get<NmiSettings>()
                          ?? throw new InvalidOperationException("NmiSettings configuration is missing.");

        if (string.IsNullOrEmpty(nmiSettings.BaseURL))
            throw new InvalidOperationException("NmiSettings:BaseURL is required.");
        if (nmiSettings.Endpoint?.Transaction == null)
            throw new InvalidOperationException("NmiSettings:Endpoint:Transaction is required.");
        if (nmiSettings.Query?.QueryApi == null)
            throw new InvalidOperationException("NmiSettings:Query:QueryApi is required.");

        services.AddSingleton(nmiSettings);
        services.AddHttpClient<INmiPaymentGateway, NmiPaymentService>(client =>
        {
            client.BaseAddress = new Uri(nmiSettings.BaseURL);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "NMIPaymentSDK/1.0");
        });
    }

    private static void AddNeutrinoServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NeutrinoApiOptions>(configuration.GetSection("NeutrinoApi"));
        services.AddHttpClient<BinLookupService>();
        services.AddHttpContextAccessor();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            var proxyIp = configuration["ProxyIp"] ?? "3.214.46.100"; // Use configuration for flexibility
            options.KnownProxies.Add(IPAddress.Parse(proxyIp));
        });
    }

    private static void AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#endif
            .MinimumLevel.Information()
            //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Reducir el ruido de Microsoft logs
            //.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo
            .Seq(configuration["Seq:Url"] ??
                 "http://localhost:5341") // throw new InvalidOperationException("Seq:Url configuration is missing."))
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .WriteTo.Console()
            .WriteTo.File(
                path: configuration["Logging:FilePath"] ?? "logs/log-.json",
                rollingInterval: RollingInterval.Day,
                formatter: new CompactJsonFormatter(),
                retainedFileCountLimit: 31)
            .CreateLogger();

        services.AddLogging(logging => logging.AddSerilog());
    }
}

/// <summary>
/// Hosted service to apply database migrations at application startup.
/// </summary>
// public class DatabaseMigrationService : IHostedService
// {
//     private readonly IServiceProvider _serviceProvider;
//     private readonly ILogger<DatabaseMigrationService> _logger;
//
//     public DatabaseMigrationService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationService> logger)
//     {
//         _serviceProvider = serviceProvider;
//         _logger = logger;
//     }
//
// //todo:comentado por seguridad 
//     /// <summary>
//     /// 
//     /// </summary>
//     /// <param name="cancellationToken"></param>
//     /// <returns></returns>
//     // public async Task StartAsync(CancellationToken cancellationToken)
//     // {
//     //     using var scope = _serviceProvider.CreateScope();
//     //     var context = scope.ServiceProvider.GetRequiredService<NmiDbContext>();
//     //     try
//     //     {
//     //         _logger.LogInformation("Applying database migrations for NmiDbContext...");
//     //        // await context.Database.MigrateAsync(cancellationToken);
//     //         _logger.LogInformation("Database migrations applied successfully.");
//     //     }
//     //     catch (Exception ex)
//     //     {
//     //         _logger.LogError(ex, "Failed to apply database migrations.");
//     //         throw; // Rethrow to prevent the application from starting with an inconsistent database
//     //     }
//     // }
//     public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
// }