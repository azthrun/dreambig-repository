using DreamBig.Repository.Abstractions;
using DreamBig.Repository.Cosmos.Extensions;
using SampleApi.Models;
using SampleApi.Repositories;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connStr = builder.Configuration.GetSection("Cosmos:ConnectionString").Value;
var databaseId = builder.Configuration.GetSection("Cosmos:DatabaseId").Value;
builder.Services.UseCosmos(connStr, databaseId);

// Manual adding repositories
//builder.Services.AddScoped<IRepository<Kid>, KidRepositoryManual>();
//builder.Services.AddScoped<IRepository<Activity>, ActivityRepository>();

// Automatic adding all repositories
builder.Services.AddCosmosRepositories(Assembly.GetExecutingAssembly());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
