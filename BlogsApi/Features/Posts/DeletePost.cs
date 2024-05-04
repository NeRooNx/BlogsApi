﻿using BlogsApi.Features.Authentication.Service;
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
[MapDelete("api/v1/posts/{id:guid}")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class DeletePost
{
    internal static Results<Ok, BadRequest<Error>> TransformResult(Result result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok();
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

    private static async ValueTask<Result> Handle(
        Request request,
        BlogsDBContext dbContext,
        IValidator<Request> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error(
                "DeletePost.Validation",
                validationResult.ToString()));
        }

        Post post = await dbContext.Posts
                                    .Where(x => x.Id == request.Id)
                                    .FirstAsync();

        post.DeleteDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return Result.Success();
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
                                .Where(y => y.Id == x)
                                .Where(y => y.DeleteDate == null)
                                .Any();
                })
                .WithMessage("El Post no existe");
        }
    }
}
