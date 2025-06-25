using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Commands.Tenants;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ThreeTP.Payment.Application.Tests.Commands.Tenants
{
    [TestFixture]
    public class UpdateTenantApiKeyCommandHandlerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ITenantRepository> _tenantRepositoryMock;
        private Mock<ILogger<UpdateTenantApiKeyCommandHandler>> _loggerMock;
        private UpdateTenantApiKeyCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tenantRepositoryMock = new Mock<ITenantRepository>();
            _loggerMock = new Mock<ILogger<UpdateTenantApiKeyCommandHandler>>();

            _unitOfWorkMock.Setup(uow => uow.TenantRepository).Returns(_tenantRepositoryMock.Object);
            _handler = new UpdateTenantApiKeyCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Handle_ValidCommand_ShouldUpdateApiKeyAndCommit()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var oldApiKey = "old_api_key";
            var newApiKey = "new_api_key_1234567890";
            var command = new UpdateTenantApiKeyCommand(tenantId, newApiKey);

            var existingTenant = new Tenant("Test Company", "TEST") { TenantId = tenantId, ApiKey = oldApiKey };

            _tenantRepositoryMock.Setup(repo => repo.GetByIdAsync(tenantId))
                .ReturnsAsync(existingTenant);
            _unitOfWorkMock.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ApiKey.Should().Be(newApiKey);
            existingTenant.ApiKey.Should().Be(newApiKey); // Ensure the original object was modified

            _tenantRepositoryMock.Verify(repo => repo.GetByIdAsync(tenantId), Times.Once);
            _tenantRepositoryMock.Verify(repo => repo.Update(existingTenant), Times.Once); // Verify Update is called
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_TenantNotFound_ShouldThrowTenantNotFoundException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var newApiKey = "new_api_key_1234567890";
            var command = new UpdateTenantApiKeyCommand(tenantId, newApiKey);

            _tenantRepositoryMock.Setup(repo => repo.GetByIdAsync(tenantId))
                .ReturnsAsync((Tenant)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TenantNotFoundException>()
                .WithMessage($"Tenant with ID '{tenantId}' not found.");

            _unitOfWorkMock.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task Handle_InvalidNewApiKey_ShouldThrowArgumentException(string invalidApiKey)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var command = new UpdateTenantApiKeyCommand(tenantId, invalidApiKey);
            var existingTenant = new Tenant("Test Company", "TEST") { TenantId = tenantId, ApiKey = "old_key" };

            _tenantRepositoryMock.Setup(repo => repo.GetByIdAsync(tenantId))
                .ReturnsAsync(existingTenant);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("New API key cannot be null or whitespace. (Parameter 'NewApiKey')");

            _unitOfWorkMock.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
