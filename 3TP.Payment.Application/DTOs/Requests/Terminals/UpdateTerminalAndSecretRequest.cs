namespace ThreeTP.Payment.Application.DTOs.Requests.Terminals;

public class UpdateTerminalAndSecretRequest
{
    public UpdateTerminalRequestDto TerminalUpdate { get; set; } = default!;
    public UpdateSecretPartDto? SecretUpdate { get; set; }
}

public class UpdateSecretPartDto
{
    public string SecretId { get; set; } = default!;
    public string NewSecretString { get; set; } = default!;
    public string? SecretDescription { get; set; }
}