using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Queries.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Tests.Queries.Terminals
{
    [TestFixture]
    public class GetTerminalByIdQueryHandlerTests
    {
        private Mock<ThreeTP.Payment.Application.Interfaces.Terminals.ITerminalRepository> _terminalRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private GetTerminalByIdQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _terminalRepositoryMock = new Mock<ThreeTP.Payment.Application.Interfaces.Terminals.ITerminalRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            // Setup IUnitOfWork to return the mocked repository
            _unitOfWorkMock.Setup(uow => uow.TerminalRepository).Returns(_terminalRepositoryMock.Object);

            _handler = new GetTerminalByIdQueryHandler(
                _unitOfWorkMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task Handle_TerminalFound_ShouldReturnTerminalResponseDto()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var tenantId = Guid.NewGuid(); // Added for clarity in Terminal constructor
            var query = new GetTerminalByIdQuery(terminalId);

            // Assuming Terminal constructor is (name, tenantId, secretKey) and TerminalId can be set.
            var mockTerminal = new Terminal("Found Terminal", tenantId, "secretkey123")
            {
                TerminalId = terminalId, // Explicitly set TerminalId for the test
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _terminalRepositoryMock.Setup(r => r.GetByIdAsync(terminalId))
                .ReturnsAsync(mockTerminal);

            var expectedDto = new TerminalResponseDto
            {
                TerminalId = terminalId,
                Name = mockTerminal.Name,
                TenantId = mockTerminal.TenantId,
                IsActive = mockTerminal.IsActive,
                CreatedDate = mockTerminal.CreatedDate
            };
            _mapperMock.Setup(m => m.Map<TerminalResponseDto>(mockTerminal))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDto);
            _terminalRepositoryMock.Verify(r => r.GetByIdAsync(terminalId), Times.Once);
            _mapperMock.Verify(m => m.Map<TerminalResponseDto>(mockTerminal), Times.Once);
        }

        [Test]
        public async Task Handle_TerminalNotFound_ShouldReturnNull()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var query = new GetTerminalByIdQuery(terminalId);

            _terminalRepositoryMock.Setup(r => r.GetByIdAsync(terminalId))
                .ReturnsAsync((Terminal)null); // Terminal not found

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _terminalRepositoryMock.Verify(r => r.GetByIdAsync(terminalId), Times.Once);
            _mapperMock.Verify(m => m.Map<TerminalResponseDto>(It.IsAny<Terminal>()), Times.Never); // Mapper should not be called
        }
    }
}
