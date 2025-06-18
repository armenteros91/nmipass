using ThreeTP.Payment.Application.Common.Responses;

namespace ThreeTP.Payment.Application.Common.Exceptions;

/// <summary>
/// Excepción personalizada para errores de validación
/// </summary>
public sealed class CustomValidationException : Exception
{
    public ValidationErrorResponse Errors { get; }

    public CustomValidationException(ValidationErrorResponse errors)
        : base("Validation errors occurred")
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    public CustomValidationException(
        ValidationErrorResponse errors,
        Exception innerException)
        : base("Validation errors occurred", innerException)
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }
}