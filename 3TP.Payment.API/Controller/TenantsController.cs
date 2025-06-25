using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.Commands.Tenants;
using ThreeTP.Payment.Application.DTOs.Requests.Tenants;
using ThreeTP.Payment.Application.Queries.Tenants;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.API.Controller;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _mediator.Send(new GetAllTenantsQuery()));

    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetById(Guid tenantId) =>
        Ok(await _mediator.Send(new GetTenantByIdQuery(tenantId)));

    /// <summary>
    /// Creates a new tenant. An API key will be automatically generated.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var tenant = await _mediator.Send(new CreateTenantCommand(request.CompanyName, request.CompanyCode));
        return CreatedAtAction(nameof(GetById), new { tenantId = tenant.TenantId }, tenant);
    }

    /// <summary>
    /// Updates an existing tenant's company name and code. API key is not changed here.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTenantRequest request)
    {
        await _mediator.Send(new UpdateTenantCommand(request.TenantId, request.CompanyName, request.CompanyCode));
        return NoContent();
    }

    /// <summary>
    /// Updates the API key for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant to update.</param>
    /// <param name="request">Request containing the new API key.</param>
    /// <returns>The updated tenant information.</returns>
    [HttpPut("{tenantId:guid}/apikey")]
    public async Task<IActionResult> UpdateApiKey(Guid tenantId, [FromBody] UpdateTenantApiKeyRequest request)
    {
        if (tenantId != request.TenantId)
        {
            return BadRequest("Tenant ID in URL must match Tenant ID in request body.");
        }
        var tenant = await _mediator.Send(new UpdateTenantApiKeyCommand(request.TenantId, request.NewApiKey));
        return Ok(tenant);
    }

    /// <summary>
    /// Changes the active status of a tenant.
    /// </summary>
    [HttpPut("{tenantId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid tenantId, [FromBody] SetStatusRequest req)
    {
        await _mediator.Send(new SetTenantStatusCommand(tenantId, req.IsActive));
        return NoContent();
    }

    /// <summary>
    /// Checks if a company code already exists.
    /// </summary>
    [HttpGet("exists/{companyCode}")]
    public async Task<IActionResult> Exists(string companyCode)
    {
        var exists = await _mediator.Send(new TenantExistsQuery(companyCode));
        return Ok(new { exists });
    }

    /// <summary>
    /// Validates tenant by API key.
    /// </summary>
    [HttpGet("by-apikey/{apiKey}")]
    public async Task<IActionResult> GetByApiKey(string apiKey)
    {
        var tenant = await _mediator.Send(new GetTenantByApiKeyQuery(apiKey));
        return tenant is not null ? Ok(tenant) : NotFound();
    }
}