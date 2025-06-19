using FluentValidation;
using ThreeTP.Payment.Application.Commands.Terminals;

namespace ThreeTP.Payment.Application.Validators.Terminals
{
    public class CreateTerminalCommandValidator : AbstractValidator<CreateTerminalCommand>
    {
        public CreateTerminalCommandValidator()
        {
            RuleFor(x => x.TerminalRequest).NotNull();
            RuleFor(x => x.TerminalRequest.Name)
                .NotEmpty().WithMessage("Terminal name is required.")
                .MaximumLength(100).WithMessage("Terminal name cannot exceed 100 characters.");

            RuleFor(x => x.TerminalRequest.TenantId)
                .NotEmpty().WithMessage("Tenant ID is required.");

            RuleFor(x => x.TerminalRequest.SecretKey)
                .NotEmpty().WithMessage("Secret key is required.")
                .MinimumLength(16).WithMessage("Secret key must be at least 16 characters long.")
                .MaximumLength(128).WithMessage("Secret key cannot exceed 128 characters.");
        }
    }
}
