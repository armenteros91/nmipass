using Serilog;

namespace ThreeTP.Payment.Infrastructure.Loggin;

public static class ApplicationLogger
{
    public static void LogStartup(string environment)
    {
        Log.Information("Application started at {StartupTime} in {Environment} environment", DateTime.UtcNow,
            environment);
    }
}