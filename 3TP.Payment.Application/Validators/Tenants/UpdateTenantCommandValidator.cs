using FluentValidation;
using ThreeTP.Payment.Application.Commands.Tenants;
using System;

namespace ThreeTP.Payment.Application.Validators.Tenants
{
    public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
    {
        public UpdateTenantCommandValidator()
        {
            RuleFor(x => x.TenantId)
                .NotEmpty().WithMessage("TenantId is required.")
                .NotEqual(Guid.Empty).WithMessage("TenantId must not be Guid.Empty.");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company Name is required.")
                .MaximumLength(100).WithMessage("Company Name must not exceed 100 characters.");

            RuleFor(x => x.CompanyCode)
                .NotEmpty().WithMessage("Company Code is required.")
                .MaximumLength(20).WithMessage("Company Code must not exceed 20 characters.")
                .Matches("^[a-zA-Z0-9]*$").WithMessage("Company Code must be alphanumeric."); // Example: Alphanumeric
        }
    }
}
