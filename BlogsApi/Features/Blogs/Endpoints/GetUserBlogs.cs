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
[MapGet("api/v1/users/{id:guid}/blogs")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class GetUserBlogs
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public record Request
    {
        public required Guid Id { get; set; }
    }

    public record Response
    {
        public List<Blog> Blogs { get; set; } = [];
    }

    public record Blog
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required DateTime CreationDate { get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request, 
        BlogsDBContext dbContext, 
        CurrentUser currentUser,
        IValidator<Request> validator, 
        CancellationToken cancellationToken)
    {
        User? user = await dbContext.GetUserWithBlogs(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new("GetUserBlogs.Handle", "El User no existe."));
        }

        Response response = new()
        {
            Blogs = user.Blogs
                        .Select(x => new Blog() { Id = x.Id, Title = x.Title ?? "", CreationDate = x.CreationDate!.Value })
                        .ToList(),
        };

        return response;

    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
        }
    }
}
