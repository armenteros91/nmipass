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


    //public IReadOnlyCollection<TenantApiKey> ApiKeys => _apiKeys.AsReadOnly();
    //MODIFIED: Changed from ICollection<TenantApiKey> to TenantApiKey?
    public TenantApiKey? ApiKey { get; set; }

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

    //MODIFIED: Updated to handle a single API key
    public void AddApiKey(TenantApiKey apiKey)
    {
        if (apiKey == null)
            throw new InvalidTenantException(nameof(apiKey), "API key cannot be null");

        // Overwrite existing key if one is present
        ApiKey = apiKey;
        AddDomainEvent(TenantApiKeyAddedEvent.Create(this, apiKey));
    }
}