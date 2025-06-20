using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Common.Exceptions;
using ThreeTP.Payment.Application.Common.Responses;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Events;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Application.Services;

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        IUnitOfWork unitOfWork,
        ILogger<TenantService> logger
    )
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Tenant> GetTenantByIdAsync(Guid tenantId)
    {
        _logger.LogInformation("Fetching tenant by id {TenantId}", tenantId);

        var tenant = await _unitOfWork.TenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant with id {TenantId} not found", tenantId);
            throw new TenantNotFoundException(tenantId);
        }

        return tenant;
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
    {
        _logger.LogInformation("Fetching all tenants");
        return await _unitOfWork.TenantRepository.GetAllAsync();
    }

    public async Task CreateTenantAsync(Tenant tenant)
    {
        ValidateTenant(tenant);

        if (await _unitOfWork.TenantRepository.CompanyCodeExistsAsync(tenant.CompanyCode))
        {
            throw new CustomValidationException(
                new ValidationErrorResponse
                {
                    Errors = new List<ValidationErrorItem>
                    {
                        new() { Field = nameof(tenant.CompanyCode), Error = "Company code already exists" }
                    }
                });
        }

        _logger.LogInformation("Creating tenant with id {Tenantname}", tenant.CompanyName);
        try
        {
            await _unitOfWork.TenantRepository.AddAsync(tenant);
            tenant.AddDomainEvent(TenantActivatedEvent.Create(tenant));

            await _unitOfWork.CommitAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating tenant");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Tenant?> ValidateByApiKeyAsync(string apiKey)
    {
        _logger.LogInformation("Validating tenant by API key");
        return await _unitOfWork.TenantRepository.GetByApiKeyAsync(apiKey);
    }

    public async Task<bool> CompanyCodeExistsAsync(string companyCode)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
            return false;

        return await _unitOfWork.TenantRepository.CompanyCodeExistsAsync(companyCode);
    }

    public async Task UpdateTenantAsync(Tenant tenant)
    {
        ValidateTenant(tenant);
        _logger.LogInformation("Updating tenant {TenantId}", tenant.TenantId);

        try
        {
            var existingTenant = await GetTenantByIdAsync(tenant.TenantId);

            if (existingTenant.CompanyCode != tenant.CompanyCode &&
                await CompanyCodeExistsAsync(tenant.CompanyCode))
            {
                throw new CustomValidationException(
                    new ValidationErrorResponse
                    {
                        Errors = new List<ValidationErrorItem>
                        {
                            new() { Field = nameof(tenant.CompanyCode), Error = "Company code already exists" }
                        }
                    });
            }

            _unitOfWork.TenantRepository.Update(tenant);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task SetActiveStatusAsync(Guid tenantId, bool isActive)
    {
        _logger.LogInformation("Setting active status {Status} for tenant {TenantId}", isActive, tenantId);

        try
        {
            var tenant = await GetTenantByIdAsync(tenantId);

            if (isActive)
                tenant.Activate();
            else
                tenant.Deactivate();

            _unitOfWork.TenantRepository.Update(tenant);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing tenant status");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private void ValidateTenant(Tenant tenant)
    {
        var errors = new List<ValidationErrorItem>();

        if (string.IsNullOrWhiteSpace(tenant.CompanyName))
        {
            errors.Add(new ValidationErrorItem
            {
                Field = nameof(tenant.CompanyName),
                Error = "Company name is required"
            });
        }

        if (string.IsNullOrWhiteSpace(tenant.CompanyCode))
        {
            errors.Add(new ValidationErrorItem
            {
                Field = nameof(tenant.CompanyCode),
                Error = "Company code is required"
            });
        }

        if (errors.Any())
        {
            _logger.LogWarning("Validation failed for tenant: {Errors}", errors);
            throw new CustomValidationException(new ValidationErrorResponse { Errors = errors });
        }
    }
}