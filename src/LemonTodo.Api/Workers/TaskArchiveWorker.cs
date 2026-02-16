using LemonTodo.Api.Hubs;
using LemonTodo.Domain.Events;
using LemonTodo.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LemonTodo.Api.Workers;

public class TaskArchiveWorker : BackgroundService
{
    private readonly ITaskEventChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<TaskHub> _hubContext;
    private readonly ILogger<TaskArchiveWorker> _logger;

    public TaskArchiveWorker(
        ITaskEventChannel channel,
        IServiceScopeFactory scopeFactory,
        IHubContext<TaskHub> hubContext,
        ILogger<TaskArchiveWorker> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evt in _channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                if (evt is TaskClosedEvent closedEvent)
                {
                    await ArchiveTaskAsync(closedEvent.TaskId, stoppingToken);
                    await _hubContext.Clients.All.SendAsync("TaskClosed", closedEvent.TaskId, stoppingToken);
                    _logger.LogInformation("Task {TaskId} archived and SignalR notified", closedEvent.TaskId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event for task {TaskId}", evt.TaskId);
            }
        }
    }

    private async Task ArchiveTaskAsync(string taskId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var activeRepo = scope.ServiceProvider.GetRequiredService<IActiveTaskRepository>();
        var archiveRepo = scope.ServiceProvider.GetRequiredService<IArchiveTaskRepository>();

        var task = await activeRepo.GetByIdAsync(taskId, ct);
        if (task is null)
        {
            _logger.LogWarning("Task {TaskId} not found in active store for archival", taskId);
            return;
        }

        await archiveRepo.AddAsync(task, ct);
        await activeRepo.DeleteAsync(taskId, ct);
    }
}
