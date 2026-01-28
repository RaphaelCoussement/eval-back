using FluentValidation;
using MediatR;

namespace DungeonCrawler_Quests_Service.Application.Behavior;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Permet de valider une requête avant qu'elle ne soit traitée par le gestionnaire (handler). (Pattern Pipeline de MediatR)
    /// </summary>
    /// [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2016:ForwardCancellationTokenToInvocable", Justification = "MediatR RequestHandlerDelegate ne prend pas de token")]
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}