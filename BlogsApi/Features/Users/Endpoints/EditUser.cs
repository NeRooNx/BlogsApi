using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapPut("api/v1/users")]
public partial class EditUser
{

    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
        public required Guid Id { get; set; }
        public string? Name { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Nickname { get; set; }

    }


    public class Response
    {
        public Guid Id { get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(Request request, BlogsDBContext dBContext, IValidator<Request> validator, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "EditUser.Validation",
                validationResult.ToString()));
        }

        User? user = await dBContext.Users.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new("EditUser.Handle", "El User no existe."));
        }

        user.Nickname = request.Nickname;
        user.Name = request.Name;
        user.Email = request.Email;
        user.Password = request.Password;
        user.LastName = request.LastName;

        await dBContext.SaveChangesAsync(cancellationToken);

        return new Response()
        {
            Id = user.Id,
        };
    }

    public class Validator : AbstractValidator<Request>
    {
        public const string ClassName = nameof(EditUser);
        public Validator() 
        { 
        
        }
    }


}
