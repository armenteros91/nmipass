using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Domain.Entities.Nmi;
using ThreeTP.Payment.Domain.Shared.Enums;

namespace ThreeTP.Payment.Domain.Entities.Payments;

/// <summary>
///  Entidad de base de datos Tabla principal de registro de transacciones
/// </summary>
public class Transactions : BaseEntity
{
    public Guid TransactionsId { get; set; }

    public string? paymentTransactionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid TraceId { get; set; }
    public Guid TypeTransactionsId { get; set; }
    public TransactionCodeResponse ResponseCode { get; set; }

    public Tenant.Tenant Tenant { get; set; }
    public TransactionType TransactionType { get; set; }

    // 👇 Nueva relación
    public ICollection<TransactionResponse> Responses { get; set; } = new List<TransactionResponse>();
}