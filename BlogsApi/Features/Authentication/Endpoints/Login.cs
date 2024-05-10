using BlogsApi.Extensions;
using BlogsApi.Features.Authentication.Service;
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
    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
        return result.TransformResult("Login");
    }

    public record Request
    {
        public required string User { get; set; }
        public required string Password { get; set; }
    }

    public record Response
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime Expiration { get; set; }

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
        CurrentUser currentUser,
        JwtTokenHelper jwtTokenHelper,
        CancellationToken cancellationToken)
    {

        FluentValidation.Results.ValidationResult validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.ValidationFailure<Response>(validationResult);
        }

        User? user = dbContext.Users
                            .Where(x => x.DeleteDate == null)
                            .FirstOrDefault(x => x.Email == request.User || x.Nickname == request.User);

        if (user == null)
        {
            return Result.Failure<Response>(new("Login.Handle", "El User no existe."));
        }

        if (!BCryptHelper.ComparePasswords(request.Password, user.Password!))
        {
            return Result.Failure<Response>(new("Login.Handle", "El User o la contraseña no coinciden."));
        }

        (string token, DateTime expirationDate) = jwtTokenHelper.GenerateToken(user);

        UserSession session = await dbContext.CreateUserSessionAsync(expirationDate, token, user.Id, Guid.NewGuid().ToString().Replace("-", ""), cancellationToken);

        Response response = new()
        {
            Token = session.Token!,
            Expiration = session.ExpirationDate!.Value,
            RefreshToken = session.RefreshToken!
        };

        return Result.Success(response);
    }
}