namespace ThreeTP.Payment.Application.DTOs.Responses.Terminals
{
    public class TerminalResponseDto
    {
        public Guid TerminalId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public Guid TenantId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
