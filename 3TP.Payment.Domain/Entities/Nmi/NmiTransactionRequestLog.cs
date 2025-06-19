using ThreeTP.Payment.Domain.Commons;

namespace ThreeTP.Payment.Domain.Entities.Nmi;

public class NmiTransactionRequestLog :BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string PayloadJson { get; set; } = default!;
    public string OrderId { get; set; } = default!;
    
    public string  RawContent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<NmiTransactionResponseLog> Responses { get; set; } = new List<NmiTransactionResponseLog>();
}