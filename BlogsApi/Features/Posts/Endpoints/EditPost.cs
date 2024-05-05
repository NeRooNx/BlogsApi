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
[MapPut("api/v1/posts/{id:guid}")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class EditPost
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    [EndpointRegistrationOverride(EndpointRegistration.AsParameters)]
    public record Request
    {
        [FromRoute]
        public required Guid Id { get; set; }

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
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "EditPost.Validation",
                validationResult.ToString()));
        }

        Post post = await dbContext.Posts
                                    .Where(x => x.Id == request.Id)
                                    .FirstAsync(cancellationToken: cancellationToken);

        post.Body = request.Post.Body;
        post.Title = request.Post.Title;

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

            RuleFor(x => x.Id)
                .Must(x =>
                {
                    return dBContext.Posts
                                .Include(x => x.Blog)
                                .Where(x => x.Blog!.Author == currentUser.Id)
                                .Where(x => x.DeleteDate == null)
                                .Where(y => y.Id == x)
                                .Any();
                })
                .WithMessage("El Post no existe");

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
