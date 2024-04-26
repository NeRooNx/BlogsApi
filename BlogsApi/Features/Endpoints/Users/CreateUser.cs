using BlogsApi.Extensions;
using BlogsApi.Shared;
using BlogsModel.Models;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BlogsApi.Features.Endpoints.Users;

public class CreateUser
{
    public class Request : IRequest<Result<Response>>
    {
        public string? Name { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Nickname { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Email no es valido");

            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty()
                .IsPassword();

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty();
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


            var user = dbContext.Users.Add(new User()
            {
                Email = request.Email,
                Password = request.Password,
                LastName = request.LastName,
                Name = request.Name,
                Nickname = request.Nickname,
                RegisterDate = DateTime.Now
            });

            await dbContext.SaveChangesAsync();

            Response response = new()
            {
                Id = user.Entity.Id
            };

            return Result.Success(response);
        }
    }
}

public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/users", async ([FromBody] CreateUser.Request request, ISender sender) =>
        {
            Result<CreateUser.Response> result = await sender.Send(request);

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result);
        });
    }
}
