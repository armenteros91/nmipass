using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Commands.Terminals;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions; // For TenantNotFoundException
using ThreeTP.Payment.Application.Commands.AwsSecrets; // For CreateSecretCommand
using Amazon.SecretsManager.Model; // For CreateSecretResponse
using Amazon.SecretsManager; // For AmazonSecretsManagerException

namespace ThreeTP.Payment.Application.Tests.Commands.Terminals
{
    [TestFixture]
    public class CreateTerminalCommandHandlerTests
    {
        private Mock<ITerminalRepository> _terminalRepositoryMock;
        private Mock<ITenantRepository> _tenantRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IAwsSecretManagerService> _mockAwsSecretManagerService; // New Mock
        private CreateTerminalCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _terminalRepositoryMock = new Mock<ITerminalRepository>();
            _tenantRepositoryMock = new Mock<ITenantRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _mockAwsSecretManagerService = new Mock<IAwsSecretManagerService>(); // Initialize new mock

            // Setup IUnitOfWork to return mocked repositories
            _unitOfWorkMock.Setup(uow => uow.TenantRepository).Returns(_tenantRepositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.TerminalRepository).Returns(_terminalRepositoryMock.Object);

            _handler = new CreateTerminalCommandHandler(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _mockAwsSecretManagerService.Object // Pass the new mock to the handler
            );
        }

        [Test]
        public async Task Handle_ValidRequest_ShouldCreateTerminalAndReturnResponseDto_Old()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var createTerminalRequestDto = new CreateTerminalRequestDto
            {
                Name = "Test Terminal",
                TenantId = tenantId,
                SecretKey = "supersecretkey123"
            };
            var command = new CreateTerminalCommand(createTerminalRequestDto);

            var mockTenant = new Tenant("Test Tenant", "contact@example.com");
            _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync(mockTenant);

            Terminal capturedTerminal = null;
            _terminalRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Terminal>()))
                 .Callback<Terminal>(t => capturedTerminal = t)
                .Returns(Task.CompletedTask);


            // Simulate successful secret creation for this old test case,
            // as the handler now depends on it.
            _mockAwsSecretManagerService
                .Setup(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSecretResponse { ARN = "arn:aws:secretsmanager:us-east-1:123456789012:secret:MySecret-123456" });

            _mapperMock.Setup(m => m.Map<TerminalResponseDto>(It.IsAny<Terminal>()))
                .Returns((Terminal src) => new TerminalResponseDto {
                    TerminalId = src.Id,
                    Name = src.Name,
                    TenantId = src.TenantId,
                    IsActive = true, // Assuming default
                    CreatedDate = DateTime.UtcNow // Assuming default
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createTerminalRequestDto.Name);
            result.TenantId.Should().Be(createTerminalRequestDto.TenantId);
            result.TerminalId.Should().NotBeEmpty();
            result.TerminalId.Should().Be(capturedTerminal.Id);


            _terminalRepositoryMock.Verify(r => r.AddAsync(It.Is<Terminal>(t =>
                t.Name == createTerminalRequestDto.Name &&
                t.TenantId == createTerminalRequestDto.TenantId &&
                t.SecretKey == createTerminalRequestDto.SecretKey // SecretKey is not encrypted in domain entity anymore
            )), Times.Once);
            // _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // This is no longer called directly
            _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify secret creation was called
        }

        [Test]
        public void Handle_TenantNotFound_ShouldThrowTenantNotFoundException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var createTerminalRequestDto = new CreateTerminalRequestDto
            {
                Name = "Test Terminal",
                TenantId = tenantId,
                SecretKey = "supersecretkey123"
            };
            var command = new CreateTerminalCommand(createTerminalRequestDto);

            _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync((Tenant)null); // Tenant not found

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            act.Should().ThrowAsync<TenantNotFoundException>()
                .WithMessage($"Tenant with ID {tenantId} not found.");

            _terminalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Terminal>()), Times.Never);
            // _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // This is no longer called
            _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Secret creation should not be called
        }

        [Test]
        public async Task Handle_ValidCommand_ShouldCallCreateSecretAsyncWithCorrectParametersAndReturnMappedTerminal()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var terminalName = "Test Terminal";
            var secretKey = "super-secret-key-123";
            var requestDto = new CreateTerminalRequestDto { TenantId = tenantId, Name = terminalName, SecretKey = secretKey };
            var command = new CreateTerminalCommand(requestDto);

            var tenant = new Tenant("Test Tenant", "tenant-alias");
            _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

            Terminal capturedTerminal = null;
            _mockTerminalRepository.Setup(r => r.AddAsync(It.IsAny<Terminal>()))
                .Callback<Terminal>(t => capturedTerminal = t) // Capture the terminal instance
                .Returns(Task.CompletedTask);

            _mockAwsSecretManagerService
                .Setup(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSecretResponse { ARN = "arn:aws:secretsmanager:us-east-1:123456789012:secret:MySecret-123456" });

            _mapperMock.Setup(m => m.Map<TerminalResponseDto>(It.IsAny<Terminal>()))
                .Returns((Terminal t) => new TerminalResponseDto { TerminalId = t.Id, Name = t.Name, TenantId = t.TenantId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(capturedTerminal, "Terminal should have been captured by AddAsync callback.");
            Assert.That(result.TerminalId, Is.EqualTo(capturedTerminal.Id));
            Assert.That(result.TenantId, Is.EqualTo(tenantId));
            Assert.That(result.Name, Is.EqualTo(terminalName));

            _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(
                It.Is<CreateSecretCommand>(c =>
                    c.Name == $"tenant/{tenantId}/terminal/{capturedTerminal.Id}/secretkey" &&
                    c.SecretString == secretKey &&
                    c.Description == $"Secret key for terminal {capturedTerminal.Id} of tenant {tenantId}" &&
                    c.TerminalId == capturedTerminal.Id
                ),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void Handle_WhenAwsSecretCreationFails_ShouldThrowException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var requestDto = new CreateTerminalRequestDto { TenantId = tenantId, Name = "Test Terminal", SecretKey = "secret" };
            var command = new CreateTerminalCommand(requestDto);
            var expectedException = new AmazonSecretsManagerException("AWS fake error");

            _mockTenantRepository.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(new Tenant("Test Tenant", "alias"));

            Terminal capturedTerminal = null;
            _mockTerminalRepository.Setup(r => r.AddAsync(It.IsAny<Terminal>()))
                .Callback<Terminal>(t => capturedTerminal = t)
                .Returns(Task.CompletedTask);

            _mockAwsSecretManagerService
                .Setup(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = Assert.ThrowsAsync<AmazonSecretsManagerException>(async () => await _handler.Handle(command, CancellationToken.None));
            Assert.That(actualException, Is.EqualTo(expectedException));

            _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(
                It.Is<CreateSecretCommand>(c => c.TerminalId == capturedTerminal.Id ), // Verify with captured terminal Id
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
