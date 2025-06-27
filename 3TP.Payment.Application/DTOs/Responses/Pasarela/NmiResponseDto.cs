using System.Text.Json.Serialization;

namespace ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
/// <summary>
/// modelo de respuesta Tipado de NMI
/// example respnse NMI 
///  ' Response=1&responsetext=SUCCESS&authcode=123456&transactionid=10805652794&avsresponse=&cvvresponse=N&orderid=&type=sale&response_code=100 '  //todo documentacion de respuesta NMI 
/// </summary>
public class NmiResponseDto
{
    /// <summary>
    ///  1 = Transactions Approved
    ///  2 = Transactions Declined
    ///  3 = Error in transaction data or system error 
    /// </summary>
    [JsonPropertyName("response")]
    public string? Response { get; set; }

    /// <summary>
    /// Textual Response
    /// </summary>
    [JsonPropertyName("responsetext")]
    public string? ResponseText { get; set; }

    /// <summary>
    /// Transactions authorization code.
    /// </summary>
    [JsonPropertyName("authcode")]
    public string? AuthCode { get; set; }

    /// <summary>
    /// Payment gateway transaction id. 
    /// </summary>
    [JsonPropertyName("transactionid")]
    public string? TransactionId { get; set; }

    /// <summary>
    /// AVS Response code (See AVS Response Codes). 
    /// </summary>
    [JsonPropertyName("avsresponse")]
    public string? AvsResponse { get; set; }

    /// <summary>
    ///  CVV Response code (See See CVV Response Codes).
    /// </summary>
    [JsonPropertyName("cvvresponse")]
    public string? CvvResponse { get; set; }

    /// <summary>
    ///  The original order id passed in the transaction request.
    /// </summary>
    [JsonPropertyName("orderid")]
    public string? OrderId { get; set; }

    /// <summary>
    /// Numeric mapping of processor responses (See See Result Code Table).
    /// </summary>
    [JsonPropertyName("response_code")]
    public string? ResponseCode { get; set; }


    /// <summary>
    /// This will optionally come back when any chip card data is provided on the authorization. This data needs to be sent back to the SDK after an authorization.
    /// </summary>
    [JsonPropertyName("emv_auth_response_data")]
    public string? EmvAuthResponseData { get; set; }

    // Conditional

    /// <summary>
    ///  The original customer_vault_id passed in the transaction request or the resulting customer_vault_id created on an approved transaction.
    ///  Note: Only returned when the "Customer Vault" service is active. 
    /// </summary>
    [JsonPropertyName("customer_vault_id")]
    public string? CustomerVaultId { get; set; }

    /// <summary>
    /// The Kount "Omniscore" indicating the level of risk on a given transaction. The higher the score, the lower the risk.
    /// Note: Only returned when the "Kount" service is active. 
    /// </summary>
    [JsonPropertyName("kount_score")]
    public string? KountScore { get; set; }

    /// <summary>
    ///	Mastercard’s Merchant Advice Code (MAC) is returned in Response if one is provided by the processor.
    /// Note: Only returned if API configuration is set to return this value. 
    /// </summary>
    [JsonPropertyName("merchant_advice_code")]
    public string? MerchantAdviceCode { get; set; }
}