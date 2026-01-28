using FluentValidation;

namespace DungeonCrawler_Quests_Service.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex) // On attrape l'erreur lancée par ton ValidationBehavior
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            // On formate l'erreur exactement comme le front l'attend
            var response = new
            {
                message = "Une erreur de validation est survenue.",
                errors = ex.Errors.GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    )
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}