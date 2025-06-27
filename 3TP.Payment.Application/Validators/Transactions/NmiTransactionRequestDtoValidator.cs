using FluentValidation;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;

namespace ThreeTP.Payment.Application.Validators.Transactions
{
    public class NmiTransactionRequestDtoValidator : AbstractValidator<SaleTransactionRequestDto>
    {
        public NmiTransactionRequestDtoValidator()
        {
            // Siempre requeridos
            RuleFor(x => x.TypeTransaction)
                .NotEmpty().WithMessage("El tipo de transacción es requerido");

            RuleFor(x => x.SecurityKey)
                .NotEmpty().WithMessage("La clave de seguridad (SecurityKey) es requerida");

            RuleFor(x => x.Amount)
                .NotNull().WithMessage("El monto es obligatorio")
                .GreaterThan(0).WithMessage("El monto debe ser mayor que cero");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("La moneda es requerida");

            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("El orderId es requerido")
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("El orderId debe tener un formato GUID válido");

            // Requiere tipo de pago
            RuleFor(x => x.PaymentType)
                .NotEmpty().WithMessage("El tipo de pago es requerido");

            // Reglas específicas para pago con tarjeta
            When(x => x.PaymentType == "creditcard", () =>
            {
                RuleFor(x => x.CreditCardNumber)
                    .NotEmpty().WithMessage("El número de tarjeta (ccnumber) es requerido para pagos con tarjeta");

                RuleFor(x => x.CreditCardExpiration)
                    .NotEmpty().WithMessage("La fecha de expiración (ccexp) es requerida para pagos con tarjeta")
                    .Matches("^(0[1-9]|1[0-2])[0-9]{2}$")
                    .WithMessage("La fecha de expiración debe estar en formato MMYY.");
            });

            // Reglas específicas para pago ACH
            When(x => x.PaymentType == "check", () =>
            {
                RuleFor(x => x.CheckName)
                    .NotEmpty().WithMessage("El nombre del titular (checkname) es requerido para pagos ACH");

                RuleFor(x => x.CheckAba)
                    .NotEmpty().WithMessage("El número ABA (checkaba) es requerido para pagos ACH");

                RuleFor(x => x.CheckAccount)
                    .NotEmpty().WithMessage("El número de cuenta (checkaccount) es requerido para pagos ACH");
            });

            // Requiere campos para 3DS si se especifica cardholder_auth
            When(x => x.CardHolderAuth == "verified" || x.CardHolderAuth == "attempted", () =>
            {
                RuleFor(x => x.Cavv)
                    .NotEmpty().WithMessage("El valor de autenticación (CAVV) es requerido para transacciones 3DS");

                RuleFor(x => x.Xid)
                    .NotEmpty().WithMessage("El ID de autenticación (XID) es requerido para transacciones 3DS");

                RuleFor(x => x.ThreeDsVersion)
                    .NotEmpty().WithMessage("La versión de 3DS es requerida");

                RuleFor(x => x.DirectoryServerId)
                    .NotEmpty().WithMessage("El ID del servidor de directorio es requerido para 3DS");
            });

            // Requiere 32 caracteres alfanuméricos si se usa Kount
            When(x => !string.IsNullOrEmpty(x.TransactionSessionId), () =>
            {
                RuleFor(x => x.TransactionSessionId)
                    .Length(32)
                    .WithMessage("TransactionSessionId debe tener exactamente 32 caracteres")
                    .Matches("^[a-zA-Z0-9]{32}$")
                    .WithMessage("TransactionSessionId debe ser alfanumérico sin símbolos");
            });

            // Validaciones adicionales recomendadas
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("El nombre del titular es requerido");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("El apellido del titular es requerido");
            RuleFor(x => x.Address1).NotEmpty().WithMessage("La dirección es requerida");
            RuleFor(x => x.City).NotEmpty().WithMessage("La ciudad es requerida");
            RuleFor(x => x.State).NotEmpty().WithMessage("El estado es requerido");
            RuleFor(x => x.Zip).NotEmpty().WithMessage("El código postal es requerido");
            RuleFor(x => x.Country).NotEmpty().WithMessage("El país es requerido");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico es requerido")
                .EmailAddress().WithMessage("Debe proporcionar un correo electrónico válido");
        }
    }
}
