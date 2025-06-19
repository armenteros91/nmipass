namespace ThreeTP.Payment.Application.DTOs.Requests.Terminals
{
    public class CreateTerminalRequestDto
    {
        public string Name { get; set; } = null!;
        public Guid TenantId { get; set; }
        public string SecretKey { get; set; } = null!;
    }
}
