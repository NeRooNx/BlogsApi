using BlogsApi.Extensions;
using BlogsApi.Features.Authentication.Service;
using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
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
    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("DeleteUser");
    }

    public record Request
    {
        public Guid Id { get; set; }
    }

    public record Response
    {
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request,
        BlogsDBContext dbContext,
        IValidator<Request> validator,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.ValidationFailure<Response>(validationResult);
        }

        Type? x = MethodBase.GetCurrentMethod()?.DeclaringType;
        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new($"{nameof(DeleteUser)}.Handle", "El User no existe."));
        }

        user.DeleteDate = DateTime.Now;

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return Result.Success(new Response());
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
        }
    }
}
