using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces
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
    }
}
