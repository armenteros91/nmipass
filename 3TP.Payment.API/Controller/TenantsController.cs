using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.Commands.Tenants;
using ThreeTP.Payment.Application.DTOs.Requests.Tenants;
using ThreeTP.Payment.Application.Queries.Tenants;

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
    /// Creates a new tenant.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var tenant = await _mediator.Send(new CreateTenantCommand(request.CompanyName, request.CompanyCode));
        return CreatedAtAction(nameof(GetById), new { tenantId = tenant.TenantId },
            tenant); // Assuming tenant.NmiTransactionRequestLogId is the ID
    }

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTenantRequest request)
    {
        await _mediator.Send(new UpdateTenantCommand(request.TenantId, request.CompanyName, request.CompanyCode));
        return NoContent();
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