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

namespace BlogsApi.Features.Endpoints.Blogs;

[Handler]
[MapPost("api/v1/blogs")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class CreateBlog
{
    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("CreateBlog");
    }

    public record Request
    {
        public required string Title { get; set; }
        public required string Description { get; set; }

        //public byte[]? Icon { get; set; } //TODO: crear filemanager y guardar el icono
    }

    public record Response
    {
        public Guid Id {  get; set; }
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

        Blog blog = new()
        {
            Author = currentUser.Id,
            CreationDate = DateTime.Now,
            Description = request.Description,
            Title = request.Title,
            Id = Guid.NewGuid()
        };

        await dbContext.Blogs.AddAsync(blog, cancellationToken: cancellationToken);

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
