using System;
using System.ComponentModel.DataAnnotations;

namespace ThreeTP.Payment.Application.DTOs.Requests.Tenants
{
    public class UpdateTenantApiKeyRequest
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [MinLength(32)] // Example minimum length for an API key
        [MaxLength(255)]
        public string NewApiKey { get; set; } = string.Empty;
    }
}
