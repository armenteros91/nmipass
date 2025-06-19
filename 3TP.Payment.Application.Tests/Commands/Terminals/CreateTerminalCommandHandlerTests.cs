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
using ThreeTP.Payment.Application.Interfaces; // For ITerminalService
using ThreeTP.Payment.Domain.Entities.Tenant; // For Terminal entity
using ThreeTP.Payment.Domain.Exceptions; // For TenantNotFoundException
using ThreeTP.Payment.Application.Common.Exceptions; // For CustomValidationException


namespace ThreeTP.Payment.Application.Tests.Commands.Terminals
{
    [TestFixture]
    public class CreateTerminalCommandHandlerTests // Existing class, but content refactored
    {
        private Mock<ITerminalService> _mockTerminalService;
        private Mock<IMapper> _mockMapper;
        private CreateTerminalCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockTerminalService = new Mock<ITerminalService>();
            _mockMapper = new Mock<IMapper>();

            _handler = new CreateTerminalCommandHandler(
                _mockTerminalService.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public async Task Handle_ValidRequest_ShouldCallServiceAndMapToResponseDto()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var createRequestDto = new CreateTerminalRequestDto
            {
                Name = "Test Terminal DTO",
                TenantId = tenantId,
                SecretKey = "dtoSecretKey"
            };
            var command = new CreateTerminalCommand(createRequestDto);

            var mappedTerminalEntity = new Terminal("Mapped Terminal", tenantId, "mappedSecretKey");
            var serviceResponseTerminalEntity = new Terminal("Service Response Terminal", tenantId, "serviceSecret")
            {
                TerminalId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            var expectedResponseDto = new TerminalResponseDto
            {
                TerminalId = serviceResponseTerminalEntity.TerminalId,
                Name = serviceResponseTerminalEntity.Name,
                TenantId = serviceResponseTerminalEntity.TenantId,
                IsActive = serviceResponseTerminalEntity.IsActive,
                CreatedDate = serviceResponseTerminalEntity.CreatedDate
            };

            _mockMapper.Setup(m => m.Map<Terminal>(createRequestDto))
                       .Returns(mappedTerminalEntity);
            _mockTerminalService.Setup(s => s.CreateTerminalAsync(mappedTerminalEntity))
                                .ReturnsAsync(serviceResponseTerminalEntity);
            _mockMapper.Setup(m => m.Map<TerminalResponseDto>(serviceResponseTerminalEntity))
                       .Returns(expectedResponseDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeSameAs(expectedResponseDto);
            _mockMapper.Verify(m => m.Map<Terminal>(createRequestDto), Times.Once);
            _mockTerminalService.Verify(s => s.CreateTerminalAsync(mappedTerminalEntity), Times.Once);
            _mockMapper.Verify(m => m.Map<TerminalResponseDto>(serviceResponseTerminalEntity), Times.Once);
        }

        [Test]
        public void Handle_ServiceThrowsTenantNotFoundException_ShouldPropagateException()
        {
            // Arrange
            var createRequestDto = new CreateTerminalRequestDto { Name = "Test", TenantId = Guid.NewGuid(), SecretKey = "key" };
            var command = new CreateTerminalCommand(createRequestDto);
            var mappedTerminalEntity = new Terminal(createRequestDto.Name, createRequestDto.TenantId, createRequestDto.SecretKey);

            _mockMapper.Setup(m => m.Map<Terminal>(createRequestDto)).Returns(mappedTerminalEntity);
            _mockTerminalService.Setup(s => s.CreateTerminalAsync(mappedTerminalEntity))
                                .ThrowsAsync(new TenantNotFoundException(createRequestDto.TenantId));

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
            act.Should().ThrowAsync<TenantNotFoundException>();
        }

        [Test]
        public void Handle_ServiceThrowsCustomValidationException_ShouldPropagateException()
        {
            // Arrange
            var createRequestDto = new CreateTerminalRequestDto { Name = "", TenantId = Guid.NewGuid(), SecretKey = "key" }; // Invalid name
            var command = new CreateTerminalCommand(createRequestDto);
            var mappedTerminalEntity = new Terminal(createRequestDto.Name, createRequestDto.TenantId, createRequestDto.SecretKey);

            _mockMapper.Setup(m => m.Map<Terminal>(createRequestDto)).Returns(mappedTerminalEntity);
            _mockTerminalService.Setup(s => s.CreateTerminalAsync(mappedTerminalEntity))
                                .ThrowsAsync(new CustomValidationException("Terminal name is required."));

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
            act.Should().ThrowAsync<CustomValidationException>();
        }

        [Test]
        public void Handle_NullCommand_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(null, CancellationToken.None);
            // Based on the implementation of CreateTerminalCommandHandler which has:
            // if (request == null) throw new ArgumentNullException(nameof(request));
            act.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'request')");
        }

        [Test]
        public void Handle_NullTerminalRequestInCommand_ShouldThrowArgumentNullException()
        {
            // Arrange
            var commandWithNullRequest = new CreateTerminalCommand(null);

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(commandWithNullRequest, CancellationToken.None);
            // Based on the implementation of CreateTerminalCommandHandler which has:
            // if (request.TerminalRequest == null) throw new ArgumentNullException(nameof(request.TerminalRequest), "TerminalRequest cannot be null.");
            act.Should().ThrowAsync<ArgumentNullException>().WithMessage("TerminalRequest cannot be null. (Parameter 'request.TerminalRequest')");
        }
    }
}
