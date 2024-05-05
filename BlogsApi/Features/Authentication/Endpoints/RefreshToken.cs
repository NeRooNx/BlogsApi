using BlogsApi.Extensions;
using BlogsApi.Infrastructure;
using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Authentication.Endpoints;

[Handler]
[MapPost("api/v1/refresh")]
public partial class RefreshToken
{
    internal static Results<Ok<Response>, BadRequest<Error>, NotFound<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? result.Error == Error.SessionExpired 
                ? TypedResults.NotFound(result.Error) 
                : TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);

    }

    public record Request
    {
        public required string RefreshToken { get; set; }
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
        }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request,
        BlogsDBContext dbContext,
        IValidator<Request> validator,
        JwtTokenHelper jwtTokenHelper,
        CancellationToken cancellationToken)
    {
        UserSession? session = dbContext.UserSessions
                            .Include(x => x.User)
                            .Where(x => x.RefreshToken == request.RefreshToken)
                            .OrderByDescending(x => x.CreationDate)
                            .FirstOrDefault();

        if (session == null)
        {
            return Result.Failure<Response>(new("RefreshToken.Handle", "El Refresh Token no existe."));
        }

        if (DateTime.Now > session.ExpirationDate!.Value.AddDays(7))
        {
            return Result.Failure<Response>(Error.SessionExpired);
        }

        var newToken = jwtTokenHelper.GenerateToken(session.User!);

        UserSession newSession =  await dbContext.CreateUserSessionAsync(session.ExpirationDate!.Value, newToken.token, session.User!.Id, session.RefreshToken!, cancellationToken);

        Response response = new()
        {
            Token = newSession.Token!,
            Expiration = newSession.ExpirationDate!.Value,
            RefreshToken = newSession.RefreshToken!
        };

        return Result.Success(response);
    }
}