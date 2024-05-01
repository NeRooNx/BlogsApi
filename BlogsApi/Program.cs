using BlogsApi;
using BlogsApi.Extensions;
using BlogsApi.Features.Authentication.Service;
using BlogsApi.Infrastructure;
using BlogsApi.Shared.Constants;
using BlogsModel.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(t => t.FullName?.Replace('+', '.')));

builder.Services.AddDbContext<BlogsDBContext>(options =>
                                options.UseSqlServer(builder.Configuration.GetConnectionString("BlogsDB")));

System.Reflection.Assembly assembly = typeof(Program).Assembly;


string secretKey = builder.Configuration.GetValue<string>("Token:JWT_SECRET_KEY") ?? throw new MissingFieldException("Token:JWT_SECRET_KEY");
string audienceToken = builder.Configuration.GetValue<string>("Token:JWT_AUDIENCE_TOKEN") ?? throw new MissingFieldException("Token:JWT_AUDIENCE_TOKEN");
string issuerToken = builder.Configuration.GetValue<string>("Token:JWT_ISSUER_TOKEN") ?? throw new MissingFieldException("Token:JWT_ISSUER_TOKEN");

SymmetricSecurityKey securityKey = new(Encoding.Default.GetBytes(secretKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuerToken,
            ValidAudience = audienceToken,
            IssuerSigningKey = securityKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddHandlers();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddHttpContextAccessor();

builder.Services.AddPolicies();

builder.Services.AddScoped<JwtTokenHelper>();
builder.Services.AddScoped<CurrentUser>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapBlogsApiEndpoints();

app.UseAuthorization();

app.Run();
