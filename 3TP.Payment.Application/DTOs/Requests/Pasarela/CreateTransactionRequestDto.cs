using System.ComponentModel.DataAnnotations;

namespace ThreeTP.Payment.Application.DTOs.Requests.Pasarela
{
    public class CreateTransactionRequestDto
    {
        [Required(ErrorMessage = "TenantId is required")]
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "TransactionTypeId is required")]
        public Guid TypeTransactionsId { get; set; }
        public string PaymentTransactionsId { get; set; }
        public string responseCode  { get; set; }

        // Opcional: Si el TraceId se genera en el servidor, omite esta propiedad.
        public Guid? TraceId { get; set; }

        // Auditoría (opcional, puede ser asignado por el sistema)
        public string? CreatedBy { get; set; }

        // Campos adicionales comunes en transacciones de pago
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters (e.g., USD)")]
        public string Currency { get; set; } = "USD";

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        // Metadata adicional (opcional)
        public Dictionary<string, string>? Metadata { get; set; }
    }

}
