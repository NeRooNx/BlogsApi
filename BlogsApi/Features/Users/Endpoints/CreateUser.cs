﻿using BlogsApi.Extensions;
using BlogsApi.Infrastructure;
using BlogsApi.Shared;
using BlogsModel.Models;
using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapPost("api/v1/users")]
public partial class CreateUser
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
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

    private static async ValueTask<Result<Response>> Handle(Request request, BlogsDBContext dbContext, IValidator<Request> validator, CancellationToken cancellationToken)
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
            Password = BCryptHelper.EncryptPassword(request.Password!),
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
