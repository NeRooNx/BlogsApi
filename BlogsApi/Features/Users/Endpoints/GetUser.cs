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

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapGet("api/v1/users/{id:guid}")]
[Authorize]
public partial class GetUser
{
    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("GetUser");
    }

    public record Request
    {
        public Guid Id { get; set; }
    }

    public record Response
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Nickname { get; set; }

        public int BlogsQuantity => Blogs.Count;

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
        IValidator<Request> validator,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.ValidationFailure<Response>(validationResult);
        }

        User? user = await dbContext.GetUserWithBlogsAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new("GetUser.Handle", "El User no existe."));
        }

        Response response = new()
        {
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            Nickname = user.Nickname,
            Id = user.Id,
            Blogs = user.Blogs
                        .Select(x => new Blog() { Id = x.Id, Title = x.Title ?? "", CreationDate = x.CreationDate!.Value })
                        .ToList(),
        };

        return Result.Success(response);
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
        }
    }
}
