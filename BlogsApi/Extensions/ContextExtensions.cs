using BlogsModel.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace BlogsApi.Extensions;

public static class ContextExtensions
{

    public static async Task<User?> GetUserWithBlogs(this BlogsDBContext dbContext, Guid userId, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
            .Include(x => x.Blogs)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken: cancellationToken);

        return user;
    }
}
