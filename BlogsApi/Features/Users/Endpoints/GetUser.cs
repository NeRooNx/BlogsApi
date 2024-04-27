using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapGet("api/v1/users/{id:guid}")]
public partial class GetUser
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
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Nickname { get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(Request request, BlogsDBContext dbContext, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

        if(user is null)
        {
            return Result.Failure<Response>(new("GetUser.Handle", "El User no existe."));
        }


        Response response = new()
        {
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            Nickname = user.Nickname,
            Id = user.Id,
        };

        return Result.Success(response);
    }
}
