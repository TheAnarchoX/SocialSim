namespace SocialSim.Core.Events;

/// <summary>
/// Base class for all simulation events
/// </summary>
public abstract class SimulationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Simulation run identifier (enables multi-run storage/streaming).
    /// </summary>
    public Guid RunId { get; set; }

    /// <summary>
    /// Discrete simulation tick when the event occurred.
    /// </summary>
    public long Tick { get; set; }

    /// <summary>
    /// Optional correlation id for grouping related events (e.g., a scenario/campaign activation).
    /// </summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>
    /// Optional immediate cause event id.
    /// </summary>
    public Guid? CausedByEventId { get; set; }

    /// <summary>
    /// Optional bounded list of prior event ids that contributed to this event.
    /// </summary>
    public List<Guid> CausalityChain { get; set; } = new();

    /// <summary>
    /// Optional probability assigned to the selected action/branch.
    /// </summary>
    public double? Probability { get; set; }

    /// <summary>
    /// Optional confidence score (e.g., for classification, moderation heuristics).
    /// </summary>
    public double? Confidence { get; set; }

    /// <summary>
    /// Optional compact agent state snapshot at event time.
    /// Use small payloads (key/value) for streaming; store large snapshots in checkpoints.
    /// </summary>
    public Dictionary<string, object?> AgentState { get; set; } = new(StringComparer.OrdinalIgnoreCase);
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

public class PostEditedEvent : SimulationEvent
{
    public Guid AgentId { get; set; }
    public Guid PostId { get; set; }
    public string NewContent { get; set; } = string.Empty;

    public PostEditedEvent()
    {
        EventType = nameof(PostEditedEvent);
    }
}

public class PostDeletedEvent : SimulationEvent
{
    public Guid AgentId { get; set; }
    public Guid PostId { get; set; }

    public PostDeletedEvent()
    {
        EventType = nameof(PostDeletedEvent);
    }
}

/// <summary>
/// Event triggered when an agent follows another agent
/// </summary>
public class AgentFollowedEvent : SimulationEvent
{
    public Guid SourceAgentId { get; set; }
    public Guid TargetAgentId { get; set; }

    public FollowReasonCode Reason { get; set; } = FollowReasonCode.Unknown;
    
    public AgentFollowedEvent()
    {
        EventType = nameof(AgentFollowedEvent);
    }
}

public class AgentUnfollowedEvent : SimulationEvent
{
    public Guid SourceAgentId { get; set; }
    public Guid TargetAgentId { get; set; }

    public UnfollowReasonCode Reason { get; set; } = UnfollowReasonCode.Unknown;

    public AgentUnfollowedEvent()
    {
        EventType = nameof(AgentUnfollowedEvent);
    }
}

public class AgentBlockedEvent : SimulationEvent
{
    public Guid BlockerAgentId { get; set; }
    public Guid BlockedAgentId { get; set; }

    public ModerationReasonCode Reason { get; set; } = ModerationReasonCode.Other;

    public AgentBlockedEvent()
    {
        EventType = nameof(AgentBlockedEvent);
    }
}

public class AgentUnblockedEvent : SimulationEvent
{
    public Guid BlockerAgentId { get; set; }
    public Guid BlockedAgentId { get; set; }

    public AgentUnblockedEvent()
    {
        EventType = nameof(AgentUnblockedEvent);
    }
}

public class AgentMutedEvent : SimulationEvent
{
    public Guid MuterAgentId { get; set; }
    public Guid MutedAgentId { get; set; }

    public MuteReasonCode Reason { get; set; } = MuteReasonCode.Unknown;

    public AgentMutedEvent()
    {
        EventType = nameof(AgentMutedEvent);
    }
}

public class AgentUnmutedEvent : SimulationEvent
{
    public Guid MuterAgentId { get; set; }
    public Guid MutedAgentId { get; set; }

    public AgentUnmutedEvent()
    {
        EventType = nameof(AgentUnmutedEvent);
    }
}

public class AgentReportedEvent : SimulationEvent
{
    public Guid ReporterAgentId { get; set; }
    public Guid? ReportedAgentId { get; set; }
    public Guid? ReportedPostId { get; set; }

    public ModerationReasonCode Reason { get; set; } = ModerationReasonCode.Other;

    public string? Notes { get; set; }

    public AgentReportedEvent()
    {
        EventType = nameof(AgentReportedEvent);
    }
}

public class AgentMentionedEvent : SimulationEvent
{
    public Guid MentioningAgentId { get; set; }
    public Guid MentionedAgentId { get; set; }
    public Guid PostId { get; set; }
    public string? Tag { get; set; }

    public AgentMentionedEvent()
    {
        EventType = nameof(AgentMentionedEvent);
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

public class PostFlaggedEvent : SimulationEvent
{
    public Guid PostId { get; set; }
    public Guid FlaggedByAgentId { get; set; }
    public ModerationReasonCode Reason { get; set; } = ModerationReasonCode.Other;

    public PostFlaggedEvent()
    {
        EventType = nameof(PostFlaggedEvent);
    }
}

public class PostModeratedEvent : SimulationEvent
{
    public Guid PostId { get; set; }
    public ModerationAction Action { get; set; } = ModerationAction.None;
    public ModerationReasonCode Reason { get; set; } = ModerationReasonCode.Other;
    public string? Notes { get; set; }

    public PostModeratedEvent()
    {
        EventType = nameof(PostModeratedEvent);
    }
}

public enum ModerationAction
{
    None,
    Label,
    Remove,
    Quarantine,
    Shadowban
}

public class ViralThresholdCrossedEvent : SimulationEvent
{
    public Guid? PostId { get; set; }
    public string? Topic { get; set; }
    public string ThresholdType { get; set; } = string.Empty;
    public double ThresholdValue { get; set; }
    public double ObservedValue { get; set; }

    public ViralThresholdCrossedEvent()
    {
        EventType = nameof(ViralThresholdCrossedEvent);
    }
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

public class ScenarioTriggeredEvent : SimulationEvent
{
    public Guid ScenarioId { get; set; }
    public string ScenarioName { get; set; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public ScenarioTriggeredEvent()
    {
        EventType = nameof(ScenarioTriggeredEvent);
    }
}

public class SimulationStartedEvent : SimulationEvent
{
    public Guid ConfigurationId { get; set; }

    public SimulationStartedEvent()
    {
        EventType = nameof(SimulationStartedEvent);
    }
}

public class SimulationPausedEvent : SimulationEvent
{
    public string? Reason { get; set; }

    public SimulationPausedEvent()
    {
        EventType = nameof(SimulationPausedEvent);
    }
}

public class SimulationResumedEvent : SimulationEvent
{
    public string? Reason { get; set; }

    public SimulationResumedEvent()
    {
        EventType = nameof(SimulationResumedEvent);
    }
}

public class SimulationStoppedEvent : SimulationEvent
{
    public string? Reason { get; set; }

    public SimulationStoppedEvent()
    {
        EventType = nameof(SimulationStoppedEvent);
    }
}

public class CheckpointCreatedEvent : SimulationEvent
{
    public Guid CheckpointId { get; set; }

    public CheckpointCreatedEvent()
    {
        EventType = nameof(CheckpointCreatedEvent);
    }
}

public class CheckpointRestoredEvent : SimulationEvent
{
    public Guid CheckpointId { get; set; }

    public CheckpointRestoredEvent()
    {
        EventType = nameof(CheckpointRestoredEvent);
    }
}

public class AgentSpawnedEvent : SimulationEvent
{
    public Guid AgentId { get; set; }

    public AgentSpawnedEvent()
    {
        EventType = nameof(AgentSpawnedEvent);
    }
}

public class AgentDeactivatedEvent : SimulationEvent
{
    public Guid AgentId { get; set; }
    public string? Reason { get; set; }

    public AgentDeactivatedEvent()
    {
        EventType = nameof(AgentDeactivatedEvent);
    }
}

public class SimulationWarningEvent : SimulationEvent
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public SimulationWarningEvent()
    {
        EventType = nameof(SimulationWarningEvent);
    }
}

public class SimulationErrorEvent : SimulationEvent
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }

    public SimulationErrorEvent()
    {
        EventType = nameof(SimulationErrorEvent);
    }
}

public class DirectMessageSentEvent : SimulationEvent
{
    public Guid SenderAgentId { get; set; }
    public Guid RecipientAgentId { get; set; }
    public Guid MessageId { get; set; }
    public string? ContentPreview { get; set; }

    public DirectMessageSentEvent()
    {
        EventType = nameof(DirectMessageSentEvent);
    }
}

public enum FollowReasonCode
{
    Unknown,
    FollowBack,
    Recommendation,
    SharedInterest,
    Reciprocity,
    Trend,
    Other
}

public enum UnfollowReasonCode
{
    Unknown,
    Spam,
    LowQualityContent,
    OpinionDivergence,
    Inactivity,
    Other
}

public enum MuteReasonCode
{
    Unknown,
    Noise,
    Replies,
    Reposts,
    ConflictAvoidance,
    Other
}

public enum ModerationReasonCode
{
    Spam,
    Harassment,
    Violence,
    NSFW,
    Misinformation,
    Impersonation,
    SelfHarm,
    Other
}
