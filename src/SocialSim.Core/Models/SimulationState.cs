namespace SocialSim.Core.Models;

/// <summary>
/// Runtime state tracking for a simulation run.
/// This is intended to be persisted and queried by the UI.
/// </summary>
public sealed class SimulationState
{
    public Guid RunId { get; set; }

    public Guid ConfigurationId { get; set; }

    public SimulationRunStatus Status { get; set; } = SimulationRunStatus.Created;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? EndedAtUtc { get; set; }

    public long CurrentTick { get; set; } = 0;

    public DateTime CurrentSimulatedTimeUtc { get; set; } = DateTime.UtcNow;

    public int ActiveAgentCount { get; set; } = 0;

    /// <summary>
    /// Most recent metrics sample.
    /// </summary>
    public SimulationMetricsSnapshot LatestMetrics { get; set; } = new();

    public string? LastError { get; set; }

    public List<ScenarioRuntimeState> ScenarioStates { get; set; } = new();

    public List<CampaignRuntimeState> CampaignStates { get; set; } = new();
}

public enum SimulationRunStatus
{
    Created,
    Running,
    Paused,
    Stopped,
    Completed,
    Failed
}

public sealed class SimulationMetricsSnapshot
{
    public Guid RunId { get; set; }

    public long Tick { get; set; }

    public DateTime ObservedAtUtc { get; set; } = DateTime.UtcNow;

    public int ActiveAgents { get; set; }

    public int PostsCreated { get; set; }

    public EngagementCounters Engagements { get; set; } = new();

    public NetworkMetrics Network { get; set; } = new();

    public SentimentMetrics Sentiment { get; set; } = new();

    public List<TrendingTopic> TrendingTopics { get; set; } = new();

    public Dictionary<string, object?> Custom { get; set; } = new();
}

public sealed class EngagementCounters
{
    public int Likes { get; set; }
    public int Reposts { get; set; }
    public int Replies { get; set; }
    public int Quotes { get; set; }
}

public sealed class NetworkMetrics
{
    public double Density { get; set; }
    public double ClusteringCoefficient { get; set; }
    public int? ApproxDiameter { get; set; }
}

public sealed class SentimentMetrics
{
    public double Mean { get; set; }
    public double StdDev { get; set; }

    /// <summary>
    /// Optional histogram buckets (bucket label -> count).
    /// </summary>
    public Dictionary<string, int> Histogram { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class TrendingTopic
{
    public string Topic { get; set; } = string.Empty;
    public double Score { get; set; }
}

public sealed class ScenarioRuntimeState
{
    public Guid ScenarioId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public long? ActivatedAtTick { get; set; }
    public Dictionary<string, object?> State { get; set; } = new();
}

public sealed class CampaignRuntimeState
{
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public long? ActivatedAtTick { get; set; }
    public Dictionary<string, object?> State { get; set; } = new();
}
