using ThreeTP.Payment.Domain.Commons;

namespace ThreeTP.Payment.Domain.Entities.Nmi;

public class NmiTransactionResponseLog :BaseEntity
{
    public Guid NmiTransactionResponseLogId { get; set; } 
    public Guid RequestId { get; set; }
    public string Status { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? TransactionId { get; set; }
    public string RawResponse { get; set; } = default!; //cadena de respueta de NMI , respuesta en bruto " Response=1&responsetext=SUCCESS&authcode=123456&transactionid=10805652794&avsresponse=&cvvresponse=N&orderid=&type=sale&response_code=100 "
   
    public NmiTransactionRequestLog Request { get; set; } = null!;
}