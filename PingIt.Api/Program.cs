using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using PingIt.Api.Data;
using PingIt.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env
DotNetEnv.Env.Load();

// Get connection string template
var connectionTemplate = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");

// Replace placeholders from .env
var connectionString = connectionTemplate
    .Replace("{username}", Env.GetString("PGUSERNAME"))
    .Replace("{password}", Env.GetString("PGPASSWORD"));

// JWT settings
var jwtKey = Env.GetString("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET not found in .env");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Services
builder.Services.AddDbContext<PingItDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<JwtService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddSingleton<EmailService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7264") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

var uploads = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploads))
    Directory.CreateDirectory(uploads);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploads),
    RequestPath = "/Uploads"
});

app.MapControllers();

app.Run();
