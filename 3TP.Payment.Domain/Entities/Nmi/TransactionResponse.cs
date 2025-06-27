using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Domain.Entities.Payments;

namespace ThreeTP.Payment.Domain.Entities.Nmi;

/// <summary>
/// Entidad de base datos
/// </summary>
public class TransactionResponse :BaseEntity
{
    public Guid TransaccionResponseId { get; set; }
    public int Response { get; set; } // valor de respuesta NMI
    public string? ResponseText { get; set; }
    public string? TransactionId { get; set; }//Respuesta de pasarela
    public string? AuthCode { get; set; }
    public string? AvsResponse { get; set; }
    public string? CvvResponse { get; set; }
    public string? OrderId { get; set; }
    public string? ResponseCode { get; set; }
    public string? EmvAuthResponseData { get; set; }

    // Conditional fields
    public string? CustomerVaultId { get; set; }
    public string? KountScore { get; set; }
    public string? MerchantAdviceCode { get; set; }

    public Guid FkTransactionsId { get; set; }//foreing key de transacciones
    public Transactions Transaction { get; set; }
}