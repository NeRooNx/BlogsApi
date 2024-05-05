using BlogsModel.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace BlogsApi.Extensions;

public static class ContextExtensions
{

    public static async Task<User?> GetUserWithBlogsAsync(this BlogsDBContext dbContext, Guid userId, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
            .Include(x => x.Blogs)
            .Where(x => x.DeleteDate == null)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken: cancellationToken);

        return user;
    }

    public static async Task<User?> GetUserWithBlogsAndPostsAsync(this BlogsDBContext dbContext, Guid userId, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
            .Include(x => x.Blogs)
                .ThenInclude(x => x.Posts)
            .Where(x => x.DeleteDate == null)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken: cancellationToken);

        return user;
    }

    public static async Task<UserSession> CreateUserSessionAsync(this BlogsDBContext dbContext, DateTime expirationDate, string token, Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        UserSession newSession = new()
        {
            Id = Guid.NewGuid(),
            ExpirationDate = expirationDate,
            Token = token,
            UserId = userId,
            RefreshToken = refreshToken,
            CreationDate = DateTime.Now,
        };

        dbContext.UserSessions.Add(newSession);

        await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return newSession;
    }
}
