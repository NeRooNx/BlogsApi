using BlogsModel.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using BlogsApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(t => t.FullName?.Replace('+', '.')));

builder.Services.AddDbContext<BlogsDBContext>(options =>
                                options.UseSqlServer(builder.Configuration.GetConnectionString("BlogsDB")));

var assembly = typeof(Program).Assembly;


builder.Services.AddHandlers();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(
        o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapBlogsApiEndpoints();

app.UseAuthorization();

app.Run();
