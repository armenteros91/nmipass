using MediatR;
using ThreeTP.Payment.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Domain.Exceptions; // Assuming a TerminalNotFoundException or similar

namespace ThreeTP.Payment.Application.Commands.Terminals
{
    public class UpdateTerminalCommandHandler : IRequestHandler<UpdateTerminalCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTerminalCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateTerminalCommand request, CancellationToken cancellationToken)
        {
            var terminalRepository = _unitOfWork.TerminalRepository;
            var terminal = await terminalRepository.GetByIdAsync(request.TerminalId);

            if (terminal == null)
            {
                // Consider throwing a specific TerminalNotFoundException
                return false;
            }

            bool updated = false;
            if (request.UpdateRequest.Name != null)
            {
                terminal.Name = request.UpdateRequest.Name;
                updated = true;
            }

            if (request.UpdateRequest.IsActive.HasValue)
            {
                terminal.IsActive = request.UpdateRequest.IsActive.Value;
                updated = true;
            }

            // Note: SecretKey update is handled by UpdateSecretKeyAsync in repository if needed as a separate operation.
            // If you want to include it here, you'd call that method.
            // For now, this handler only updates Name and IsActive.

            if (updated)
            {
                terminalRepository.Update(terminal); // Assuming a generic Update method from IGenericRepository
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
