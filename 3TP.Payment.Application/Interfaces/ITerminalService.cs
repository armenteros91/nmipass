using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals; // Added
using ThreeTP.Payment.Application.DTOs.Responses.Terminals; // Added
using System; // Added
using System.Collections.Generic; // Added
using System.Threading.Tasks; // Added

namespace ThreeTP.Payment.Application.Interfaces
{
    public interface ITerminalService
    {
        Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId);
        Task<Terminal?> FindBySecretNameAsync(string plainSecretName);

        // New methods
        Task<TerminalResponseDto> CreateTerminalAsync(CreateTerminalRequestDto createRequest);
        Task<TerminalResponseDto?> GetTerminalByIdAsync(Guid terminalId);
        Task<TerminalResponseDto?> GetTerminalByTenantIdAsync(Guid tenantId); // Renamed and changed return type
        Task<bool> UpdateTerminalAsync(Guid terminalId, UpdateTerminalRequestDto updateRequest);
    }
}
