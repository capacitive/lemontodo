using FluentValidation;
using LemonTodo.Api.Endpoints;
using LemonTodo.Api.Hubs;
using LemonTodo.Api.Workers;
using LemonTodo.Application.Interfaces;
using LemonTodo.Application.Services;
using LemonTodo.Application.Validators;
using LemonTodo.Domain.Interfaces;
using LemonTodo.Infrastructure.Channels;
using LemonTodo.Infrastructure.Data;
using LemonTodo.Infrastructure.IdGeneration;
using LemonTodo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
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

// EF Core - Active (InMemory)
builder.Services.AddDbContext<ActiveDbContext>(opt =>
    opt.UseInMemoryDatabase("ActiveTasks"));

// EF Core - Archive (SQLite)
var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, "archive.db");
builder.Services.AddDbContext<ArchiveDbContext>(opt =>
    opt.UseSqlite($"Data Source={sqlitePath}"));

// Repositories
builder.Services.AddScoped<IActiveTaskRepository, ActiveTaskRepository>();
builder.Services.AddScoped<IArchiveTaskRepository, ArchiveTaskRepository>();

// Services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IArchiveService, ArchiveService>();

// Infrastructure
builder.Services.AddSingleton<IIdGenerator, NanoIdGenerator>();
builder.Services.AddSingleton<ITaskEventChannel, TaskEventChannel>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();

// Background worker
builder.Services.AddHostedService<TaskArchiveWorker>();

var app = builder.Build();

// Ensure archive DB schema is current (recreate if model changed)
using (var scope = app.Services.CreateScope())
{
    var archiveDb = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();
    if (archiveDb.Database.EnsureCreated() == false)
    {
        // DB exists — check if schema matches model by testing for new columns
        try
        {
            archiveDb.Database.ExecuteSqlRaw("SELECT StartedAt FROM Tasks LIMIT 0");
        }
        catch
        {
            archiveDb.Database.EnsureDeleted();
            archiveDb.Database.EnsureCreated();
        }
    }
}

// Middleware
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Endpoints
app.MapTaskEndpoints();
app.MapArchiveEndpoints();

// SignalR Hub
app.MapHub<TaskHub>("/hubs/tasks");

app.Run();

// For integration tests
public partial class Program { }
