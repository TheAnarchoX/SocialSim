using Microsoft.AspNetCore.SignalR;
using SocialSim.Core.Events;

namespace SocialSim.Api.Hubs;

/// <summary>
/// SignalR hub for real-time simulation updates
/// Broadcasts simulation events to connected clients
/// </summary>
public class SimulationHub : Hub
{
    private readonly ILogger<SimulationHub> _logger;

    public SimulationHub(ILogger<SimulationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcast a simulation event to all connected clients
    /// </summary>
    public async Task BroadcastEvent(SimulationEvent simulationEvent)
    {
        await Clients.All.SendAsync("ReceiveEvent", simulationEvent);
    }

    /// <summary>
    /// Subscribe to specific agent updates
    /// </summary>
    public async Task SubscribeToAgent(string agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"agent-{agentId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to agent {AgentId}", Context.ConnectionId, agentId);
    }

    /// <summary>
    /// Unsubscribe from agent updates
    /// </summary>
    public async Task UnsubscribeFromAgent(string agentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent-{agentId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from agent {AgentId}", Context.ConnectionId, agentId);
    }
}
