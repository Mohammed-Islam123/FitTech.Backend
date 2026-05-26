using ErrorOr;

namespace Activity.Shared;

public static class ErrorOnExtensions
{
    public static IResult MapErrorsToResult(List<Error> errors)
    {
        var firstError = errors[0];
        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(statusCode: statusCode, title: firstError.Description, detail: firstError.Code);
    }
}
