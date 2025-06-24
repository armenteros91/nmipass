namespace ThreeTP.Payment.Application.DTOs.Requests.Terminals;

public class UpdateTerminalAndSecretRequest
{
    public UpdateTerminalRequestDto TerminalUpdate { get; set; } = default!;
    public string? NewSecretString { get; set; }
    public string? SecretId { get; set; }
    public string? SecretDescription { get; set; }
}