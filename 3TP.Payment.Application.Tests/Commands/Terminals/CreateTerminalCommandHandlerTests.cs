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
using ThreeTP.Payment.Domain.Exceptions;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using ThreeTP.Payment.Application.Interfaces.aws;
using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Application.Interfaces.Terminals;

namespace ThreeTP.Payment.Application.Tests.Commands.Terminals
{
    [TestFixture]
    public class CreateTerminalCommandHandlerTests
    {
        private Mock<ThreeTP.Payment.Application.Interfaces.Terminals.ITerminalRepository> _terminalRepositoryMock;
        private Mock<ThreeTP.Payment.Application.Interfaces.Tenants.ITenantRepository> _tenantRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IAwsSecretManagerService> _mockAwsSecretManagerService;
        private CreateTerminalCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _terminalRepositoryMock = new Mock<ThreeTP.Payment.Application.Interfaces.Terminals.ITerminalRepository>();
            _tenantRepositoryMock = new Mock<ThreeTP.Payment.Application.Interfaces.Tenants.ITenantRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _mockAwsSecretManagerService = new Mock<IAwsSecretManagerService>();

            _unitOfWorkMock.Setup(uow => uow.TenantRepository).Returns(_tenantRepositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.TerminalRepository).Returns(_terminalRepositoryMock.Object);

            _handler = new CreateTerminalCommandHandler(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _mockAwsSecretManagerService.Object
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

            _mockAwsSecretManagerService
                .Setup(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSecretResponse { ARN = "arn:aws:secretsmanager:us-east-1:123456789012:secret:MySecret-123456" });

            _mapperMock.Setup(m => m.Map<TerminalResponseDto>(It.IsAny<Terminal>()))
                .Returns((Terminal src) => new TerminalResponseDto {
                    TerminalId = src.TerminalId,
                    Name = src.Name,
                    TenantId = src.TenantId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createTerminalRequestDto.Name);
            result.TenantId.Should().Be(createTerminalRequestDto.TenantId);
            result.TerminalId.Should().NotBeEmpty();
            NUnit.Framework.Assert.IsNotNull(capturedTerminal);
            if (capturedTerminal != null)
            {
                result.TerminalId.Should().Be(capturedTerminal.TerminalId);
            }

            _terminalRepositoryMock.Verify(r => r.AddAsync(It.Is<Terminal>(t =>
                t.Name == createTerminalRequestDto.Name &&
                t.TenantId == createTerminalRequestDto.TenantId
            )), Times.Once);
            _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()), Times.Once);
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
                .ReturnsAsync((Tenant)null);

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            act.Should().ThrowAsync<TenantNotFoundException>()
                .WithMessage($"Tenant with ID {tenantId} not found.");

            _terminalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Terminal>()), Times.Never);
            _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()), Times.Never);
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
            _terminalRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Terminal>()))
                .Callback<Terminal>(t => capturedTerminal = t)
                .Returns(Task.CompletedTask);

            _mockAwsSecretManagerService
                .Setup(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSecretResponse { ARN = "arn:aws:secretsmanager:us-east-1:123456789012:secret:MySecret-123456" });

            _mapperMock.Setup(m => m.Map<TerminalResponseDto>(It.IsAny<Terminal>()))
                .Returns((Terminal t) => new TerminalResponseDto { TerminalId = t.TerminalId, Name = t.Name, TenantId = t.TenantId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            NUnit.Framework.Assert.IsNotNull(capturedTerminal, "Terminal should have been captured by AddAsync callback.");
            if (capturedTerminal != null)
            {
                NUnit.Framework.Assert.That(result.TerminalId, Is.EqualTo(capturedTerminal.TerminalId));
                NUnit.Framework.Assert.That(result.TenantId, Is.EqualTo(tenantId));
                NUnit.Framework.Assert.That(result.Name, Is.EqualTo(terminalName));

                _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(
                    It.Is<CreateSecretCommand>(c =>
                        c.Name == $"tenant/{tenantId}/terminal/{capturedTerminal.TerminalId}/secretkey" &&
                        c.SecretString == secretKey &&
                        c.Description == $"Secret key for terminal {capturedTerminal.TerminalId} of tenant {tenantId}" &&
                        c.TerminalId == capturedTerminal.TerminalId
                    ),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        [Test]
        public void Handle_WhenAwsSecretCreationFails_ShouldThrowException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var requestDto = new CreateTerminalRequestDto { TenantId = tenantId, Name = "Test Terminal", SecretKey = "secret" };
            var command = new CreateTerminalCommand(requestDto);
            var expectedException = new AmazonSecretsManagerException("AWS fake error");

            _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(new Tenant("Test Tenant", "alias"));

            Terminal capturedTerminal = null;
            _terminalRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Terminal>()))
                .Callback<Terminal>(t => capturedTerminal = t)
                .Returns(Task.CompletedTask);

            _mockAwsSecretManagerService
                .Setup(s => s.CreateSecretAsync(It.IsAny<CreateSecretCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = NUnit.Framework.Assert.ThrowsAsync<AmazonSecretsManagerException>(async () => await _handler.Handle(command, CancellationToken.None));
            NUnit.Framework.Assert.IsInstanceOf<AmazonSecretsManagerException>(actualException);
            NUnit.Framework.Assert.AreEqual(expectedException.Message, actualException.Message);

            NUnit.Framework.Assert.IsNotNull(capturedTerminal);
            if (capturedTerminal != null)
            {
                _mockAwsSecretManagerService.Verify(s => s.CreateSecretAsync(
                    It.Is<CreateSecretCommand>(c => c.TerminalId == capturedTerminal.TerminalId),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }
    }
}
