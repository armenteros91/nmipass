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
// using ThreeTP.Payment.Domain.Events; // TenantApiKeyAddedEvent is removed
using ThreeTP.Payment.Domain.Events.TenantEvent; // For TenantActivatedEvent
using Microsoft.Extensions.Logging;

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

            // Constructor of CreateTenantCommandHandler was changed (ITenantService removed)
            _handler = new CreateTenantCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Handle_ValidCommand_ShouldCreateTenantSetApiKeyAndRaiseActivatedEvent()
        {
            // Arrange
            var command = new CreateTenantCommand("Test Company", "TESTCODE");
            Tenant capturedTenant = null;

            _tenantRepositoryMock.Setup(repo => repo.CompanyCodeExistsAsync(command.CompanyCode))
                .ReturnsAsync(false);

            _tenantRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Tenant>()))
                .Callback<Tenant>(tenant => capturedTenant = tenant)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1); // Assuming CommitAsync now returns int (number of affected rows) or Task<int>

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.CompanyName.Should().Be(command.CompanyName);
            result.CompanyCode.Should().Be(command.CompanyCode);
            result.ApiKey.Should().NotBeNullOrEmpty("because an API key should have been generated and set");

            capturedTenant.Should().NotBeNull("because AddAsync should have been called with a tenant");
            capturedTenant.ApiKey.Should().Be(result.ApiKey);
            capturedTenant.ApiKey.Length.Should().BeGreaterThan(10); // Basic check for generated key

            // Verify repository and unit of work calls
            _tenantRepositoryMock.Verify(repo => repo.CompanyCodeExistsAsync(command.CompanyCode), Times.Once);
            _tenantRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Tenant>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verify domain event for TenantActivatedEvent
            capturedTenant.DomainEvents.Should().ContainItemsAssignableTo<TenantActivatedEvent>();

            // TenantApiKeyAddedEvent is removed, so no check for it.
            // capturedTenant.DomainEvents.Should().NotContainItemsAssignableTo<TenantApiKeyAddedEvent>(); // Optional: explicitly check it's not there
        }

        [Test]
        public async Task Handle_CompanyCodeExists_ShouldThrowException()
        {
            // Arrange
            var command = new CreateTenantCommand("Test Company", "TESTCODE_EXISTS");

            _tenantRepositoryMock.Setup(repo => repo.CompanyCodeExistsAsync(command.CompanyCode))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage($"Company code {command.CompanyCode} already exists");

            _tenantRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Tenant>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
