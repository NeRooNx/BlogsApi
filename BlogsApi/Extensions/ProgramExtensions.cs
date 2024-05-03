using BlogsApi.Features.Authentication.Service;
using BlogsApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BlogsApi.Extensions;

public static class ProgramExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection service, IConfiguration configuration)
    {
        string secretKey = configuration.GetValue<string>("Token:JWT_SECRET_KEY") ?? throw new MissingFieldException("Token:JWT_SECRET_KEY");
        string audienceToken = configuration.GetValue<string>("Token:JWT_AUDIENCE_TOKEN") ?? throw new MissingFieldException("Token:JWT_AUDIENCE_TOKEN");
        string issuerToken = configuration.GetValue<string>("Token:JWT_ISSUER_TOKEN") ?? throw new MissingFieldException("Token:JWT_ISSUER_TOKEN");

        SymmetricSecurityKey securityKey = new(Encoding.Default.GetBytes(secretKey));

        service.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuerToken,
                    ValidAudience = audienceToken,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        return service;
    }

    public static IServiceCollection AddScopes(this IServiceCollection services)
    {
        services.AddScoped<JwtTokenHelper>();
        services.AddScoped<CurrentUser>();

        return services;
    }

}
