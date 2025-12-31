namespace SocialSim.Core.Models;

/// <summary>
/// A reusable, versioned template describing how to run a simulation.
/// Stored as data (e.g., PostgreSQL) and used to initialize a run.
/// </summary>
public sealed class SimulationConfiguration
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Master seed for reproducibility.
    /// Same seed + same config should yield the same event stream.
    /// </summary>
    public long Seed { get; set; } = 1;

    public SimulationTimeConfiguration Time { get; set; } = new();

    public AgentPopulationConfiguration Population { get; set; } = new();

    public NetworkTopologyConfiguration Topology { get; set; } = new();

    public TerminationConditions Termination { get; set; } = new();

    /// <summary>
    /// Optional scenario references to activate for this configuration.
    /// </summary>
    public List<Guid> ScenarioIds { get; set; } = new();

    /// <summary>
    /// Optional campaign references to activate for this configuration.
    /// </summary>
    public List<Guid> CampaignIds { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class SimulationTimeConfiguration
{
    public SimulationTimeMode Mode { get; set; } = SimulationTimeMode.Accelerated;

    /// <summary>
    /// Duration of a simulation tick.
    /// </summary>
    public TimeSpan TickDuration { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Wall-clock acceleration factor (e.g., 20 = 20x faster than real time).
    /// Used when Mode is Accelerated.
    /// </summary>
    public double AccelerationFactor { get; set; } = 1.0;

    public DateTime StartTimeUtc { get; set; } = DateTime.UtcNow;
}

public enum SimulationTimeMode
{
    RealTime,
    Accelerated,
    StepByStep
}

public sealed class AgentPopulationConfiguration
{
    public int AgentCount { get; set; } = 1000;

    /// <summary>
    /// Named cohorts to support heterogeneous populations.
    /// </summary>
    public List<AgentCohortConfiguration> Cohorts { get; set; } = new();

    /// <summary>
    /// Baseline behavior defaults applied when cohorts do not override.
    /// </summary>
    public AgentBehavior BaselineBehavior { get; set; } = new();

    public AgentPersonality BaselinePersonality { get; set; } = new();

    public ActivityPattern BaselineActivityPattern { get; set; } = new();

    public ContentPreferences BaselineContentPreferences { get; set; } = new();

    public SocialBehaviorProfile BaselineSocialBehavior { get; set; } = new();

    public InfluenceSusceptibilityProfile BaselineInfluenceSusceptibility { get; set; } = new();
}

public sealed class AgentCohortConfiguration
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cohort weight in [0,1]. Remaining probability mass uses baseline.
    /// </summary>
    public double Weight { get; set; } = 0.0;

    public AgentBehavior? BehaviorOverride { get; set; }
    public AgentPersonality? PersonalityOverride { get; set; }
    public ActivityPattern? ActivityPatternOverride { get; set; }
    public ContentPreferences? ContentPreferencesOverride { get; set; }
    public SocialBehaviorProfile? SocialBehaviorOverride { get; set; }
    public InfluenceSusceptibilityProfile? InfluenceSusceptibilityOverride { get; set; }

    /// <summary>
    /// Free-form parameters for future cohort behavior extensions.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = new();
}

public sealed class NetworkTopologyConfiguration
{
    public NetworkTopologyMode Mode { get; set; } = NetworkTopologyMode.SmallWorld;

    /// <summary>
    /// Topology parameters (e.g., { k: 12, rewireProb: 0.08 }).
    /// Stored as data to avoid schema churn.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = new();
}

public enum NetworkTopologyMode
{
    Random,
    ScaleFree,
    SmallWorld,
    Custom
}

public sealed class TerminationConditions
{
    public long? MaxTicks { get; set; }

    public TimeSpan? MaxWallClockDuration { get; set; }

    public TimeSpan? MaxSimulatedDuration { get; set; }

    public long? TargetEventCount { get; set; }

    public Dictionary<string, object?> Custom { get; set; } = new();
}
