using Microsoft.AspNetCore.SignalR;

namespace LemonTodo.Api.Hubs;

public class TaskHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
