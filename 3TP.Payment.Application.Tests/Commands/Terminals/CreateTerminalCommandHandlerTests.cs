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

namespace ThreeTP.Payment.Application.Tests.Commands.Terminals
{
    [TestFixture]
    public class CreateTerminalCommandHandlerTests
    {
        private Mock<ITerminalRepository> _terminalRepositoryMock;
        private Mock<ITenantRepository> _tenantRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private CreateTerminalCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _terminalRepositoryMock = new Mock<ITerminalRepository>();
            _tenantRepositoryMock = new Mock<ITenantRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            // Setup IUnitOfWork to return mocked repositories
            _unitOfWorkMock.Setup(uow => uow.TenantRepository).Returns(_tenantRepositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.TerminalRepository).Returns(_terminalRepositoryMock.Object);

            _handler = new CreateTerminalCommandHandler(
                _unitOfWorkMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task Handle_ValidRequest_ShouldCreateTerminalAndReturnResponseDto()
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

            var mockTenant = new Tenant("Test Tenant", "contact@example.com", "SomeApiKey"); // Assuming Tenant constructor
            _tenantRepositoryMock.Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync(mockTenant);

            _terminalRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Terminal>()))
                .Returns(Task.CompletedTask); // Or .Callback<Terminal>(t => { /* set t.TerminalId if needed */ });


            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var expectedTerminalResponseDto = new TerminalResponseDto
            {
                TerminalId = Guid.NewGuid(),
                Name = createTerminalRequestDto.Name,
                TenantId = createTerminalRequestDto.TenantId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _mapperMock.Setup(m => m.Map<TerminalResponseDto>(It.IsAny<Terminal>()))
                .Returns((Terminal src) => new TerminalResponseDto { // More robust mapping for test
                    TerminalId = src.TerminalId, // Use the actual TerminalId from the created Terminal
                    Name = src.Name,
                    TenantId = src.TenantId,
                    IsActive = src.IsActive,
                    CreatedDate = src.CreatedDate
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createTerminalRequestDto.Name);
            result.TenantId.Should().Be(createTerminalRequestDto.TenantId);
            // TerminalId and CreatedDate are generated, so exact match for those might be tricky
            // unless captured from the Terminal instance passed to _mapperMock.Setup.
            // The updated _mapperMock.Setup above helps make this more consistent.
            result.TerminalId.Should().NotBeEmpty();
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));


            _terminalRepositoryMock.Verify(r => r.AddAsync(It.Is<Terminal>(t =>
                t.Name == createTerminalRequestDto.Name &&
                t.TenantId == createTerminalRequestDto.TenantId &&
                t.SecretKeyEncrypted == createTerminalRequestDto.SecretKey
            )), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
