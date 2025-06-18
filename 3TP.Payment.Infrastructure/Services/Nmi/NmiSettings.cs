namespace ThreeTP.Payment.Infrastructure.Services.Nmi;

public class NmiSettings
{
    public required string BaseURL { get; init; }
    public string? SecurityKey { get; init; } // Nullable
    public required EndpointSettings Endpoint { get; init; }
    public required QuerySettings Query { get; init; }
}

public class EndpointSettings
{
    public required string Transaction { get; init; }
}

public class QuerySettings
{
    public required string QueryApi { get; init; }
}