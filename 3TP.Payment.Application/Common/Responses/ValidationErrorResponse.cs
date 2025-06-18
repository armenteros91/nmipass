namespace ThreeTP.Payment.Application.Common.Responses;

public sealed record ValidationErrorResponse
{
    public string Title { get; init; } = "Validation Error";
    public int Status { get; init; } = 400; // Usar el valor numérico directamente
    public IReadOnlyList<ValidationErrorItem> Errors { get; init; } = [];
}

public sealed record ValidationErrorItem
{
    public required string Field { get; init; }
    public required string Error { get; init; }
    public string? ErrorCode { get; init; }
}