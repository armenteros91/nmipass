using Moq;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Commands.Tenants;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Events;
using ThreeTP.Payment.Domain.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ThreeTP.Payment.Application.Interfaces.Tenants;

namespace ThreeTP.Payment.Application.Tests.Commands.Tenants
{
    [TestFixture]
    public class AddApiKeyToTenantCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ITenantRepository> _mockTenantRepository;
        private Mock<ILogger<AddApiKeyToTenantCommandHandler>> _mockLogger;
        private AddApiKeyToTenantCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockLogger = new Mock<ILogger<AddApiKeyToTenantCommandHandler>>();

            _mockUnitOfWork.Setup(uow => uow.TenantRepository).Returns(_mockTenantRepository.Object);
            _handler = new AddApiKeyToTenantCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_Should_AddApiKeyAndCommit_WhenTenantExists()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant("Test Company", "TC001");
            tenant.TenantId = tenantId;

            var command = new AddApiKeyToTenantCommand(tenantId, "new_api_key_value", "Test Key", true);

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var resultApiKey = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);

            NUnit.Framework.Assert.IsNotNull(resultApiKey);
            NUnit.Framework.Assert.AreEqual(command.ApiKeyValue, resultApiKey.ApiKeyValue);
            NUnit.Framework.Assert.AreEqual(command.Description, resultApiKey.Description);
            NUnit.Framework.Assert.AreEqual(command.IsActive, resultApiKey.Status);
            NUnit.Framework.Assert.AreEqual(tenantId, resultApiKey.TenantId);

            NUnit.Framework.Assert.IsTrue(tenant.ApiKeys.Any(k => k.ApiKeyValue == command.ApiKeyValue));

            NUnit.Framework.Assert.IsTrue(tenant.DomainEvents.Any(ev => ev is TenantApiKeyAddedEvent));
            var apiKeyAddedEvent = tenant.DomainEvents.OfType<TenantApiKeyAddedEvent>().FirstOrDefault();
            NUnit.Framework.Assert.IsNotNull(apiKeyAddedEvent);
            NUnit.Framework.Assert.AreEqual(tenant, apiKeyAddedEvent.Tenant);
            NUnit.Framework.Assert.AreEqual(resultApiKey, apiKeyAddedEvent.ApiKey);

            _mockTenantRepository.Verify(repo => repo.Update(tenant), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Handle_Should_ThrowTenantNotFoundException_WhenTenantDoesNotExist()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new AddApiKeyToTenantCommand(tenantId, "new_api_key_value", "Test Key", true);

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync((Tenant)null);

            // Act & Assert
            NUnit.Framework.Assert.ThrowsAsync<TenantNotFoundException>(async () => await _handler.Handle(command, CancellationToken.None));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void Handle_Should_RethrowInvalidTenantException_WhenTenantAddApiKeyThrows()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var existingApiKey = "existing_key";
            var tenant = new Tenant("Test Company", "TC001");
            tenant.TenantId = tenantId;
            tenant.AddApiKey(new TenantApiKey(existingApiKey, tenantId));
            tenant.ClearDomainEvents();

            var command = new AddApiKeyToTenantCommand(tenantId, existingApiKey, "Duplicate Key", true);

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

            // Act & Assert
            NUnit.Framework.Assert.ThrowsAsync<InvalidTenantException>(async () => await _handler.Handle(command, CancellationToken.None));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
