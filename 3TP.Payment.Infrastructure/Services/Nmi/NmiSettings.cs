using System.ComponentModel.DataAnnotations;

namespace ThreeTP.Payment.Infrastructure.Services.Nmi;

public class NmiSettings 
{
    [Required] public required string BaseUrl { get; init; }
     public string? SecurityKey { get; init; } // Nullable

    [Required] public required EndpointSettings Endpoint { get; init; }
    public required QuerySettings Query { get; init; }

    public NmiSettings Value { get; }
}

public class EndpointSettings
{
    public required string Transaction { get; init; }
}

public class QuerySettings
{
    public required string QueryApi { get; init; }
}