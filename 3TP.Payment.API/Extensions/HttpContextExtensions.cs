namespace ThreeTP.Payment.API.Extensions;

public static class HttpContextExtensions
{
    public static string? GetClientIp(this HttpContext context)
    {
        // use  UseForwardedHeaders, RemoteIpAddress es ahora la IP del cliente real
        return context.Connection.RemoteIpAddress?.ToString();
    }
}