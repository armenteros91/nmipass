using Serilog;
using ThreeTP.Payment.API.Installers;
using ThreeTP.Payment.API.Middleware;
using ThreeTP.Payment.Application;
using ThreeTP.Payment.Infrastructure.Loggin;

var builder = WebApplication.CreateBuilder(args);

// Step 1: Replace the default logging provider with Serilog
builder.Host.UseSerilog(); // Usa Serilog como el proveedor de logging

// Add services to the container
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
    
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddControllers();
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var provider = app.Services.GetService<IServiceProvider>();
if (provider != null) ApplicationDiagnostics.VerifyServices(provider);

// Log application startup
ApplicationLogger.LogStartup(builder.Environment.EnvironmentName);

//Middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();
// app.UseMiddleware<ApiKeyAuthMiddleware>();

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush(); // Asegura que todos los logs se escriban antes de cerrar
}