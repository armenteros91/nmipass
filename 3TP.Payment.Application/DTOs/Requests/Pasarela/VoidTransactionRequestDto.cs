using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThreeTP.Payment.Application.DTOs.Requests.Pasarela;

public class VoidTransactionRequestDto : BaseTransactionRequestDto
{
    public VoidTransactionRequestDto() => TypeTransaction = "void";

    /// <summary>
    /// Original payment gateway transaction id
    /// </summary>
    [Required]
    [JsonPropertyName("transactionid")]
    public string TransactionId { get; set; } = default!;

    /// <summary>
    /// Reason the EMV transaction is being voided.
    /// Values: 'fraud', 'user_cancel', 'icc_rejected', 'icc_card_removed', 'icc_no_confirmation', or 'pos_timeout'
    /// </summary>
    [JsonPropertyName("void_reason")]
    public string? VoidReason { get; set; }
}