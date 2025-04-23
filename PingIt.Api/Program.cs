using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Load .env
DotNetEnv.Env.Load();

// Retrieve connection string template from appsettings.json
var connectionTemplate = builder.Configuration.GetConnectionString("PostgresConnection");

// Replace placeholders in connection string template with values from .env
var connectionString = connectionTemplate
    .Replace("{username}", Env.GetString("PGUSERNAME"))
    .Replace("{password}", Env.GetString("PGPASSWORD"));

// DbContext registreren met dynamische connection string
builder.Services.AddDbContext<PingItDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
