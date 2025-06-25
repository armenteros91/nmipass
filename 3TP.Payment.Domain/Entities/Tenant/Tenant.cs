using System.Text.Json.Serialization;
using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Domain.Events.TenantEvent;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Domain.Entities.Tenant;

public class Tenant : BaseEntityWithEvents
{
    public Guid TenantId { get; set; }
    public string? CompanyName { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string ApiKey { get; set; } = string.Empty; // New ApiKey property

    // Changed from ICollection<Terminal> to Terminal?
    [JsonIgnore]
    public virtual Terminal? Terminal { get; set; } //todo: evitar anidacion infinita al serializar objetos relacionados 

    protected Tenant()
    {
    } // EF Core constructor


    public Tenant(string companyName, string companyCode)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new InvalidTenantException(nameof(CompanyName), "Company name is required");

        if (string.IsNullOrWhiteSpace(companyCode))
            throw new InvalidTenantException(nameof(CompanyCode), "Company code is required");

        TenantId = Guid.NewGuid();
        CompanyName = companyName.Trim();
        CompanyCode = companyCode.Trim().ToUpper();
        IsActive = true;
        // ApiKey will be set by the command handler after tenant creation.
    }

    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        AddDomainEvent(TenantActivatedEvent.Create(this));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(TenantDeactivatedEvent.Create(this));
        
    }
}