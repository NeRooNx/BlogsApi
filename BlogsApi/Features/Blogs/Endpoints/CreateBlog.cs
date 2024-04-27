using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlogsApi.Features.Endpoints.Blogs;

[Handler]
[MapPost("api/v1/blogs")]
public partial class CreateBlog
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
        public Guid Author { get; set; } //TODO: gestión de sesiones -> current user
        public required string Title { get; set; }
        public required string Description { get; set; }

        //public byte[]? Icon { get; set; } //TODO: crear filemanager y guardar el icono
    }

    public class Response
    {
        public Guid Id {  get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(Request request, BlogsDBContext dbContext, IValidator<Request> validator, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "CreateBlog.Validation",
                validationResult.ToString()));
        }


        Blog blog = new()
        {
            Author = request.Author,
            CreationDate = DateTime.Now,
            Description = request.Description,
            Title = request.Title,
            Id = Guid.NewGuid()
        };

        await dbContext.Blogs.AddAsync(blog);

        await dbContext.SaveChangesAsync();


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
