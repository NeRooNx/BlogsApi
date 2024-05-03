using BlogsApi.Shared.Constants;
using BlogsApi.Shared;

namespace BlogsApi.Features.Authentication.Service;

public record CurrentUser
{
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        System.Security.Claims.ClaimsPrincipal? currentUser = httpContextAccessor.HttpContext?.User;
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("No existe la sesión");
        }

        string? userIdString = currentUser.Claims.FirstOrDefault(x => x.Type == TokenConstants.ID)?.Value;
        string? userEmailString = currentUser.Claims.FirstOrDefault(x => x.Type == TokenConstants.EMAIL)?.Value;
        string? rolesString = currentUser.Claims.FirstOrDefault(x => x.Type == TokenConstants.ROLES)?.Value;

        if (userIdString is null || userEmailString is null || rolesString is null)
        {
            throw new UnauthorizedAccessException("Claims están vacíos");
        }

        Id = Guid.Parse(userIdString);
        Email = userEmailString;
        Role = rolesString;
    }


    public string Email { get; set; }
    public Guid Id { get; set; }
    public string Role {  get; set; }

}
