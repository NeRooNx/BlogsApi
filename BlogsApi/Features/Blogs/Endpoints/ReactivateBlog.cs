using BlogsApi.Features.Authentication.Service;
using BlogsApi.Shared;
using BlogsApi.Shared.Constants;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Blogs;

[Handler]
[MapPut("api/v1/blogs/{id:guid}")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class ReactivateBlog
{
    internal static Results<Ok, BadRequest<Error>> TransformResult(Result result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok();
    }

    public record Request
    {
        public required Guid Id {  get; set; }
    }

    public record Response
    {
    }

    private static async ValueTask<Result> Handle(
        Request request, 
        BlogsDBContext dbContext, 
        CurrentUser currentUser,
        IValidator<Request> validator, 
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error(
                "ReactivateBlog.Validation",
                validationResult.ToString()));
        }

        Blog? blog = await dbContext.Blogs
                                .Where(x => x.DeleteDate != null)
                                .FirstOrDefaultAsync(x => x.Id == request.Id && x.Author == currentUser.Id, cancellationToken: cancellationToken);

        if (blog is null)
        {
            return Result.Failure(new Error(
                "ReactivateBlog.Handle",
                "El Blog no existe"));
        }

        blog.DeleteDate = null;

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return Result.Success();
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
        }
    }
}