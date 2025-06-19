using ThreeTP.Payment.Domain.Commons;

namespace ThreeTP.Payment.Domain.Entities.Tenant
{
    public class Terminal : BaseEntity
    {
        public Guid TerminalId { get; set; }
        public string Name { get; set; }
        public string SecretKeyEncrypted { get; set; } // Encriptado con AES 
        public bool IsActive { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Hash desencriptado como índice, para optimizar el rendimiento de la consulta en terminals 
        /// </summary>
        public string SecretKeyHash { get; set; } = null!;

        // Constructor para EF Core
        protected Terminal()
        {
        }

        public Terminal(string name, Guid tenantId, string secretKey)
        {
            TerminalId = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TenantId = tenantId;
            CreatedDate = DateTime.UtcNow;
            SecretKeyEncrypted = secretKey;
        }

        public void UpdateSecretKey(string secretKey)
        {
            SecretKeyEncrypted = secretKey; // Será encriptado por el repositorio
        }
    }
}