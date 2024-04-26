using BlogsApi.Features.Endpoints.Users.GetUserDTOs;
using BlogsApi.Shared;
using BlogsModel.Models;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Users;

public class GetUser
{
    public class Request : IRequest<Result<Response>>
    {
    }

    public class Response
    {
        public List<UserDto>? users { get; set; }
    }


    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
        }
    }

    internal sealed class Handler(BlogsDBContext dbContext, IValidator<Request> validator) : IRequestHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Request request, CancellationToken cancellationToken)
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
    }
}

public class GetUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/users", async (ISender sender) =>
        {
            Result<GetUser.Response> result = await sender.Send(new GetUser.Request());

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result);
        });
    }
}
