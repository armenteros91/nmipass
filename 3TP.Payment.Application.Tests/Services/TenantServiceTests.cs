using Moq;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Services;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Events.TenantEvent; // For TenantActivatedEvent
using ThreeTP.Payment.Domain.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit; // Using Xunit for assertions
using FluentAssertions; // Using FluentAssertions for more readable assertions

namespace ThreeTP.Payment.Application.Tests.Services
{
    public class TenantServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITenantRepository> _mockTenantRepository;
        private readonly Mock<ILogger<TenantService>> _mockLogger;
        private readonly TenantService _tenantService;

        public TenantServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockLogger = new Mock<ILogger<TenantService>>();

            _mockUnitOfWork.Setup(uow => uow.TenantRepository).Returns(_mockTenantRepository.Object);
            // If other repositories are used by TenantService, they should be mocked here too.
            // e.g., _mockUnitOfWork.Setup(uow => uow.SomeOtherRepository).Returns(new Mock<ISomeOtherRepository>().Object);

            _tenantService = new TenantService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        // Tests for AddApiKeyAsync are removed as the method itself is removed from ITenantService.

        #region UpdateTenantApiKeyAsync Tests

        [Fact]
        public async Task UpdateTenantApiKeyAsync_ValidTenantAndKey_ShouldUpdateApiKeyAndCommit()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var oldApiKey = "old_key_from_service_test";
            var newApiKey = "new_key_from_service_test";
            var tenant = new Tenant("UpdateKey Test Co", "UKTC") { TenantId = tenantId, ApiKey = oldApiKey };

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1)); // CommitAsync now returns Task<int>

            // Act
            var updatedTenant = await _tenantService.UpdateTenantApiKeyAsync(tenantId, newApiKey);

            // Assert
            updatedTenant.Should().NotBeNull();
            updatedTenant.ApiKey.Should().Be(newApiKey);
            tenant.ApiKey.Should().Be(newApiKey); // Ensure the instance passed to Update was modified

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Update(tenant), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTenantApiKeyAsync_TenantNotFound_ShouldThrowTenantNotFoundException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newApiKey = "new_key_for_nonexistent_tenant";
            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync((Tenant)null);

            // Act
            Func<Task> action = async () => await _tenantService.UpdateTenantApiKeyAsync(tenantId, newApiKey);

            // Assert
            await action.Should().ThrowAsync<TenantNotFoundException>();
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateTenantApiKeyAsync_InvalidNewApiKey_ShouldThrowArgumentException(string invalidApiKey)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant("UpdateKey Test Co", "UKTC") { TenantId = tenantId, ApiKey = "old_api_key" };
            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

            // Act
            Func<Task> action = async () => await _tenantService.UpdateTenantApiKeyAsync(tenantId, invalidApiKey);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>().WithMessage("New API key cannot be null or whitespace. (Parameter 'newApiKey')");
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ValidateByApiKeyAsync Tests

        [Fact]
        public async Task ValidateByApiKeyAsync_ExistingApiKey_ShouldReturnTenant()
        {
            // Arrange
            var apiKey = "valid_api_key_to_validate";
            var tenant = new Tenant("Validate Co", "VCO") { ApiKey = apiKey };
            _mockTenantRepository.Setup(repo => repo.GetByApiKeyAsync(apiKey)).ReturnsAsync(tenant);

            // Act
            var result = await _tenantService.ValidateByApiKeyAsync(apiKey);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(tenant);
            _mockTenantRepository.Verify(repo => repo.GetByApiKeyAsync(apiKey), Times.Once);
        }

        [Fact]
        public async Task ValidateByApiKeyAsync_NonExistingApiKey_ShouldReturnNull()
        {
            // Arrange
            var apiKey = "invalid_api_key_to_validate";
            _mockTenantRepository.Setup(repo => repo.GetByApiKeyAsync(apiKey)).ReturnsAsync((Tenant)null);

            // Act
            var result = await _tenantService.ValidateByApiKeyAsync(apiKey);

            // Assert
            result.Should().BeNull();
            _mockTenantRepository.Verify(repo => repo.GetByApiKeyAsync(apiKey), Times.Once);
        }
        #endregion

        #region CreateTenantAsync Tests
        [Fact]
        public async Task CreateTenantAsync_ValidTenant_ShouldAddAndCommit()
        {
            // Arrange
            var tenantToCreate = new Tenant("New Create Co", "NCC");
            // Note: ApiKey is set by CreateTenantCommandHandler, not directly in TenantService.CreateTenantAsync

            _mockTenantRepository.Setup(repo => repo.CompanyCodeExistsAsync(tenantToCreate.CompanyCode)).ReturnsAsync(false);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

            // Act
            await _tenantService.CreateTenantAsync(tenantToCreate);

            // Assert
            _mockTenantRepository.Verify(repo => repo.CompanyCodeExistsAsync(tenantToCreate.CompanyCode), Times.Once);
            _mockTenantRepository.Verify(repo => repo.AddAsync(tenantToCreate), Times.Once);
            tenantToCreate.DomainEvents.Should().ContainItemsAssignableTo<TenantActivatedEvent>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateTenantAsync_CompanyCodeExists_ShouldThrowCustomValidationException()
        {
            // Arrange
            var tenantToCreate = new Tenant("Existing Code Co", "EXCODE");
            _mockTenantRepository.Setup(repo => repo.CompanyCodeExistsAsync(tenantToCreate.CompanyCode)).ReturnsAsync(true);

            // Act
            Func<Task> action = async () => await _tenantService.CreateTenantAsync(tenantToCreate);

            // Assert
            await action.Should().ThrowAsync<CustomValidationException>()
                .Where(ex => ex.Errors.Any(e => e.Field == nameof(Tenant.CompanyCode) && e.Error == "Company code already exists"));

            _mockTenantRepository.Verify(repo => repo.AddAsync(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
