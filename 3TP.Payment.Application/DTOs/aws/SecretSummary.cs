namespace ThreeTP.Payment.Application.DTOs.aws;

public class SecretSummary
{
    public string SecretId { get; set; } = default!;
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime? LastModified { get; set; }
}

