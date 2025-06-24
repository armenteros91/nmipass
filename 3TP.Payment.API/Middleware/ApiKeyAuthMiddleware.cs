using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.Tenants;

namespace ThreeTP.Payment.API.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantService _tenantService;

        public ApiKeyAuthMiddleware(RequestDelegate next, ITenantService tenantService)
        {
            _next = next;
            _tenantService = tenantService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key no proporcionada");
                return;
            }

            var tenant = await _tenantService.ValidateByApiKeyAsync(apiKey!);
            if (tenant == null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Tenant no válido");
                return;
            }

            context.Items["Tenant"] = tenant;  // Para uso en controllers
            await _next(context);
        }
    }
}
