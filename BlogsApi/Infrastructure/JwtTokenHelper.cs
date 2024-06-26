﻿using BlogsApi.Shared.Constants;
using BlogsModel.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace BlogsApi.Infrastructure;

public class JwtTokenHelper(IConfiguration configuration)
{

    public string GenerateToken(User user)
    {
        string? secretKey = configuration.GetValue<string>("Token:JWT_SECRET_KEY");
        string? audienceToken = configuration.GetValue<string>("Token:JWT_AUDIENCE_TOKEN");
        string? issuerToken = configuration.GetValue<string>("Token:JWT_ISSUER_TOKEN");
        int expireTime = configuration.GetValue<int>("Token:JWT_EXPIRE_MINUTES");

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(secretKey));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        ClaimsIdentity claimsIdentity = new(new[] {
            new Claim(TokenConstants.EMAIL, user.Email!),
            new Claim(TokenConstants.ROLES, user.Roles ?? "User"),
            new Claim(TokenConstants.ID, user.Id.ToString()),
        });

        JwtSecurityTokenHandler tokenHandler = new();

        JwtSecurityToken jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
            audience: audienceToken,
            issuer: issuerToken,
            subject: claimsIdentity,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expireTime),
            signingCredentials: signingCredentials);

        string jwtTokenString = tokenHandler.WriteToken(jwtSecurityToken);

        return jwtTokenString;
    }
}
