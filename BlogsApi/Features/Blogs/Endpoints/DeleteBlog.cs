using BlogsApi.Extensions;
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
[MapDelete("api/v1/blogs/{id:guid}")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class DeleteBlog
{
    internal static Results<Ok, BadRequest<Error>, ValidationProblem> TransformResult(Result result)
    {
        return result.TransformResult("DeleteBlog");
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
        IValidator<Request> validator,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {

        FluentValidation.Results.ValidationResult validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.ValidationFailure(validationResult);
        }

        Blog? blog = await dbContext.Blogs
                                .Where(x => x.DeleteDate == null)
                                .FirstOrDefaultAsync(x => x.Id == request.Id && x.Author == currentUser.Id, cancellationToken: cancellationToken);

        if (blog is null)
        {
            return Result.Failure(new Error(
                "DeleteBlog.Handle",
                "El Blog no existe"));
        }

        blog.DeleteDate = DateTime.Now;

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return Result.Success();
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
        }
    }
}