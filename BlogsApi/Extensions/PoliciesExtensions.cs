using BlogsApi.Shared.Constants;

namespace BlogsApi.Extensions;

public static class PoliciesExtensions
{

    public static IServiceCollection AddPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                PolicyConstants.ADMIN,
                policy => policy.RequireClaim(TokenConstants.ROLES, RolesConstants.ADMIN));

            options.AddPolicy(PolicyConstants.USER,
                policy => policy.RequireClaim(TokenConstants.ROLES, RolesConstants.USER, RolesConstants.ADMIN));
        });

        return services;
    }

}