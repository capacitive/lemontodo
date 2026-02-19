using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LemonTodo.Api.Tests.Helpers;
using LemonTodo.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LemonTodo.Api.Tests;

public class TaskEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TaskEndpointTests(WebApplicationFactory<Program> factory)
    {
        var testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

                services.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });
            });
        });
        _client = testFactory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTask_Returns201WithLocation()
    {
        var request = new CreateTaskRequest("Test task", "Description", new DateOnly(2026, 3, 1));

        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        task!.Name.Should().Be("Test task");
        task.Status.Should().Be("Open");
        task.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_Returns400()
    {
        var request = new CreateTaskRequest("", null, default);

        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_AfterCreate_ReturnsTask()
    {
        var request = new CreateTaskRequest("Findable task", null, new DateOnly(2026, 3, 1));
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", request);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await _client.GetAsync($"/api/tasks/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        task!.Name.Should().Be("Findable task");
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/tasks/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_Returns200()
    {
        var createReq = new CreateTaskRequest("Original", null, new DateOnly(2026, 3, 1));
        var createResp = await _client.PostAsJsonAsync("/api/tasks", createReq);
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var updateReq = new UpdateTaskRequest("Updated", "New desc", new DateOnly(2026, 4, 1));
        var response = await _client.PutAsJsonAsync($"/api/tasks/{created!.Id}", updateReq);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        task!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task StartTask_Returns200()
    {
        var createReq = new CreateTaskRequest("To start", null, new DateOnly(2026, 3, 1));
        var createResp = await _client.PostAsJsonAsync("/api/tasks", createReq);
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await _client.PatchAsync($"/api/tasks/{created!.Id}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        task!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task CloseTask_AfterStart_Returns200()
    {
        var createReq = new CreateTaskRequest("To close", null, new DateOnly(2026, 3, 1));
        var createResp = await _client.PostAsJsonAsync("/api/tasks", createReq);
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        await _client.PatchAsync($"/api/tasks/{created!.Id}/start", null);
        var response = await _client.PatchAsync($"/api/tasks/{created.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        task!.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task CloseOpenTask_Returns409()
    {
        var createReq = new CreateTaskRequest("Cannot close directly", null, new DateOnly(2026, 3, 1));
        var createResp = await _client.PostAsJsonAsync("/api/tasks", createReq);
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await _client.PatchAsync($"/api/tasks/{created!.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReopenOpenTask_Returns409()
    {
        var createReq = new CreateTaskRequest("Cannot reopen", null, new DateOnly(2026, 3, 1));
        var createResp = await _client.PostAsJsonAsync("/api/tasks", createReq);
        var created = await createResp.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await _client.PatchAsync($"/api/tasks/{created!.Id}/reopen", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
