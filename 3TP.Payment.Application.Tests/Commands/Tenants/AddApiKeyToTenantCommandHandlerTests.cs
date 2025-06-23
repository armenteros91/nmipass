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
using Xunit;

namespace ThreeTP.Payment.Application.Tests.Commands.Tenants
{
    public class AddApiKeyToTenantCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITenantRepository> _mockTenantRepository;
        private readonly Mock<ILogger<AddApiKeyToTenantCommandHandler>> _mockLogger;
        private readonly AddApiKeyToTenantCommandHandler _handler;

        public AddApiKeyToTenantCommandHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockLogger = new Mock<ILogger<AddApiKeyToTenantCommandHandler>>();

            // Setup IUnitOfWork to return the mock TenantRepository
            _mockUnitOfWork.Setup(uow => uow.TenantRepository).Returns(_mockTenantRepository.Object);

            _handler = new AddApiKeyToTenantCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_Should_AddApiKeyAndCommit_WhenTenantExists()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant("Test Company", "TC001"); // Using real Tenant to test AddApiKey behavior
            tenant.TenantId = tenantId; // Set TenantId as it's normally set by DB or constructor logic

            var command = new AddApiKeyToTenantCommand(tenantId, "new_api_key_value", "Test Key", true);

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var resultApiKey = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);

            Assert.NotNull(resultApiKey);
            Assert.Equal(command.ApiKeyValue, resultApiKey.ApiKeyValue);
            Assert.Equal(command.Description, resultApiKey.Description);
            Assert.Equal(command.IsActive, resultApiKey.Status);
            Assert.Equal(tenantId, resultApiKey.TenantId);

            // Check if the API key was added to the tenant's collection
            Assert.Contains(tenant.ApiKeys, k => k.ApiKeyValue == command.ApiKeyValue);

            // Check if the domain event was raised
            Assert.Contains(tenant.DomainEvents, ev => ev is TenantApiKeyAddedEvent);
            var apiKeyAddedEvent = tenant.DomainEvents.OfType<TenantApiKeyAddedEvent>().FirstOrDefault();
            Assert.NotNull(apiKeyAddedEvent);
            Assert.Equal(tenant, apiKeyAddedEvent.Tenant);
            Assert.Equal(resultApiKey, apiKeyAddedEvent.ApiKey);

            _mockTenantRepository.Verify(repo => repo.Update(tenant), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowTenantNotFoundException_WhenTenantDoesNotExist()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new AddApiKeyToTenantCommand(tenantId, "new_api_key_value", "Test Key", true);

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync((Tenant)null);

            // Act & Assert
            await Assert.ThrowsAsync<TenantNotFoundException>(() => _handler.Handle(command, CancellationToken.None));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_RethrowInvalidTenantException_WhenTenantAddApiKeyThrows()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            // Use a real tenant, add one key, then try to add the same one.
            var existingApiKey = "existing_key";
            var tenant = new Tenant("Test Company", "TC001");
            tenant.TenantId = tenantId;
            tenant.AddApiKey(new TenantApiKey(existingApiKey, tenantId));
            // Clear domain events from initial add if not desired in this specific test part
            tenant.ClearDomainEvents();


            var command = new AddApiKeyToTenantCommand(tenantId, existingApiKey, "Duplicate Key", true);

            _mockTenantRepository.Setup(repo => repo.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

            // Act & Assert
            // Tenant.AddApiKey will throw InvalidTenantException if the key already exists.
            await Assert.ThrowsAsync<InvalidTenantException>(() => _handler.Handle(command, CancellationToken.None));

            _mockTenantRepository.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            // Update and Commit should not be called if AddApiKey throws
            _mockTenantRepository.Verify(repo => repo.Update(It.IsAny<Tenant>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
