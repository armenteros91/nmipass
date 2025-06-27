using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace ThreeTP.Payment.Domain.Exceptions
{
    public class ValidationException : AppExceptionBase
    {
        public List<ValidationError> Errors { get; }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("Se produjeron uno o más errores de validación.")
        {
            Errors = failures
                .Select(failure => new ValidationError(failure.PropertyName, failure.ErrorMessage))
                .ToList();
        }

        public class ValidationError
        {
            public string Field { get; }
            public string Message { get; }

            public ValidationError(string field, string message)
            {
                Field = field;
                Message = message;
            }
        }
    }
}
