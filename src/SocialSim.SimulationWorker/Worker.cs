using SocialSim.Core.Events;

namespace SocialSim.SimulationWorker;

/// <summary>
/// Background service that runs the social network simulation
/// Generates events based on agent behaviors and network dynamics
/// </summary>
public class Worker(ILogger<Worker> logger) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Social Network Simulation Worker starting at: {time}", DateTimeOffset.Now);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Simulate various social network activities
                await SimulateNetworkActivity(stoppingToken);
                
                // Run simulation tick every 5 seconds
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in simulation worker");
            }
        }
        
        logger.LogInformation("Social Network Simulation Worker stopping at: {time}", DateTimeOffset.Now);
    }
    
    private async Task SimulateNetworkActivity(CancellationToken cancellationToken)
    {
        // TODO: Load agents from database
        // TODO: Generate events based on agent behaviors
        // TODO: Publish events to event bus/message queue
        // TODO: Update graph database with new connections
        
        logger.LogInformation("Simulation tick at: {time}", DateTimeOffset.Now);
        
        // Placeholder: Generate random simulation events
        var eventType = Random.Shared.Next(0, 3);
        
        switch (eventType)
        {
            case 0:
                var postEvent = new PostCreatedEvent
                {
                    AgentId = Guid.NewGuid(),
                    PostId = Guid.NewGuid(),
                    Content = "Simulated post content"
                };
                logger.LogInformation("Generated event: {EventType}", postEvent.EventType);
                break;
                
            case 1:
                var followEvent = new AgentFollowedEvent
                {
                    SourceAgentId = Guid.NewGuid(),
                    TargetAgentId = Guid.NewGuid()
                };
                logger.LogInformation("Generated event: {EventType}", followEvent.EventType);
                break;
                
            case 2:
                var engagementEvent = new PostEngagementEvent
                {
                    AgentId = Guid.NewGuid(),
                    PostId = Guid.NewGuid(),
                    Type = EngagementType.Like
                };
                logger.LogInformation("Generated event: {EventType}", engagementEvent.EventType);
                break;
        }
        
        await Task.CompletedTask;
    }
}

