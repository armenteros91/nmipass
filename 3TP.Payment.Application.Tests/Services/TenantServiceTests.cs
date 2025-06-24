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
using NUnit.Framework;
using ThreeTP.Payment.Application.Interfaces.Tenants;

namespace ThreeTP.Payment.Application.Tests.Services
{
    [TestFixture]
    public class TenantServiceAddApiKeyTests // Original name was TenantServiceAddApiKeyTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ITenantRepository> _mockTenantRepository;
        private Mock<ILogger<TenantService>> _mockLogger;
        private TenantService _tenantService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockLogger = new Mock<ILogger<TenantService>>();

            _mockUnitOfWork.Setup(uow => uow.TenantRepository).Returns(_mockTenantRepository.Object);
            _tenantService = new TenantService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
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
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var resultApiKey = await _tenantService.AddApiKeyAsync(tenantId, apiKeyValue, description, isActive);

            // Assert
            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);

            NUnit.Framework.Assert.IsNotNull(resultApiKey);
            NUnit.Framework.Assert.AreEqual(apiKeyValue, resultApiKey.ApiKeyValue);
            NUnit.Framework.Assert.AreEqual(description, resultApiKey.Description);
            NUnit.Framework.Assert.AreEqual(isActive, resultApiKey.Status);
            NUnit.Framework.Assert.AreEqual(tenantId, resultApiKey.TenantId);

            NUnit.Framework.Assert.IsTrue(tenant.ApiKeys.Any(k => k.ApiKeyValue == apiKeyValue));

            NUnit.Framework.Assert.IsTrue(tenant.DomainEvents.Any(ev => ev is TenantApiKeyAddedEvent));
            var apiKeyAddedEvent = tenant.DomainEvents.OfType<TenantApiKeyAddedEvent>().FirstOrDefault();
            NUnit.Framework.Assert.IsNotNull(apiKeyAddedEvent);
            NUnit.Framework.Assert.AreEqual(tenant, apiKeyAddedEvent.Tenant);
            NUnit.Framework.Assert.AreEqual(resultApiKey, apiKeyAddedEvent.ApiKey);

            _mockTenantRepository.Verify(repo => repo.Addapikey(It.Is<TenantApiKey>(tak => tak.ApiKeyValue == apiKeyValue)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void AddApiKeyAsync_Should_ThrowTenantNotFoundException_WhenTenantDoesNotExist()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var apiKeyValue = "service_key_not_found";
            var description = "Service Test Key NF";
            var isActive = false;

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync((Tenant)null);

            // Act & Assert
            NUnit.Framework.Assert.ThrowsAsync<TenantNotFoundException>(async () => await _tenantService.AddApiKeyAsync(tenantId, apiKeyValue, description, isActive));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Addapikey(It.IsAny<TenantApiKey>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void AddApiKeyAsync_Should_RethrowInvalidTenantException_WhenTenantAddApiKeyThrows()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var existingApiKey = "service_existing_key";
            var tenant = new Tenant("Test Service Company", "TSC02");
            tenant.TenantId = tenantId;
            tenant.AddApiKey(new TenantApiKey(existingApiKey, tenantId));
            tenant.ClearDomainEvents();

            var description = "Service Duplicate Key";
            var isActive = true;

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

            // Act & Assert
            NUnit.Framework.Assert.ThrowsAsync<InvalidTenantException>(async () => await _tenantService.AddApiKeyAsync(tenantId, existingApiKey, description, isActive));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Addapikey(It.IsAny<TenantApiKey>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
