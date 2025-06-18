using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.DTOs.Requests.Tenants;
using ThreeTP.Payment.Application.Services;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.API.Controller;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly TenantService _tenantService;

    public TenantsController(TenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _tenantService.GetAllTenantsAsync());

    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetById(Guid tenantId) =>
        Ok(await _tenantService.GetTenantByIdAsync(tenantId));

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var tenant = new Tenant(request.CompanyName, request.CompanyCode);
        await _tenantService.CreateTenantAsync(tenant);
        return CreatedAtAction(nameof(GetById), new { tenantId = tenant.TenantId }, tenant);
    }

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTenantRequest request)
    {
        var tenant = new Tenant(request.CompanyName, request.CompanyCode)
        {
            TenantId = request.TenantId
        };
        await _tenantService.UpdateTenantAsync(tenant);
        return NoContent();
    }
    /// <summary>
    /// Changes the active status of a tenant.
    /// </summary>
    [HttpPut("{tenantId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid tenantId, [FromBody] SetStatusRequest req)
    {
        await _tenantService.SetActiveStatusAsync(tenantId, req.IsActive);
        return NoContent();
    }

    /// <summary>
    /// Checks if a company code already exists.
    /// </summary>
    [HttpGet("exists/{companyCode}")]
    public async Task<IActionResult> Exists(string companyCode)
    {
        var exists = await _tenantService.CompanyCodeExistsAsync(companyCode);
        return Ok(new { exists });
    }

    /// <summary>
    /// Validates tenant by API key.
    /// </summary>
    [HttpGet("by-apikey/{apiKey}")]
    public async Task<IActionResult> GetByApiKey(string apiKey)
    {
        var tenant = await _tenantService.ValidateByApiKeyAsync(apiKey);
        return tenant is not null ? Ok(tenant) : NotFound();
    }
}