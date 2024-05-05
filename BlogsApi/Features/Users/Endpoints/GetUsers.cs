using BlogsApi.Features.Authentication.Service;
using BlogsApi.Shared;
using BlogsApi.Shared.Constants;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapGet("api/v1/users")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class GetUsers
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public record Request
    {
    }

    public record Response
    {
        public List<UserDto>? users { get; set; }
    }
    public record UserDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Nickname { get; set; }
        public int BlogsQuantity {get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request, 
        BlogsDBContext dbContext, 
        IValidator<Request> validator, 
        CurrentUser current,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "CreateUser.Validation",
                validationResult.ToString()));
        }


        List<User> user = await dbContext.Users
                                .Include(x => x.Blogs)
                                .Where(x => x.DeleteDate == null)
                                .ToListAsync(cancellationToken: cancellationToken);

        List<UserDto> dtoList = new();

        user.ForEach(x =>
        {
            UserDto dto = new()
            {
                Name = x.Name,
                LastName = x.LastName,
                Email = x.Email,
                Nickname = x.Nickname,
                Id = x.Id,
                BlogsQuantity = x.Blogs.Count,
            };

            dtoList.Add(dto);
        });

        Response response = new()
        {
            users = dtoList
        };

        return Result.Success(response);
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
        }
    }
}
