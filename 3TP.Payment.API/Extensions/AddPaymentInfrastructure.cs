using System.Net;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Formatting.Compact;
using ThreeTP.Payment.Application.Interfaces;
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
    /// Registers all necessary services for the payment infrastructure, including DbContext, Unit of Work, repositories,
    /// AutoMapper, and NMI payment services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration provider for accessing connection strings and other settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the 'NmiDb' connection string is missing.</exception>
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Retrieve the connection string for the NMI database
        var connectionString = configuration.GetConnectionString("NmiDb")
                               ?? throw new InvalidOperationException(
                                   "The 'NmiDb' connection string is missing or not configured.");

        // Register the DbContext factory for NmiDbContext
        services.AddDbContextFactory<NmiDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(); // Adds resilience to transient failures
            }));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITerminalRepository, TerminalRepository>();
        services.AddScoped<IEncryptionService, AesEncryptionService>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();


        //servicios 
        services.AddScoped<ITerminalService, TerminalService>();


        // Register AutoMapper with the NMI mapping profile
        services.AddAutoMapper(typeof(NmiMappingProfile).Assembly);

        //Registrar configuración y servicios para AwsSecretManagerService
        services.Configure<AwsSecretManagerOptions>(
            configuration.GetSection("AwsSecrets")
        );

        // 2. Inyecta IAmazonSecretsManager con credenciales explícitas
        services.AddSingleton<IAmazonSecretsManager>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<AwsSecretManagerOptions>>().Value;

            var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);
            var region = Amazon.RegionEndpoint.GetBySystemName(config.DefaultRegion);

            return new AmazonSecretsManagerClient(credentials, region);
        });


        //implementation services aws secret  
        services.AddMemoryCache(); // IMemoryCache
        services.AddAWSService<IAmazonSecretsManager>();
        services.AddScoped<IAwsSecretManagerService, AwsSecretManagerService>();


        // Retrieve NMI configuration (e.g., API URL, API Key)
        // Bind NmiSettings from configuration
        var nmiSettings = configuration.GetSection("NmiSettings").Get<NmiSettings>()
                          ?? throw new InvalidOperationException("NmiSettings configuration is missing.");

        // Validate required properties
        if (string.IsNullOrEmpty(nmiSettings.BaseURL))
            throw new InvalidOperationException("NmiSettings:BaseURL is required.");
        if (nmiSettings.Endpoint?.Transaction == null)
            throw new InvalidOperationException("NmiSettings:Endpoint:Transaction is required.");
        if (nmiSettings.Query?.QueryApi == null)
            throw new InvalidOperationException("NmiSettings:Query:QueryApi is required.");

        // Register as singleton
        services.AddSingleton(nmiSettings);

        services.AddHttpClient<INmiPaymentGateway, NmiPaymentService>(client =>
        {
            client.BaseAddress = new Uri(nmiSettings.BaseURL);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "NMIPaymentSDK/1.0");
        });

        #region AWS secretManager Old

        //configuracion de secret manager 
        // Fetch SecurityKey from AWS Secrets Manager
        // var secretsManagerClient = new AmazonSecretsManagerClient();
        // var secretRequest = new GetSecretValueRequest
        // {
        //     SecretId = configuration["NmiSettings:SecurityKeySecretId"] // Store SecretId in configuration
        // };
        //
        // try
        // {
        //     var secretResponse = secretsManagerClient.GetSecretValueAsync(secretRequest).GetAwaiter().GetResult();
        //     nmiSettings = new NmiSettings
        //     {
        //         BaseURL = nmiSettings.BaseURL,
        //         SecurityKey = secretResponse.SecretString,
        //         Endpoint = nmiSettings.Endpoint,
        //         Query = nmiSettings.Query
        //     };
        // }
        // catch (Exception ex)
        // {
        //     throw new InvalidOperationException("Failed to retrieve SecurityKey from AWS Secrets Manager.", ex);
        // }

        #endregion


        #region New config  Neutrino endoint Service

        // Neutrino service 
        services.AddScoped<BinLookupService>();

        // Configura la política segura para confiar solo en proxies reales del clúster
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Limpia cualquier proxy de confianza por defecto
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            // 🔐 IP de tu Ingress Controller o Router de OpenShift
            options.KnownProxies.Add(
                IPAddress.Parse("3.214.46.100")); // KnownProxies  from pod curl -s http://checkip.amazonaws.com
        });
        // Configure NeutrinoApiOptions
        services.Configure<NeutrinoApiOptions>(configuration.GetSection("NeutrinoApi"));
        services.AddSingleton<HttpClient>();
        services.AddHttpClient<BinLookupService>();
        //HttpContext Accesor
        services.AddHttpContextAccessor();

        #endregion

        services.AddLogging(logging =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Reducir el ruido de Microsoft logs
                    //.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.Seq(configuration["Seq:Url"] ?? "http://localhost:5341")
                    .Enrich.FromLogContext() // Permite añadir propiedades dinámicas al contexto
                    .Enrich.WithMachineName() // Añade el nombre de la máquina
                    .Enrich.WithEnvironmentName() // Añade el nombre del entorno (Development, Production, etc.)
                    .Enrich.WithThreadId() // Añade el ID del hilo
                    .Enrich.WithProcessId() // Añade el ID del proceso
                    .WriteTo.Console() // Escribe logs en la consola (útil para desarrollo)
                    .WriteTo.File(
                        path: configuration["Logging:FilePath"] ??
                              "logs/log-.json", // Ruta del archivo de log con rotación diaria
                        rollingInterval: RollingInterval.Day, // Rotación diaria
                        formatter: new CompactJsonFormatter(), // Formato JSON compacto
                        retainedFileCountLimit: 31 // Retiene logs de los últimos 31 días
                    )
                    .CreateLogger();
                logging.AddSerilog();
            }
        );

        return services;
    }
}