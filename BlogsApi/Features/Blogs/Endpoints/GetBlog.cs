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
        public Author? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreationDate { get; set; }
    }

    public class Author
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Nickname { get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(Request request, BlogsDBContext dbContext, CancellationToken cancellationToken)
    {

        Blog? blog = await dbContext.Blogs
            .Include(x => x.AuthorNavigation)
            .SingleOrDefaultAsync(x => x.Id == request.Id);

        if (blog is null)
        {
            return Result.Failure<Response>(new("GetBlog.Handle", "El Blog no existe."));
        }

        Response response = new Response()
        {
            CreationDate = blog.CreationDate,
            Title = blog.Title,
            Description = blog.Description,
            Author = new()
            {
                Id = blog.AuthorNavigation?.Id,
                LastName = blog.AuthorNavigation?.LastName,
                Name = blog.AuthorNavigation?.Name,
                Nickname = blog.AuthorNavigation?.Nickname,
            }
        };

        return Result.Success<Response>(response);
    }
}
