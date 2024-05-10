using BlogsApi.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlogsApi.Extensions;

public static class ResultExtensions
{

    public static Results<Ok<T>, BadRequest<Error>, ValidationProblem> TransformResult<T>(this Result<T> result, string className)
    {
        return result.IsFailure
            ? result.ValidationError != null
                ? TypedResults.ValidationProblem(result.ValidationError.ToValidationDictionary(), title: $"Errores de validación en {className}")
                : TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public static Results<Ok, BadRequest<Error>, ValidationProblem> TransformResult(this Result result, string className)
    {
        return result.IsFailure
            ? result.ValidationError != null
                ? TypedResults.ValidationProblem(result.ValidationError.ToValidationDictionary(), title: $"Errores de validación en {className}")
                : TypedResults.BadRequest(result.Error)
            : TypedResults.Ok();
    }
}
