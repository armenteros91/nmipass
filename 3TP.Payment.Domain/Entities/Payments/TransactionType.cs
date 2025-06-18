using ThreeTP.Payment.Domain.Commons;

namespace ThreeTP.Payment.Domain.Entities.Payments;

public class TransactionType :BaseEntity
{
    public Guid TypeTransactionsId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; }
    
    public ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();
}