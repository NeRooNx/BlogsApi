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
using Microsoft.EntityFrameworkCore;

namespace BlogsApi.Features.Endpoints.Users;

[Handler]
[MapPut("api/v1/users/edit")]
[Authorize(Policy = PolicyConstants.USER)]
public partial class EditUser
{

    internal static Results<Ok<Response>, BadRequest<Error>, ValidationProblem> TransformResult(Result<Response> result)
    {
      return result.TransformResult("EditUser");
    }

    public record Request
    {
        public string? Name { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? Nickname { get; set; }

    }

    public record Response
    {
        public Guid Id { get; set; }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request,
        BlogsDBContext dBContext,
        IValidator<Request> validator,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {

        FluentValidation.Results.ValidationResult validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.ValidationFailure<Response>(validationResult);
        }

        User? user = await dBContext.Users
                            .Where(x => x.DeleteDate == null)
                            .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new("EditUser.Handle", "El User no existe."));
        }

        user.Nickname = request.Nickname;
        user.Name = request.Name;
        user.Email = request.Email;
        user.LastName = request.LastName;

        await dBContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return Result.Success<Response>(new() { Id = user.Id });
    }

    public class Validator : AbstractValidator<Request>
    {
        public const string ClassName = nameof(EditUser);
        public Validator(BlogsDBContext dbContext, CurrentUser currentUser)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .Must(x => !dbContext.Users.Where(y => y.Id != currentUser.Id && y.Email == x).Any())
                    .WithMessage("Este email ya está registrado")
                    .WithName("Email");

            RuleFor(x => x.Nickname)
                .NotNull()
                .NotEmpty()
                .Must(x => !dbContext.Users.Where(y => y.Id != currentUser.Id && y.Nickname == x).Any())
                    .WithMessage("Este nickname ya está registrado")
                    .WithName("Nickname");
        }
    }
}
