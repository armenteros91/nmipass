using AutoMapper; // IMapper is injected, though not used if mapping is manual
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.Commands.Terminals;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.Interfaces; // For ITerminalService
using ThreeTP.Payment.Domain.Entities.Tenant; // For Terminal entity

namespace ThreeTP.Payment.Application.Tests.Commands.Terminals
{
    [TestFixture]
    public class UpdateTerminalCommandHandlerTests // Existing class, but content refactored
    {
        private Mock<ITerminalService> _mockTerminalService;
        private Mock<IMapper> _mockMapper; // Injected but might not be used if handler does manual mapping
        private UpdateTerminalCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockTerminalService = new Mock<ITerminalService>();
            _mockMapper = new Mock<IMapper>(); // Mock it even if not used by current impl of handler

            _handler = new UpdateTerminalCommandHandler(
                _mockTerminalService.Object,
                _mockMapper.Object // Pass the mock
            );
        }

        [Test]
        public async Task Handle_TerminalFound_ShouldUpdateManuallyAndCallService_ReturnsTrue()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var updateRequestDto = new UpdateTerminalRequestDto
            {
                Name = "Updated Name",
                IsActive = false
            };
            var command = new UpdateTerminalCommand(terminalId, updateRequestDto);

            var existingTerminal = new Terminal("Old Name", Guid.NewGuid(), "secret")
            {
                TerminalId = terminalId,
                IsActive = true
            };

            _mockTerminalService.Setup(s => s.GetTerminalByIdAsync(terminalId))
                                .ReturnsAsync(existingTerminal);
            _mockTerminalService.Setup(s => s.UpdateTerminalAsync(It.Is<Terminal>(t =>
                                    t.TerminalId == terminalId &&
                                    t.Name == updateRequestDto.Name && // Name is updated
                                    t.IsActive == updateRequestDto.IsActive))) // IsActive is updated
                                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _mockTerminalService.Verify(s => s.GetTerminalByIdAsync(terminalId), Times.Once);
            _mockTerminalService.Verify(s => s.UpdateTerminalAsync(It.Is<Terminal>(t =>
                t.Name == "Updated Name" &&
                t.IsActive == false &&
                t.TerminalId == terminalId)), Times.Once);
        }

        [Test]
        public async Task Handle_TerminalFound_PartialUpdateNullName_ShouldKeepOldName()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var updateRequestDto = new UpdateTerminalRequestDto // Name is null
            {
                IsActive = false // Only IsActive is provided
            };
            var command = new UpdateTerminalCommand(terminalId, updateRequestDto);
            var originalName = "Old Name";
            var existingTerminal = new Terminal(originalName, Guid.NewGuid(), "secret")
            {
                TerminalId = terminalId,
                IsActive = true
            };

            _mockTerminalService.Setup(s => s.GetTerminalByIdAsync(terminalId)).ReturnsAsync(existingTerminal);
            _mockTerminalService.Setup(s => s.UpdateTerminalAsync(It.IsAny<Terminal>())).ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockTerminalService.Verify(s => s.UpdateTerminalAsync(It.Is<Terminal>(t =>
                t.Name == originalName && // Name should not have changed
                t.IsActive == false // IsActive should be updated
                )), Times.Once);
        }

        [Test]
        public async Task Handle_TerminalFound_PartialUpdateNullIsActive_ShouldKeepOldIsActive()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var updateRequestDto = new UpdateTerminalRequestDto // IsActive is null
            {
                Name = "New Name Only" // Only Name is provided
            };
            var command = new UpdateTerminalCommand(terminalId, updateRequestDto);
            var originalIsActive = true;
            var existingTerminal = new Terminal("Old Name", Guid.NewGuid(), "secret")
            {
                TerminalId = terminalId,
                IsActive = originalIsActive
            };

            _mockTerminalService.Setup(s => s.GetTerminalByIdAsync(terminalId)).ReturnsAsync(existingTerminal);
            _mockTerminalService.Setup(s => s.UpdateTerminalAsync(It.IsAny<Terminal>())).ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockTerminalService.Verify(s => s.UpdateTerminalAsync(It.Is<Terminal>(t =>
                t.Name == "New Name Only" && // Name should be updated
                t.IsActive == originalIsActive // IsActive should not have changed
                )), Times.Once);
        }


        [Test]
        public async Task Handle_TerminalNotFound_ShouldReturnFalse()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var updateRequestDto = new UpdateTerminalRequestDto { Name = "Updated Name" };
            var command = new UpdateTerminalCommand(terminalId, updateRequestDto);

            _mockTerminalService.Setup(s => s.GetTerminalByIdAsync(terminalId))
                                .ReturnsAsync((Terminal)null); // Terminal not found

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockTerminalService.Verify(s => s.GetTerminalByIdAsync(terminalId), Times.Once);
            _mockTerminalService.Verify(s => s.UpdateTerminalAsync(It.IsAny<Terminal>()), Times.Never);
        }

        [Test]
        public async Task Handle_UpdateServiceCallReturnsFalse_ShouldReturnFalse()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var updateRequestDto = new UpdateTerminalRequestDto { Name = "Updated Name", IsActive = true };
            var command = new UpdateTerminalCommand(terminalId, updateRequestDto);
            var existingTerminal = new Terminal("Old Name", Guid.NewGuid(), "secret") { TerminalId = terminalId };

            _mockTerminalService.Setup(s => s.GetTerminalByIdAsync(terminalId)).ReturnsAsync(existingTerminal);
            _mockTerminalService.Setup(s => s.UpdateTerminalAsync(It.IsAny<Terminal>())).ReturnsAsync(false); // Service update fails

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Handle_NullCommand_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(null, CancellationToken.None);
            act.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'request')");
        }

        [Test]
        public void Handle_NullUpdateRequestInCommand_ShouldThrowArgumentNullException()
        {
            // Arrange
            var commandWithNullRequest = new UpdateTerminalCommand(Guid.NewGuid(), null);

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(commandWithNullRequest, CancellationToken.None);
            act.Should().ThrowAsync<ArgumentNullException>().WithMessage("UpdateRequest property cannot be null. (Parameter 'request.UpdateRequest')");
        }

        [Test]
        public void Handle_EmptyTerminalIdInCommand_ShouldThrowArgumentException()
        {
            // Arrange
            var updateRequestDto = new UpdateTerminalRequestDto { Name = "Updated Name", IsActive = false };
            var commandWithEmptyTerminalId = new UpdateTerminalCommand(Guid.Empty, updateRequestDto);

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(commandWithEmptyTerminalId, CancellationToken.None);
            act.Should().ThrowAsync<ArgumentException>().WithMessage("TerminalId must be a valid GUID. (Parameter 'request.TerminalId')");
        }
    }
}
