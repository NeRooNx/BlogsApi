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
[MapPut("api/v1/blogs")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class EditBlog
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public record Request
    {
        public required Guid Id {  get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
    }

    public record Response
    {
        public Guid Id {  get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request, 
        BlogsDBContext dbContext, 
        CurrentUser currentUser,
        IValidator<Request> validator, 
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "EditBlog.Validation",
                validationResult.ToString()));
        }

        Blog? blog = await dbContext.Blogs
                                .Where(x => x.DeleteDate == null)
                                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

        if (blog is null)
        {
            return Result.Failure<Response>(new Error(
                "EditBlog.Handle",
                "El Blog no existe"));
        }

        blog.Title = request.Title;
        blog.Description = request.Description;

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return new Response()
        {
            Id = blog.Id,
        };
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .NotNull()
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}