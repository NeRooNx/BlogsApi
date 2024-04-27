using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapGet("api/v1/users")]
public partial class GetUsers
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
    }

    public class Response
    {
        public List<UserDto>? users { get; set; }
    }
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Nickname { get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(Request request, BlogsDBContext dbContext, IValidator<Request> validator, CancellationToken cancellationToken)
    {

        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "CreateUser.Validation",
                validationResult.ToString()));
        }


        List<User> user = await dbContext.Users.ToListAsync();

        List<UserDto> dtoList = new();

        user.ForEach(x =>
        {
            UserDto dto = new()
            {
                Name = x.Name,
                LastName = x.LastName,
                Email = x.Email,
                Nickname = x.Nickname,
                Id = x.Id
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
