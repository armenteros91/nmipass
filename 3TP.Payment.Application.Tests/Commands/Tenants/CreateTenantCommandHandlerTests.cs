using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Commands.Tenants;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Events;
using Microsoft.Extensions.Logging; // Required for ILogger

namespace ThreeTP.Payment.Application.Tests.Commands.Tenants
{
    [TestFixture]
    public class CreateTenantCommandHandlerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ITenantRepository> _tenantRepositoryMock;
        private Mock<ILogger<CreateTenantCommandHandler>> _loggerMock;
        private CreateTenantCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tenantRepositoryMock = new Mock<ITenantRepository>();
            _loggerMock = new Mock<ILogger<CreateTenantCommandHandler>>();

            _unitOfWorkMock.Setup(uow => uow.TenantRepository).Returns(_tenantRepositoryMock.Object);
            // If there are other repositories needed by IUnitOfWork that CreateTenantCommandHandler indirectly uses,
            // they would need to be mocked here as well. For now, only TenantRepository seems direct.

            _handler = new CreateTenantCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Handle_ValidCommand_ShouldCreateTenantAddApiKeyAndRaiseEvent()
        {
            // Arrange
            var command = new CreateTenantCommand("Test Company", "TESTCODE");
            Tenant capturedTenant = null;

            _tenantRepositoryMock.Setup(repo => repo.CompanyCodeExistsAsync(command.CompanyCode))
                .ReturnsAsync(false);

            // Capture the tenant when AddAsync is called
            _tenantRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Tenant>()))
                .Callback<Tenant>(tenant => capturedTenant = tenant)
                .Returns(Task.CompletedTask);

            // UnitOfWork.CommitAsync setup
            _unitOfWorkMock.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // CommitAsync returns Task<bool>

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.CompanyName.Should().Be(command.CompanyName);
            result.CompanyCode.Should().Be(command.CompanyCode);

            capturedTenant.Should().NotBeNull("because AddAsync should have been called with a tenant");
            capturedTenant.ApiKeys.Should().NotBeNullOrEmpty("because an API key should have been added");
            capturedTenant.ApiKeys.First().ApiKeyValue.Should().NotBeNullOrEmpty();
            capturedTenant.ApiKeys.First().TenantId.Should().Be(capturedTenant.TenantId); // Or result.TenantId if Id is set post-creation

            // Verify repository and unit of work calls
            _tenantRepositoryMock.Verify(repo => repo.CompanyCodeExistsAsync(command.CompanyCode), Times.Once);
            _tenantRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Tenant>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verify domain event for TenantApiKeyAddedEvent
            // Assuming Tenant entity has a way to access its domain events, e.g., a public DomainEvents collection
            // And TenantApiKeyAddedEvent is a distinct type.
            // The original CreateTenantCommandHandler also adds TenantActivatedEvent.
            // So we check for both, but specifically that TenantApiKeyAddedEvent exists.

            // First, let's check TenantActivatedEvent as per original handler
            capturedTenant.DomainEvents.Should().ContainItemsAssignableTo<TenantActivatedEvent>();

            // Now, let's check for TenantApiKeyAddedEvent which is the core of this subtask's related feature
            // This part of the assertion might need adjustment based on how Tenant.AddApiKey() and event handling are implemented.
            // For example, if AddApiKey itself adds an event, or if the event is added in the handler.
            // Based on the previous subtask, tenant.AddApiKey() is called, and this method should be responsible
            // for creating and adding the TenantApiKeyAddedEvent.
            capturedTenant.DomainEvents.Should().ContainItemsAssignableTo<TenantApiKeyAddedEvent>("because adding an API key should raise this event");

            var apiKeyAddedEvent = capturedTenant.DomainEvents.OfType<TenantApiKeyAddedEvent>().FirstOrDefault();
            apiKeyAddedEvent.Should().NotBeNull();
            apiKeyAddedEvent.Tenant.TenantId.Should().Be(capturedTenant.TenantId); // Access TenantId via Tenant property
            apiKeyAddedEvent.ApiKey.ApiKeyValue.Should().Be(capturedTenant.ApiKeys.First().ApiKeyValue); // Compare ApiKeyValue property
        }
    }
}
