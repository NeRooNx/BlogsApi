using BlogsApi.Shared;
using BlogsModel.Models;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapDelete("api/v1/users/{id:guid}")]
[Authorize]
public partial class DeleteUser
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
        public Guid Id { get; set; }
    }

    public class Response
    {
    }

    private static async ValueTask<Result<Response>> Handle(Request request, BlogsDBContext dbContext, CancellationToken cancellationToken)
    {
        Type? x = MethodBase.GetCurrentMethod()?.DeclaringType;
        User? user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new($"{nameof(DeleteUser)}.Handle", "El User no existe."));
        }

        dbContext.Users.Remove(user);

        await dbContext.SaveChangesAsync();

        return Result.Success(new Response());
    }
}
