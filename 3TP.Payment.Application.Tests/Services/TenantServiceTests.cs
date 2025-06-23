using Moq;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Services;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Events;
using ThreeTP.Payment.Domain.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ThreeTP.Payment.Application.Tests.Services
{
    // Assuming a new test class for these specific tests or adapt if TenantServiceTests exists
    public class TenantServiceAddApiKeyTests // Renamed to avoid conflict if TenantServiceTests exists
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITenantRepository> _mockTenantRepository;
        private readonly Mock<ILogger<TenantService>> _mockLogger;
        private readonly TenantService _tenantService;

        public TenantServiceAddApiKeyTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockLogger = new Mock<ILogger<TenantService>>();

            _mockUnitOfWork.Setup(uow => uow.TenantRepository).Returns(_mockTenantRepository.Object);

            _tenantService = new TenantService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task AddApiKeyAsync_Should_AddApiKeyAndCommit_WhenTenantExists()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant("Test Service Company", "TSC01");
            tenant.TenantId = tenantId;

            var apiKeyValue = "service_new_api_key";
            var description = "Service Test Key";
            var isActive = true;

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask); // Make sure CommitAsync is mockable on IUnitOfWork for service test

            // Act
            var resultApiKey = await _tenantService.AddApiKeyAsync(tenantId, apiKeyValue, description, isActive);

            // Assert
            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);

            Assert.NotNull(resultApiKey);
            Assert.Equal(apiKeyValue, resultApiKey.ApiKeyValue);
            Assert.Equal(description, resultApiKey.Description);
            Assert.Equal(isActive, resultApiKey.Status);
            Assert.Equal(tenantId, resultApiKey.TenantId);

            Assert.Contains(tenant.ApiKeys, k => k.ApiKeyValue == apiKeyValue);

            Assert.Contains(tenant.DomainEvents, ev => ev is TenantApiKeyAddedEvent);
            var apiKeyAddedEvent = tenant.DomainEvents.OfType<TenantApiKeyAddedEvent>().FirstOrDefault();
            Assert.NotNull(apiKeyAddedEvent);
            Assert.Equal(tenant, apiKeyAddedEvent.Tenant);
            Assert.Equal(resultApiKey, apiKeyAddedEvent.ApiKey);

            _mockTenantRepository.Verify(repo => repo.Update(tenant), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddApiKeyAsync_Should_ThrowTenantNotFoundException_WhenTenantDoesNotExist()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var apiKeyValue = "service_key_not_found";
            var description = "Service Test Key NF";
            var isActive = false;

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync((Tenant)null);

            // Act & Assert
            await Assert.ThrowsAsync<TenantNotFoundException>(() => _tenantService.AddApiKeyAsync(tenantId, apiKeyValue, description, isActive));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddApiKeyAsync_Should_RethrowInvalidTenantException_WhenTenantAddApiKeyThrows()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var existingApiKey = "service_existing_key";
            var tenant = new Tenant("Test Service Company", "TSC02");
            tenant.TenantId = tenantId;
            tenant.AddApiKey(new TenantApiKey(existingApiKey, tenantId));
            tenant.ClearDomainEvents(); // Assuming this helper exists on Tenant or BaseEntityWithEvents

            var description = "Service Duplicate Key";
            var isActive = true;

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidTenantException>(() => _tenantService.AddApiKeyAsync(tenantId, existingApiKey, description, isActive));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
