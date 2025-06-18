using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThreeTP.Payment.Application.DTOs.Requests.Pasarela;

public class CaptureTransactionRequestDto : BaseTransactionRequestDto
{
    public CaptureTransactionRequestDto() => TypeTransaction = "capture";

    /// <summary>
    /// Original payment gateway transaction id
    /// </summary>
    [Required]
    [JsonPropertyName("transactionid")]
    public string TransactionId { get; set; } = default!;

    /// <summary>
    /// Shipping tracking number
    /// </summary>
    [JsonPropertyName("tracking_number")]
    public string TrackingNumber { get; set; } 

    /// <summary>
    ///  	Shipping carrier. Values: 'ups', 'fedex', 'dhl', or 'usps'
    /// </summary>
    [JsonPropertyName("shipping_carrier")]
    public string? shipping_carrier { get; set; } 
    
}