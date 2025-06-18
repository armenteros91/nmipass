using System.ComponentModel.DataAnnotations;

namespace ThreeTP.Payment.Application.DTOs.Requests.Pasarela
{
    public class UpdateTransactionRequestDto
    {
        [Required(ErrorMessage = "TransactionId is required")]
        public Guid TransactionId { get; set; }  // ID de la transacción a actualizar

        // Campos actualizables
        public Guid? TypeTransactionsId { get; set; }  // Opcional: Cambiar tipo de transacción

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }  // Nueva descripción

        // Auditoría (quién actualiza)
        [Required(ErrorMessage = "ModifiedBy is required for auditing")]
        public string ModifiedBy { get; set; }  // Ej: "user@email.com" o "system"

        // Metadata adicional (opcional)
        public Dictionary<string, string>? Metadata { get; set; }

        // Campos bloqueados (no actualizables vía DTO)
        // - TenantId, TraceId, CreatedDate, etc. no se incluyen para evitar modificaciones inseguras.
    }
}
