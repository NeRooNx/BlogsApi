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
[MapGet("api/v1/users/{userId:guid}/posts")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class GetUserPost
{
    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("GetUserPost");
    }

    public record Request
    {
        public required Guid UserId { get; set; }
    }

    public record Response
    {
        public List<PostDto> Posts { get; set; } = [];
    }
    public record PostDto
    {
        public required string Title { get; set; }
        public required string Body { get; set; }
        public required DateTime CreationDate { get; set; }
        public required Guid BlogId { get; set; }
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

        User user = await dbContext.Users
                                    .Include(x => x.Blogs)
                                        .ThenInclude(x => x.Posts.Where( x => x.DeleteDate == null))
                                    .Where(x => x.Id == request.UserId)
                                    .FirstAsync(cancellationToken: cancellationToken);

        Response response = new()
        {
            Posts = user.Blogs
                        .SelectMany(x => x.Posts)
                        .Select(x => new PostDto()
                        {
                            BlogId = x.BlogId!.Value,
                            Body = x.Body!,
                            CreationDate = x.CreationDate!.Value,
                            Title = x.Title!
                        })
                        .ToList(),
        };

        return Result.Success(response);
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(BlogsDBContext dBContext, CurrentUser currentUser)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.UserId)
                .Must(x =>
                {
                    return dBContext.Users
                                    .Where(x => x.DeleteDate == null)
                                    .Where(y => y.Id == x)
                                    .Any();
                })
                .WithMessage("El User no existe");
        }
    }
}
