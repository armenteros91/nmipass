using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Services;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions; // For TenantNotFoundException
using ThreeTP.Payment.Application.Common.Exceptions; // For CustomValidationException

namespace ThreeTP.Payment.Application.Tests.Services
{
    [TestFixture]
    public class TerminalServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ITerminalRepository> _mockTerminalRepository;
        private Mock<ITenantRepository> _mockTenantRepository;
        private Mock<ILogger<TerminalService>> _mockLogger;
        private Mock<IEncryptionService> _mockEncryptionService;
        private TerminalService _terminalService;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTerminalRepository = new Mock<ITerminalRepository>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockLogger = new Mock<ILogger<TerminalService>>();
            _mockEncryptionService = new Mock<IEncryptionService>();

            _mockUnitOfWork.Setup(uow => uow.TerminalRepository).Returns(_mockTerminalRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.TenantRepository).Returns(_mockTenantRepository.Object);

            _terminalService = new TerminalService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockEncryptionService.Object
            );
        }

        // --- CreateTerminalAsync Tests ---
        [Test]
        public async Task CreateTerminalAsync_ValidTerminal_ShouldAddAndCommitAndReturnTerminal()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            // Assuming Terminal constructor: Terminal(string name, Guid tenantId, string plainSecretKey)
            // The plainSecretKey is temporarily stored in SecretKeyEncrypted by the constructor.
            var terminalToCreate = new Terminal("Test Terminal", tenantId, "plainSecretKey123");
            var mockTenant = new Tenant("Test Tenant", "contact@example.com", "SomeApiKey");


            _mockTenantRepository.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(mockTenant);
            _mockTerminalRepository.Setup(r => r.AddAsync(terminalToCreate)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _terminalService.CreateTerminalAsync(terminalToCreate);

            // Assert
            result.Should().BeSameAs(terminalToCreate);
            _mockTerminalRepository.Verify(r => r.AddAsync(terminalToCreate), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CreateTerminalAsync_TenantNotFound_ShouldThrowTenantNotFoundException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var terminalToCreate = new Terminal("Test Terminal", tenantId, "plainSecretKey123");

            _mockTenantRepository.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync((Tenant)null);

            // Act & Assert
            Func<Task> act = async () => await _terminalService.CreateTerminalAsync(terminalToCreate);
            act.Should().ThrowAsync<TenantNotFoundException>()
                .WithMessage($"Tenant with ID {terminalToCreate.TenantId} not found. Cannot create terminal.");
            _mockTerminalRepository.Verify(r => r.AddAsync(It.IsAny<Terminal>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void CreateTerminalAsync_NullTerminal_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Func<Task> act = async () => await _terminalService.CreateTerminalAsync(null);
            act.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'terminal')");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateTerminalAsync_InvalidName_ShouldThrowCustomValidationException(string invalidName)
        {
            var terminal = new Terminal(invalidName, Guid.NewGuid(), "secret"); // SecretKeyEncrypted holds plain key here
            Func<Task> act = async () => await _terminalService.CreateTerminalAsync(terminal);
            act.Should().ThrowAsync<CustomValidationException>().WithMessage("Terminal name is required.");
        }

        [Test]
        public void CreateTerminalAsync_EmptyTenantId_ShouldThrowCustomValidationException()
        {
            var terminal = new Terminal("Valid Name", Guid.Empty, "secret");
            Func<Task> act = async () => await _terminalService.CreateTerminalAsync(terminal);
            act.Should().ThrowAsync<CustomValidationException>().WithMessage("TenantId is required.");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateTerminalAsync_InvalidSecretKey_ShouldThrowCustomValidationException(string invalidSecret)
        {
            // The constructor Terminal(name, tenantId, secretKey) assigns secretKey to SecretKeyEncrypted.
            // The service validation checks this SecretKeyEncrypted field (expecting the plain key).
            var terminal = new Terminal("Valid Name", Guid.NewGuid(), invalidSecret);
            Func<Task> act = async () => await _terminalService.CreateTerminalAsync(terminal);
            act.Should().ThrowAsync<CustomValidationException>().WithMessage("SecretKey is required.");
        }


        // --- GetTerminalByIdAsync Tests ---
        [Test]
        public async Task GetTerminalByIdAsync_TerminalExists_ShouldReturnTerminal()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var expectedTerminal = new Terminal("Found Terminal", Guid.NewGuid(), "secret") { TerminalId = terminalId };
            _mockTerminalRepository.Setup(r => r.GetByIdAsync(terminalId)).ReturnsAsync(expectedTerminal);

            // Act
            var result = await _terminalService.GetTerminalByIdAsync(terminalId);

            // Assert
            result.Should().BeSameAs(expectedTerminal);
        }

        [Test]
        public async Task GetTerminalByIdAsync_TerminalDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            _mockTerminalRepository.Setup(r => r.GetByIdAsync(terminalId)).ReturnsAsync((Terminal)null);

            // Act
            var result = await _terminalService.GetTerminalByIdAsync(terminalId);

            // Assert
            result.Should().BeNull();
        }

        // --- UpdateTerminalAsync Tests ---
        [Test]
        public async Task UpdateTerminalAsync_TerminalExists_ShouldUpdateAndCommitAndReturnTrue()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var existingTerminal = new Terminal("Old Name", tenantId, "oldSecret") { TerminalId = terminalId, IsActive = true };

            // terminalUpdateData typically comes from a mapping of a DTO.
            // For this test, we simulate an update object. Name and IsActive are changed.
            // The secret key itself is not part of this update data for the UpdateTerminalAsync method's current design.
            var terminalUpdateData = new Terminal("New Name", tenantId, "oldSecret") { TerminalId = terminalId, IsActive = false };


            _mockTerminalRepository.Setup(r => r.GetByIdAsync(terminalId)).ReturnsAsync(existingTerminal);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _terminalService.UpdateTerminalAsync(terminalUpdateData);

            // Assert
            result.Should().BeTrue();
            existingTerminal.Name.Should().Be("New Name"); // Verify the original object was modified
            existingTerminal.IsActive.Should().BeFalse();
            _mockTerminalRepository.Verify(r => r.Update(existingTerminal), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateTerminalAsync_TerminalDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var terminalUpdateData = new Terminal("New Name", Guid.NewGuid(), "secret") { TerminalId = terminalId };
            _mockTerminalRepository.Setup(r => r.GetByIdAsync(terminalId)).ReturnsAsync((Terminal)null);

            // Act
            var result = await _terminalService.UpdateTerminalAsync(terminalUpdateData);

            // Assert
            result.Should().BeFalse();
            _mockTerminalRepository.Verify(r => r.Update(It.IsAny<Terminal>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateTerminalAsync_NullTerminalData_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Func<Task> act = async () => await _terminalService.UpdateTerminalAsync(null);
            act.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'terminalUpdateData')");
        }
    }
}
