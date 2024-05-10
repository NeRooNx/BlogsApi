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
[MapPost("api/v1/post/{postId:guid}/comments")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class CreateComment
{
    internal static void CustomizeEndpoint(IEndpointConventionBuilder endpoint) => endpoint.WithTags("Comment").WithDescription("Endpoint to create a comment under a specific post");


    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("CreateComment");
    }

    [EndpointRegistrationOverride(EndpointRegistration.AsParameters)]
    public record Request
    {
        [FromRoute]
        public required Guid PostId { get; set; }

        [FromBody]
        public required CommentDto Comment { get; set; }

    }

    public record CommentDto
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

        Comment comment = new()
        {
            Body = request.Comment.Body,
            Title = request.Comment.Title,
            Id = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            Post = request.PostId,
            Author = currentUser.Id
        };

        await dbContext.Comments.AddAsync(comment, cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        Response response = new()
        {
            Id = comment.Id,
        };

        return Result.Success(response);
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(BlogsDBContext dBContext, CurrentUser currentUser)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.PostId)
                .Must(x =>
                {
                    return dBContext.Posts
                                    .Include(x => x.Blog)
                                    .Where(x => x.Blog!.Author == currentUser.Id)
                                    .Where(y => y.Id == x)
                                    .Any();
                })
                .WithMessage("El Post no existe");

            RuleFor(x => x.Comment.Title)
                .NotNull()
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Comment.Body)
                .NotNull()
                .NotEmpty();
        }
    }
}
