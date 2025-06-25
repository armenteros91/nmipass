using ThreeTP.Payment.Application.Common.Exceptions;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces.Tenants
{
    public interface ITenantService
    {
        /// <summary>
        /// Obtiene un tenant por su ID
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Tenant encontrado</returns>
        /// <exception cref="DomainException">Cuando el tenant no existe</exception>
        Task<Tenant> GetTenantByIdAsync(Guid tenantId);

        /// <summary>
        /// Obtiene todos los tenants disponibles
        /// </summary>
        /// <returns>Lista de tenants</returns>
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();

        /// <summary>
        /// Crea un nuevo tenant
        /// </summary>
        /// <param name="tenant">Datos del tenant a crear</param>
        /// <exception cref="CustomValidationException">Cuando los datos no son válidos</exception>
        Task CreateTenantAsync(Tenant tenant);

        /// <summary>
        /// Valida un tenant mediante su API Key
        /// </summary>
        /// <param name="apiKey">API Key a validar</param>
        /// <returns>Tenant si es válido, null si no</returns>
        Task<Tenant?> ValidateByApiKeyAsync(string apiKey);

        /// <summary>
        /// Verifica si un código de compañía ya existe
        /// </summary>
        /// <param name="companyCode">Código a verificar</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> CompanyCodeExistsAsync(string companyCode);

        /// <summary>
        /// Actualiza la información de un tenant
        /// </summary>
        /// <param name="tenant">Datos actualizados del tenant</param>
        Task UpdateTenantAsync(Tenant tenant);

        /// <summary>
        /// Cambia el estado activo/inactivo de un tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="isActive">Nuevo estado</param>
        Task SetActiveStatusAsync(Guid tenantId, bool isActive);

        // Task<TenantApiKey> AddApiKeyAsync(Guid tenantId, string apiKeyValue, string? description, bool isActive); // Removed

        /// <summary>
        /// Actualiza la API Key de un tenant existente.
        /// </summary>
        /// <param name="tenantId">ID del tenant a actualizar.</param>
        /// <param name="newApiKey">La nueva API Key para el tenant.</param>
        /// <returns>El tenant actualizado.</returns>
        /// <exception cref="TenantNotFoundException">Si el tenant con el ID especificado no se encuentra.</exception>
        /// <exception cref="ArgumentException">Si la nueva API key es inválida (null o vacía).</exception>
        Task<Tenant> UpdateTenantApiKeyAsync(Guid tenantId, string newApiKey); // Added for updating API key
    }
}