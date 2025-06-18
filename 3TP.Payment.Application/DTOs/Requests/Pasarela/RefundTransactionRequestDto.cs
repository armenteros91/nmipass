using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThreeTP.Payment.Application.DTOs.Requests.Pasarela;

public class RefundTransactionRequestDto : BaseTransactionRequestDto
{
    public RefundTransactionRequestDto() => TypeTransaction = "refund";

    [Required]
    [JsonPropertyName("transactionid")]
    public string TransactionId { get; set; } = default!;
}