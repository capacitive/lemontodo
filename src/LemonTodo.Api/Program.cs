using System.Text;
using FluentValidation;
using LemonTodo.Api.Auth;
using LemonTodo.Api.Endpoints;
using LemonTodo.Api.Hubs;
using LemonTodo.Api.Workers;
using LemonTodo.Application.Interfaces;
using LemonTodo.Application.Services;
using LemonTodo.Application.Validators;
using LemonTodo.Domain.Interfaces;
using LemonTodo.Infrastructure.Auth;
using LemonTodo.Infrastructure.Channels;
using LemonTodo.Infrastructure.Data;
using LemonTodo.Infrastructure.IdGeneration;
using LemonTodo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR token from query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
})
.AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>("ApiKey", _ => { });

// Authorization
builder.Services.AddAuthorization();

// EF Core - Active (SQLite)
var activePath = Path.Combine(builder.Environment.ContentRootPath, "active.db");
builder.Services.AddDbContext<ActiveDbContext>(opt =>
    opt.UseSqlite($"Data Source={activePath}"));

// EF Core - Archive (SQLite)
var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, "archive.db");
builder.Services.AddDbContext<ArchiveDbContext>(opt =>
    opt.UseSqlite($"Data Source={sqlitePath}"));

// EF Core - Users (SQLite)
var usersDbPath = Path.Combine(builder.Environment.ContentRootPath, "users.db");
builder.Services.AddDbContext<UserDbContext>(opt =>
    opt.UseSqlite($"Data Source={usersDbPath}"));

// Repositories
builder.Services.AddScoped<IActiveTaskRepository, ActiveTaskRepository>();
builder.Services.AddScoped<IArchiveTaskRepository, ArchiveTaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IArchiveService, ArchiveService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// Infrastructure
builder.Services.AddSingleton<IIdGenerator, NanoIdGenerator>();
builder.Services.AddSingleton<ITaskEventChannel, TaskEventChannel>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator>(sp =>
    new JwtTokenGenerator(sp.GetRequiredService<JwtSettings>()));
builder.Services.AddSingleton<ITotpService, TotpService>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();

// Background worker
builder.Services.AddHostedService<TaskArchiveWorker>();

var app = builder.Build();

// Ensure DB schemas are current (recreate if model changed)
using (var scope = app.Services.CreateScope())
{
    var activeDb = scope.ServiceProvider.GetRequiredService<ActiveDbContext>();
    var archiveDb = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();

    foreach (var db in new DbContext[] { activeDb, archiveDb })
    {
        if (db.Database.EnsureCreated() == false)
        {
            // DB exists — check if schema matches model by testing for new columns
            try
            {
                db.Database.ExecuteSqlRaw("SELECT StartedAt, UserId FROM Tasks LIMIT 0");
            }
            catch
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        }
    }

    var userDb = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    userDb.Database.EnsureCreated();
}

// Middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Endpoints
app.MapAuthEndpoints();
app.MapAccountEndpoints();
app.MapTaskEndpoints();
app.MapArchiveEndpoints();

// SignalR Hub
app.MapHub<TaskHub>("/hubs/tasks");

app.Run();

// For integration tests
public partial class Program { }
