namespace BlogsApi.Features.Endpoints.Users.GetUserDTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }
    public string? Nickname { get; set; }
}