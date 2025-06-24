using FluentValidation;
using ThreeTP.Payment.Application.Commands.Terminals;

namespace ThreeTP.Payment.Application.Validators.Terminals
{
    public class UpdateTerminalCommandValidator : AbstractValidator<UpdateTerminalCommand>
    {
        public UpdateTerminalCommandValidator()
        {
            RuleFor(x => x.TerminalId)
                .NotEmpty().WithMessage("Terminal ID is required.");

            RuleFor(x => x.UpdateRequest).NotNull();

            RuleFor(x => x.UpdateRequest.TerminalUpdate.Name)
                .MaximumLength(100).WithMessage("Terminal name cannot exceed 100 characters.")
                .When(x => x.UpdateRequest.TerminalUpdate.Name != null); // Validate only if Name is provided
        }
    }
}
