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

namespace BlogsApi.Features.Endpoints.Blogs;

[Handler]
[MapGet("api/v1/blogs/{id:guid}")]
[Authorize]
public partial class GetBlog
{
    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("GetBlog");
    }

    public record Request
    {
        public Guid Id { get; set; }
    }

    public record Response
    {
        public Author? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreationDate { get; set; }
        public required int PostQuantity { get; set; }
    }

    public record Author
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Nickname { get; set; }
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

        Blog? blog = await dbContext.Blogs
            .Include(x => x.AuthorNavigation)
            .Include(x => x.Posts)
            .Where(x => x.DeleteDate == null)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

        if (blog is null)
        {
            return Result.Failure<Response>(new("GetBlog.Handle", "El Blog no existe."));
        }

        Response response = new Response()
        {
            CreationDate = blog.CreationDate,
            Title = blog.Title,
            Description = blog.Description,
            PostQuantity = blog.Posts.Count,
            Author = new()
            {
                Id = blog.AuthorNavigation?.Id,
                LastName = blog.AuthorNavigation?.LastName,
                Name = blog.AuthorNavigation?.Name,
                Nickname = blog.AuthorNavigation?.Nickname,
            },
        };

        return Result.Success<Response>(response);
    }
}
