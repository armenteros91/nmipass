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
using ThreeTP.Payment.Application.Interfaces.Tenants; // Added for ITenantRepository and ITenantService

namespace ThreeTP.Payment.Application.Tests.Commands.Tenants
{
    [TestFixture]
    public class CreateTenantCommandHandlerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ITenantRepository> _tenantRepositoryMock;
        private Mock<ITenantService> _mockTenantService; // Added
        private Mock<ILogger<CreateTenantCommandHandler>> _loggerMock;
        private CreateTenantCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tenantRepositoryMock = new Mock<ITenantRepository>(); // Uses specific from using
            _mockTenantService = new Mock<ITenantService>(); // Added, uses specific from using
            _loggerMock = new Mock<ILogger<CreateTenantCommandHandler>>();

            _unitOfWorkMock.Setup(uow => uow.TenantRepository).Returns(_tenantRepositoryMock.Object);

            _handler = new CreateTenantCommandHandler(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _mockTenantService.Object); // Added ITenantService mock
        }

        [Test]
        public async Task Handle_ValidCommand_ShouldCreateTenantAddApiKeyAndRaiseEvent()
        {
            // Arrange
            var command = new CreateTenantCommand("Test Company", "TESTCODE");
            Tenant capturedTenant = null;

            _tenantRepositoryMock.Setup(repo => repo.CompanyCodeExistsAsync(command.CompanyCode))
                .ReturnsAsync(false);

            _tenantRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Tenant>()))
                .Callback<Tenant>(tenant => capturedTenant = tenant)
                .Returns(Task.CompletedTask);

            // Mock ITenantService.AddApiKeyAsync
            _mockTenantService.Setup(s => s.AddApiKeyAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((Guid tId, string val, string desc, bool stat) => new TenantApiKey(val, tId) { Description = desc, Status = stat });


            _unitOfWorkMock.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.CompanyName.Should().Be(command.CompanyName);
            result.CompanyCode.Should().Be(command.CompanyCode);

            capturedTenant.Should().NotBeNull("because AddAsync should have been called with a tenant");

            // Verify ITenantService.AddApiKeyAsync was called
            _mockTenantService.Verify(s => s.AddApiKeyAsync(
                capturedTenant.TenantId,
                It.IsAny<string>(), // API key value is generated, so It.IsAny
                "ApiKey for " + command.CompanyName,
                true), Times.Once);

            // Verify repository and unit of work calls
            _tenantRepositoryMock.Verify(repo => repo.CompanyCodeExistsAsync(command.CompanyCode), Times.Once);
            _tenantRepositoryMock.Verify(repo => repo.AddAsync(capturedTenant), Times.Once); // Verify with the captured tenant
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Check TenantActivatedEvent on the captured tenant instance
            NUnit.Framework.Assert.IsTrue(capturedTenant.DomainEvents.Any(ev => ev is TenantActivatedEvent), "TenantActivatedEvent should be present.");

            // TenantApiKeyAddedEvent is handled by TenantService, which is mocked.
            // We only verify that TenantService.AddApiKeyAsync was called.
        }
    }
}
