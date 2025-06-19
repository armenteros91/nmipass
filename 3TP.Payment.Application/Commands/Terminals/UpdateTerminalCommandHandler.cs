using MediatR;
using ThreeTP.Payment.Application.Interfaces; // For ITerminalService
using ThreeTP.Payment.Domain.Entities.Tenant; // For Terminal entity
using System.Threading;
using System.Threading.Tasks;
using AutoMapper; // For IMapper
using System; // For ArgumentNullException
// using ThreeTP.Payment.Application.DTOs.Requests.Terminals; // Implicitly used by UpdateTerminalCommand

// TerminalNotFoundException could be thrown by service or GetTerminalByIdAsync
// For example: using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Application.Commands.Terminals
{
    public class UpdateTerminalCommandHandler : IRequestHandler<UpdateTerminalCommand, bool>
    {
        private readonly ITerminalService _terminalService;
        private readonly IMapper _mapper;

        public UpdateTerminalCommandHandler(
            ITerminalService terminalService,
            IMapper mapper)
        {
            _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> Handle(UpdateTerminalCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.UpdateRequest == null) throw new ArgumentNullException(nameof(request.UpdateRequest), "UpdateRequest property cannot be null.");
            if (request.TerminalId == Guid.Empty) throw new ArgumentException("TerminalId must be a valid GUID.", nameof(request.TerminalId));

            var existingTerminal = await _terminalService.GetTerminalByIdAsync(request.TerminalId);

            if (existingTerminal == null)
            {
                // Aligning with the service's current behavior (UpdateTerminalAsync also returns false if not found)
                // Alternatively, could throw a TerminalNotFoundException here.
                return false;
            }

            // Apply updates from UpdateRequest DTO to the existing domain entity.
            // This requires an AutoMapper profile: CreateMap<UpdateTerminalRequestDto, Terminal>()
            // - It should only map Name if request.UpdateRequest.Name is not null.
            // - It should only map IsActive if request.UpdateRequest.IsActive.HasValue.
            // This can be configured in the profile using .ForAllMembers(opts => opts.Condition(...)) or specific .ForMember conditions.
            // If such a profile is not set up, manual mapping is safer here.
            // Manual mapping is more explicit about what gets updated:
            if (request.UpdateRequest.Name != null)
            {
                existingTerminal.Name = request.UpdateRequest.Name;
            }
            if (request.UpdateRequest.IsActive.HasValue)
            {
                existingTerminal.IsActive = request.UpdateRequest.IsActive.Value;
            }
            // Using _mapper.Map for now, assuming profile handles conditional mapping.
            // If not, the manual approach above should be used.
            // _mapper.Map(request.UpdateRequest, existingTerminal);
            // The above line _mapper.Map(dto, entity) would overwrite Name with null if Name in dto is null.
            // The manual mapping shown above is generally safer for partial updates unless the AutoMapper profile is specifically configured for this.
            // Let's stick to the manual mapping for safety as the profile was not explicitly defined for this partial update.

            // The call to _terminalService.UpdateTerminalAsync will use the modified existingTerminal.
            return await _terminalService.UpdateTerminalAsync(existingTerminal);
        }
    }
}
