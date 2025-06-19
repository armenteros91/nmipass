using AutoMapper; // Added
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Application.Interfaces; // ITerminalService
using ThreeTP.Payment.Domain.Entities.Tenant; // For Terminal domain entity
using ThreeTP.Payment.Domain.Exceptions; // For TenantNotFoundException
using ThreeTP.Payment.Application.Common.Exceptions; // For CustomValidationException (if thrown by service)


namespace ThreeTP.Payment.API.Controller
{
    [Route("api/")]
    [ApiController]
    public class TerminalsController : ControllerBase
    {
        private readonly ITerminalService _terminalService;
        private readonly IMapper _mapper; // Added

        public TerminalsController(ITerminalService terminalService, IMapper mapper) // Added IMapper
        {
            _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Creates a new terminal for a specified tenant.
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/terminals")]
        [ProducesResponseType(typeof(TerminalResponseDto), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> CreateTerminalForTenant(Guid tenantId, [FromBody] CreateTerminalRequestDto createRequest)
        {
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new ProblemDetails { Title = "Tenant ID is required.", Status = 400 });
            }
            if (createRequest == null)
            {
                return BadRequest(new ProblemDetails { Title = "Request body is required.", Status = 400 });
            }
            if (createRequest.TenantId != tenantId)
            {
                 return BadRequest(new ProblemDetails { Title = "Tenant ID in path does not match Tenant ID in request body.", Status = 400 });
            }

            try
            {
                var terminalToCreate = _mapper.Map<Terminal>(createRequest);
                // terminalToCreate.TenantId is set by mapper from createRequest.TenantId
                // terminalToCreate.SecretKeyEncrypted will hold the plain key from createRequest.SecretKey (due to TerminalMappingProfile)
                // The service/repository will handle actual encryption.

                var createdTerminalEntity = await _terminalService.CreateTerminalAsync(terminalToCreate);

                var responseDto = _mapper.Map<TerminalResponseDto>(createdTerminalEntity);

                return CreatedAtAction(nameof(GetTerminalById), new { terminalId = responseDto.TerminalId }, responseDto);
            }
            catch (CustomValidationException ex)
            {
                return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message, Status = 400 });
            }
            catch (TenantNotFoundException ex)
            {
                return NotFound(new ProblemDetails { Title = "Tenant Not Found", Detail = ex.Message, Status = 404 });
            }
        }

        /// <summary>
        /// Retrieves a specific terminal by its ID.
        /// </summary>
        [HttpGet("terminals/{terminalId:guid}")]
        [ProducesResponseType(typeof(TerminalResponseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetTerminalById(Guid terminalId)
        {
            if (terminalId == Guid.Empty)
            {
                return BadRequest(new ProblemDetails { Title = "Terminal ID is required.", Status = 400 });
            }

            var terminalEntity = await _terminalService.GetTerminalByIdAsync(terminalId);
            if (terminalEntity == null)
            {
                return NotFound(new ProblemDetails { Title = "Terminal Not Found", Detail = $"Terminal with ID {terminalId} not found.", Status = 404 });
            }

            var responseDto = _mapper.Map<TerminalResponseDto>(terminalEntity);
            return Ok(responseDto);
        }

        /// <summary>
        /// Retrieves all terminals associated with a specific tenant.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/terminals")]
        [ProducesResponseType(typeof(IEnumerable<TerminalResponseDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails),400)]
        public async Task<IActionResult> GetTerminalsByTenantId(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new ProblemDetails { Title = "Tenant ID is required.", Status = 400 });
            }

            var terminalEntities = await _terminalService.GetTerminalsByTenantIdAsync(tenantId);
            var responseDtos = _mapper.Map<IEnumerable<TerminalResponseDto>>(terminalEntities);
            return Ok(responseDtos);
        }

        /// <summary>
        /// Updates an existing terminal.
        /// </summary>
        [HttpPut("terminals/{terminalId:guid}")]
        [ProducesResponseType(204)] // No Content
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> UpdateTerminal(Guid terminalId, [FromBody] UpdateTerminalRequestDto updateRequest)
        {
            if (terminalId == Guid.Empty)
            {
                return BadRequest(new ProblemDetails { Title = "Terminal ID is required.", Status = 400 });
            }
            if (updateRequest == null)
            {
                return BadRequest(new ProblemDetails { Title = "Request body is required.", Status = 400 });
            }

            try
            {
                var existingTerminalEntity = await _terminalService.GetTerminalByIdAsync(terminalId);
                if (existingTerminalEntity == null)
                {
                    return NotFound(new ProblemDetails { Title = "Terminal Not Found", Detail = $"Terminal with ID {terminalId} not found for update.", Status = 404 });
                }

                // Manual mapping for safety and clarity in partial updates
                if (updateRequest.Name != null)
                {
                    existingTerminalEntity.Name = updateRequest.Name;
                }
                if (updateRequest.IsActive.HasValue)
                {
                    existingTerminalEntity.IsActive = updateRequest.IsActive.Value;
                }
                // Note: Secret key updates are not handled by this endpoint.

                var success = await _terminalService.UpdateTerminalAsync(existingTerminalEntity);
                if (!success)
                {
                    return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = "An error occurred while updating the terminal.", Status = 400 });
                }
                return NoContent();
            }
            catch (CustomValidationException ex)
            {
                 return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message, Status = 400 });
            }
        }

        /// <summary>
        /// Sets the active status of a terminal.
        /// </summary>
        [HttpPatch("terminals/{terminalId:guid}/status")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> SetTerminalStatus(Guid terminalId, [FromBody] SetTerminalStatusRequestDto statusRequest)
        {
            if (terminalId == Guid.Empty)
            {
                return BadRequest(new ProblemDetails { Title = "Terminal ID is required.", Status = 400 });
            }
            if (statusRequest == null)
            {
                 return BadRequest(new ProblemDetails { Title = "Request body is required.", Status = 400 });
            }

            var success = await _terminalService.SetActiveStatusAsync(terminalId, statusRequest.IsActive);
            if (!success)
            {
                // This could be because the terminal was not found by the service.
                return NotFound(new ProblemDetails { Title = "Operation failed", Detail = "Could not set terminal status. Terminal may not exist or another error occurred.", Status = 404 });
            }
            return NoContent();
        }
    }
}
