namespace ThreeTP.Payment.Application.DTOs.Requests.Tenants;

public record CreateTenantRequest(string CompanyName, string CompanyCode);
public record UpdateTenantRequest(Guid TenantId, string CompanyName, string CompanyCode);
public record SetStatusRequest(bool IsActive);