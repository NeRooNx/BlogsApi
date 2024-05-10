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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features;

[Handler]
[MapPost("api/v1/blogs/{BlogId:guid}/posts")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class CreatePost
{
    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("CreatePost");
    }

    [EndpointRegistrationOverride(EndpointRegistration.AsParameters)]
    public record Request
    {
        [FromRoute]
        public required Guid BlogId { get; set; }

        [FromBody]
        public required PostDto Post { get; set; }

    }

    public record PostDto
    {
        public required string Title { get; set; }
        public required string Body { get; set; }
    }

    public record Response
    {
        public Guid Id { get; set; }
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

        Blog blog = await dbContext.Blogs
                                    .Include(x => x.Posts)
                                    .Where(x => x.Id == request.BlogId)
                                    .FirstAsync(cancellationToken: cancellationToken);

        Post post = new()
        {
            Body = request.Post.Body,
            Title = request.Post.Title,
            Id = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            BlogId = blog.Id,
        };

        await dbContext.Posts.AddAsync(post, cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        Response response = new()
        {
            Id = post.Id,
        };

        return Result.Success(response);
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(BlogsDBContext dBContext, CurrentUser currentUser)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.BlogId)
                .Must(x =>
                {
                    return dBContext.Blogs
                                    .Where(x => x.Author == currentUser.Id)
                                    .Where(y => y.Id == x)
                                    .Any();
                })
                .WithMessage("El Blog no existe");

            RuleFor(x => x.Post.Title)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Post.Body)
                .NotNull()
                .NotEmpty();
        }
    }
}
