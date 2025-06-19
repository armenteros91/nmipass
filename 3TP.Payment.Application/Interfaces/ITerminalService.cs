using ThreeTP.Payment.Domain.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreeTP.Payment.Application.Interfaces
{
    public interface ITerminalService
    {
        Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId); // Stays, implementation will change
        Task<Terminal?> FindBySecretNameAsync(string plainSecretName); // Stays, implementation will change

        // Refactored methods to align with TenantService pattern
        Task<Terminal> CreateTerminalAsync(Terminal terminal); // Takes domain entity, returns domain entity
        Task<Terminal?> GetTerminalByIdAsync(Guid terminalId); // Returns domain entity or null
        Task<IEnumerable<Terminal>> GetTerminalsByTenantIdAsync(Guid tenantId); // Returns collection of domain entities
        Task<bool> UpdateTerminalAsync(Terminal terminal); // Takes domain entity, returns bool for success
        Task<bool> DeleteTerminalAsync(Guid terminalId); // Added for completeness, can be implemented later if needed
        Task<bool> SetActiveStatusAsync(Guid terminalId, bool isActive); // Similar to TenantService
    }
}
