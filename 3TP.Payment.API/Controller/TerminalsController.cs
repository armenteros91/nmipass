using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Application.Interfaces; // ITerminalService
// using MediatR; // Alternative if directly using MediatR

namespace ThreeTP.Payment.API.Controller
{
    [Route("api/")]
    [ApiController]
    // Consider adding [Authorize] attribute if authentication is required
    public class TerminalsController : ControllerBase
    {
        private readonly ITerminalService _terminalService;
        // private readonly IMediator _mediator; // Alternative

        public TerminalsController(ITerminalService terminalService) // or IMediator mediator
        {
            _terminalService = terminalService;
            // _mediator = mediator;
        }

        /// <summary>
        /// Creates a new terminal for a specified tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for whom the terminal is being created.</param>
        /// <param name="createRequest">The terminal creation request data.</param>
        /// <returns>The created terminal's details.</returns>
        [HttpPost("tenants/{tenantId:guid}/terminal")] // Route changed to singular "terminal"
        [ProducesResponseType(typeof(TerminalResponseDto), 201)]
        [ProducesResponseType(400)] // Bad Request (validation errors)
        [ProducesResponseType(404)] // Tenant Not Found
        [ProducesResponseType(409)] // Conflict (Terminal already exists for tenant)
        public async Task<IActionResult> CreateTerminalForTenant(Guid tenantId, [FromBody] CreateTerminalRequestDto createRequest)
        {
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { message = "Tenant ID is required." });
            }

            // It's good practice to ensure the TenantId in the path matches the one in the body, if present.
            // Or, set it from the path to ensure consistency if the DTO also has TenantId.
            // For this example, assuming CreateTerminalRequestDto's TenantId will be used or set by the service/handler.
            // If CreateTerminalRequestDto.TenantId is meant to be ignored or validated against tenantId from path, adjust logic.
            // Let's assume the service layer will handle the TenantId from the DTO.

            // If your CreateTerminalRequestDto doesn't have TenantId, you'd pass it to the service method.
            // e.g., var terminal = await _terminalService.CreateTerminalAsync(tenantId, createRequest);
            // Since CreateTerminalRequestDto *does* have TenantId, we pass it as is.
            // Consider adding validation to ensure tenantId in path matches tenantId in DTO if both are present.

            if (createRequest.TenantId != tenantId)
            {
                 return BadRequest(new { message = "Tenant ID in path does not match Tenant ID in request body." });
            }

            try
            {
                var terminal = await _terminalService.CreateTerminalAsync(createRequest);
                // Assuming GetTerminalByIdAsync exists and is the correct route for "CreatedAtAction"
                return CreatedAtAction(nameof(GetTerminalById), new { terminalId = terminal.TerminalId }, terminal);
            }
            catch (Application.Common.Exceptions.CustomValidationException ex) // Or your specific validation exception
            {
                return BadRequest(ex.Errors);
            }
            catch (Domain.Exceptions.TenantNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // Catch if tenant already has a terminal
            {
                // This exception is thrown by CreateTerminalCommandHandler
                return Conflict(new { message = ex.Message });
            }
            // Add other specific exception handling as needed
        }

        /// <summary>
        /// Retrieves a specific terminal by its ID.
        /// </summary>
        /// <param name="terminalId">The ID of the terminal to retrieve.</param>
        /// <returns>The terminal details if found; otherwise, Not Found.</returns>
        [HttpGet("terminals/{terminalId:guid}")]
        [ProducesResponseType(typeof(TerminalResponseDto), 200)]
        [ProducesResponseType(404)] // Not Found
        public async Task<IActionResult> GetTerminalById(Guid terminalId)
        {
            if (terminalId == Guid.Empty)
            {
                return BadRequest(new { message = "Terminal ID is required." });
            }

            var terminal = await _terminalService.GetTerminalByIdAsync(terminalId);
            if (terminal == null)
            {
                return NotFound(new { message = $"Terminal with ID {terminalId} not found." });
            }
            return Ok(terminal);
        }

        /// <summary>
        /// Retrieves all terminals associated with a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant whose terminal is to be retrieved.</param>
        /// <returns>The terminal details if found for the specified tenant; otherwise, Not Found.</returns>
        [HttpGet("tenants/{tenantId:guid}/terminal")] // Route changed to singular "terminal"
        [ProducesResponseType(typeof(TerminalResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTerminalByTenantId(Guid tenantId) // Method name changed
        {
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { message = "Tenant ID is required." });
            }

            var terminal = await _terminalService.GetTerminalByTenantIdAsync(tenantId); // Use the new service method
            if (terminal == null)
            {
                // This could mean tenant not found, or tenant exists but has no terminal.
                // Depending on desired behavior, you might want to differentiate.
                // For now, returning 404 if no terminal is found for the tenant.
                return NotFound(new { message = $"Terminal not found for Tenant ID {tenantId}." });
            }
            return Ok(terminal);
        }

        /// <summary>
        /// Updates an existing terminal.
        /// </summary>
        /// <param name="terminalId">The ID of the terminal to update.</param>
        /// <param name="updateRequest">The terminal update request data.</param>
        /// <returns>No Content if successful; Not Found if terminal doesn't exist.</returns>
        [HttpPut("terminals/{terminalId:guid}")]
        [ProducesResponseType(204)] // No Content
        [ProducesResponseType(400)] // Bad Request (validation errors)
        [ProducesResponseType(404)] // Not Found
        public async Task<IActionResult> UpdateTerminal(Guid terminalId, [FromBody] UpdateTerminalRequestDto updateRequest)
        {
            if (terminalId == Guid.Empty)
            {
                return BadRequest(new { message = "Terminal ID is required." });
            }

            try
            {
                var success = await _terminalService.UpdateTerminalAsync(terminalId, updateRequest);
                if (!success)
                {
                    // This case might be hit if UpdateTerminalAsync returns false for "not found"
                    // instead of throwing an exception, which the current UpdateTerminalCommandHandler does.
                    // If the handler throws, this specific check might not be strictly needed here.
                    return NotFound(new { message = $"Terminal with ID {terminalId} not found or update failed." });
                }
                return NoContent();
            }
            catch (Application.Common.Exceptions.CustomValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            // Catch specific "NotFound" exception if your service/handler throws it
            // catch (TerminalNotFoundException ex)
            // {
            //     return NotFound(new { message = ex.Message });
            // }
        }
    }
}
