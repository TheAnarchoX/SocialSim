namespace SocialSim.Core.Events;

/// <summary>
/// Base class for all simulation events
/// </summary>
public abstract class SimulationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
}

/// <summary>
/// Event triggered when an agent creates a post
/// </summary>
public class PostCreatedEvent : SimulationEvent
{
    public Guid AgentId { get; set; }
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    
    public PostCreatedEvent()
    {
        EventType = nameof(PostCreatedEvent);
    }
}

/// <summary>
/// Event triggered when an agent follows another agent
/// </summary>
public class AgentFollowedEvent : SimulationEvent
{
    public Guid SourceAgentId { get; set; }
    public Guid TargetAgentId { get; set; }
    
    public AgentFollowedEvent()
    {
        EventType = nameof(AgentFollowedEvent);
    }
}

/// <summary>
/// Event triggered when an agent engages with a post
/// </summary>
public class PostEngagementEvent : SimulationEvent
{
    public Guid AgentId { get; set; }
    public Guid PostId { get; set; }
    public EngagementType Type { get; set; }
    
    public PostEngagementEvent()
    {
        EventType = nameof(PostEngagementEvent);
    }
}

public enum EngagementType
{
    Like,
    Repost,
    Reply,
    Quote
}

/// <summary>
/// Event to trigger a simulation scenario (e.g., viral event, trending topic)
/// </summary>
public class SimulationScenarioEvent : SimulationEvent
{
    public string ScenarioType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public SimulationScenarioEvent()
    {
        EventType = nameof(SimulationScenarioEvent);
    }
}
