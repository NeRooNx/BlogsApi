using BlogsApi;
using BlogsApi.Extensions;
using BlogsModel.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(t => t.FullName?.Replace('+', '.')));

builder.Services.AddDbContext<BlogsDBContext>(options =>
                                options.UseSqlServer(builder.Configuration.GetConnectionString("BlogsDB")));

System.Reflection.Assembly assembly = typeof(Program).Assembly;

builder.Services.AddAuth(builder.Configuration);

builder.Services.AddHandlers();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddHttpContextAccessor();

builder.Services.AddPolicies();

builder.Services.AddScopes();

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
