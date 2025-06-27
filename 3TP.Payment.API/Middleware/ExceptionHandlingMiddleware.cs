using ThreeTP.Payment.Application.Common.Exceptions;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Continue pipeline
        }
        catch (CustomValidationException ex)
        {
            _logger.LogWarning("Validation error: {ErrorCount}", ex.Errors.Errors.Count);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = 400,
                Errors = ex.Errors.Errors
                    .GroupBy(e => e.Field)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Error).ToArray())
            };

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (AppExceptionBase ex)
        {
            _logger.LogWarning("App domain exception: {ErrorCode}", ex.ErrorCode);

            context.Response.StatusCode = MapToStatusCode(ex);
            context.Response.ContentType = "application/json";

            var response = new
            {
                title = ex.Message,
                errorCode = ex.ErrorCode ?? "APP_ERROR"
            };

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                title = "Unexpected error",
                detail = ex.Message
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private static int MapToStatusCode(AppExceptionBase ex) => ex switch
    {
        TenantNotFoundException => StatusCodes.Status404NotFound,
        InvalidTenantException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status422UnprocessableEntity
    };
}