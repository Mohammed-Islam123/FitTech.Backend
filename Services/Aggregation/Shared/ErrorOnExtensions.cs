using ErrorOr;

namespace Aggregation.Shared;

public static class ErrorOnExtensions
{
    public static IResult MapErrorsToResult(List<Error> errors)
    {
        var firstError = errors[0];
        var statusCode = firstError.Type switch
        {
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(statusCode: statusCode, title: firstError.Description, detail: firstError.Code);
    }
}
