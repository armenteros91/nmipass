using System.Runtime.Serialization;

namespace ThreeTP.Payment.Domain.Exceptions
{
    [Serializable]
    public class TenantNotFoundException : AppExceptionBase
    {
        public Guid TenantId { get; }

        public TenantNotFoundException(Guid tenantId)
            : base($"Tenant with ID {tenantId} not found.")
        {
            TenantId = tenantId;
        }

        public TenantNotFoundException(Guid tenantId, string message)
            : base(message ?? $"Tenant with ID {tenantId} not found.")
        {
            TenantId = tenantId;
        }

        public TenantNotFoundException(Guid tenantId, string message, Exception? innerException)
            : base(message ?? $"Tenant with ID {tenantId} not found.", innerException)
        {
            TenantId = tenantId;
        }

        protected TenantNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            TenantId = (Guid)(info.GetValue(nameof(TenantId), typeof(Guid)) ?? Guid.Empty);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);
            info.AddValue(nameof(TenantId), TenantId);
            base.GetObjectData(info, context);
        }
    }
}