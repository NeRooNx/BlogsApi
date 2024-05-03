using BlogsApi.Extensions;
using BlogsApi.Infrastructure;
using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using FluentValidation.Results;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlogsApi.Features.Authentication.Endpoints;

[Handler]
[MapPost("api/v1/login")]
public partial class Login
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
        public required string User { get; set; }
        public required string Password { get; set; }
    }

    public class Response
    {
        public required string Token { get; set; }
        //devolver roles para comodidad del recurso que consuma la api
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty()
                .IsPassword();

            RuleFor(x => x.User)
                .NotNull()
                .NotEmpty();
        }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request,
        BlogsDBContext dbContext,
        IValidator<Request> validator,
        JwtTokenHelper jwtTokenHelper,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error("Login.Validation", validationResult.ToString()));
        }

        User? user = dbContext.Users.SingleOrDefault(x => x.Email == request.User || x.Nickname == request.User);

        if (user == null)
        {
            return Result.Failure<Response>(new("Login.Handle", "El User no existe."));
        }

        if (!BCryptHelper.ComparePasswords(request.Password, user.Password!))
        {
            return Result.Failure<Response>(new("Login.Handle", "El User o la contraseña no coinciden."));
        }

        string token = jwtTokenHelper.GenerateToken(user);

        Response response = new()
        {
            Token = token
        };

        return Result.Success(response);
    }
}