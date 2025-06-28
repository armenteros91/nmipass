using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.Commands.Terminals;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Application.Queries.Terminals;

namespace ThreeTP.Payment.API.Controller;

[Authorize]
[Route("api/")]
[ApiController]
public class TerminalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TerminalsController> _logger;

    public TerminalsController(IMediator mediator, ILogger<TerminalsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new terminal.
    /// </summary>
    /// <param name="createRequest">The terminal creation request data.</param>
    /// <returns>The created terminal's details.</returns>
    [HttpPost("terminals")] // Changed route to "terminals" from "terminal" for consistency
    [ProducesResponseType(typeof(TerminalResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Tenant Not Found
    [ProducesResponseType(StatusCodes.Status409Conflict)] // Conflict (Terminal already exists for tenant)
    public async Task<IActionResult> CreateTerminal([FromBody] CreateTerminalRequestDto createRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var command = new CreateTerminalCommand(createRequest);
            var terminal = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTerminalById), new { terminalId = terminal.TerminalId }, terminal);
        }
        catch (Application.Common.Exceptions.CustomValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Domain.Exceptions.TenantNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) // Catch if tenant already has a terminal or other service layer validation
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating terminal.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the terminal.");
        }
    }

    /// <summary>
    /// Retrieves a specific terminal by its ID.
    /// </summary>
    /// <param name="terminalId">The ID of the terminal to retrieve.</param>
    /// <returns>The terminal details if found; otherwise, Not Found.</returns>
    [HttpGet("terminals/{terminalId:guid}")]
    [ProducesResponseType(typeof(TerminalResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTerminalById(Guid terminalId)
    {
        if (terminalId == Guid.Empty)
        {
            return BadRequest(new { message = "Terminal ID is required." });
        }

        try
        {
            var query = new GetTerminalByIdQuery(terminalId);
            var terminal = await _mediator.Send(query);
            return terminal != null ? Ok(terminal) : NotFound(new { message = $"Terminal with ID {terminalId} not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving terminal by ID {TerminalId}", terminalId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the terminal.");
        }
    }

    /// <summary>
    /// Retrieves the terminal associated with a specific tenant.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant whose terminal is to be retrieved.</param>
    /// <returns>The terminal details if found for the specified tenant; otherwise, Not Found.</returns>
    [HttpGet("tenants/{tenantId:guid}/terminal")]
    [ProducesResponseType(typeof(TerminalResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTerminalByTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            return BadRequest(new { message = "Tenant ID is required." });
        }
        try
        {
            var query = new GetTerminalByTenantIdQuery(tenantId);
            var terminal = await _mediator.Send(query);
            return terminal != null ? Ok(terminal) : NotFound(new { message = $"Terminal not found for Tenant ID {tenantId}." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving terminal by Tenant ID {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the terminal for the tenant.");
        }
    }

    /// <summary>
    /// Updates an existing terminal and optionally its associated AWS secret.
    /// </summary>
    /// <param name="terminalId">The ID of the terminal to update.</param>
    /// <param name="request">The terminal update request data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No Content if successful; Not Found if terminal doesn't exist; Bad Request for invalid data.</returns>
    [HttpPut("terminals/{terminalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTerminal(
        Guid terminalId,
        [FromBody] UpdateTerminalAndSecretRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // The UpdateTerminalCommand now encapsulates the logic previously in UpdateTerminalAndSecretAsync
            var command = new UpdateTerminalCommand(terminalId, request);
            var success = await _mediator.Send(command, cancellationToken);

            return success ? NoContent() : NotFound($"Terminal with ID {terminalId} not found or update failed.");
        }
        catch (Application.Common.Exceptions.CustomValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Domain.Exceptions.TenantNotFoundException ex) // Example if command handler throws this
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating terminal {TerminalId}", terminalId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the terminal.");
        }
    }

    /// <summary>
    /// Retrieves all terminals.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all terminals.</returns>
    [HttpGet("terminals/all")]
    [ProducesResponseType(typeof(List<TerminalResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTerminals(CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAllTerminalsQuery();
            var terminals = await _mediator.Send(query, cancellationToken);
            return Ok(terminals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all terminals");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving terminals.");
        }
    }
}