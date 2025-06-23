using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ThreeTP.Payment.Application.Common.Exceptions;
using ThreeTP.Payment.Application.Common.Responses;

namespace ThreeTP.Payment.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var response = new ValidationErrorResponse
            {
                Errors = failures.Select(f => new ValidationErrorItem
                {
                    Field = f.PropertyName,
                    Error = f.ErrorMessage,
                    ErrorCode = f.ErrorCode
                }).ToList()
            };

            throw new CustomValidationException(response);
        }

        return await next(cancellationToken);
    }
}