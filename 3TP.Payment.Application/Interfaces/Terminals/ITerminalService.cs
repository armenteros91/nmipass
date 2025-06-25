using ThreeTP.Payment.Application.Commands.Terminals;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces.Terminals
{
    public interface ITerminalService
    {
        Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId);
        Task<Terminal?> FindBySecretNameAsync(string plainSecretName);
        Task<TerminalResponseDto> CreateTerminalAsync(CreateTerminalRequestDto createRequest);
        Task<TerminalResponseDto?> GetTerminalByIdAsync(Guid terminalId);
        Task<TerminalResponseDto?> GetTerminalByTenantIdAsync(Guid tenantId);
        Task<bool> UpdateTerminalAsync(Guid terminalId, UpdateTerminalAndSecretRequest updateRequest);

        Task<bool> UpdateTerminalAndSecretAsync(Guid terminalId, UpdateTerminalAndSecretRequest updateRequest,
            CancellationToken cancellationToken);

        Task<List<TerminalResponseDto>> GetAllTerminalsAsync();
    }
}