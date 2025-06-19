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
        private Mock<ITerminalRepository> _mockTerminalRepository;
        private Mock<ITenantRepository> _mockTenantRepository;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private CreateTerminalCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockTerminalRepository = new Mock<ITerminalRepository>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _handler = new CreateTerminalCommandHandler(
                _mockTerminalRepository.Object,
                _mockTenantRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object
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
            _mockTenantRepository.Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync(mockTenant);

            _mockTerminalRepository.Setup(r => r.AddAsync(It.IsAny<Terminal>()))
                .Returns(Task.CompletedTask); // Or .Callback<Terminal>(t => { /* set t.TerminalId if needed */ });


            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var expectedTerminalResponseDto = new TerminalResponseDto
            {
                TerminalId = Guid.NewGuid(),
                Name = createTerminalRequestDto.Name,
                TenantId = createTerminalRequestDto.TenantId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _mockMapper.Setup(m => m.Map<TerminalResponseDto>(It.IsAny<Terminal>()))
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
            // unless captured from the Terminal instance passed to _mockMapper.Setup.
            // The updated _mockMapper.Setup above helps make this more consistent.
            result.TerminalId.Should().NotBeEmpty();
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));


            _mockTerminalRepository.Verify(r => r.AddAsync(It.Is<Terminal>(t =>
                t.Name == createTerminalRequestDto.Name &&
                t.TenantId == createTerminalRequestDto.TenantId &&
                t.SecretKeyEncrypted == createTerminalRequestDto.SecretKey
            )), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

            _mockTenantRepository.Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync((Tenant)null); // Tenant not found

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            act.Should().ThrowAsync<TenantNotFoundException>()
                .WithMessage($"Tenant with ID {tenantId} not found.");

            _mockTerminalRepository.Verify(r => r.AddAsync(It.IsAny<Terminal>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
