namespace ThreeTP.Payment.Domain.Entities.Nmi;

/// <summary>
/// Entidad de base datos
/// </summary>
public class TransactionResponse
{
    public Guid TransaccionResponseId { get; set; }
    public int Id { get; set; } // valor de respuesta NMI

    // Standard fields
    public string? Response { get; set; }
    public string? ResponseText { get; set; }
    public string? TransactionId { get; set; }
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

    // Optional metadata
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}