using BlogsApi.Extensions;
using BlogsApi.Features.Authentication.Service;
using BlogsApi.Infrastructure;
using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapPut("api/v1/users/edit/password")]
[Authorize]
public partial class EditUserPassword
{

    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public record Request
    {
        public string? Password { get; set; }
    }


    public record Response
    {
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request, 
        BlogsDBContext dBContext, 
        CurrentUser currentUser,
        IValidator<Request> validator, 
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "EditUserPassword.Validation",
                validationResult.ToString()));
        }

        User? user = await dBContext.Users
                                    .Where(x => x.DeleteDate == null)
                                    .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new("EditUserPassword.Handle", "El User no existe."));
        }

        user.Password = BCryptHelper.EncryptPassword(request.Password!);

        await dBContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return new Response();
    }

    public class Validator : AbstractValidator<Request>
    {
        public const string ClassName = nameof(EditUser);
        public Validator() 
        {
            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty()
                .IsPassword();
        }
    }


}
