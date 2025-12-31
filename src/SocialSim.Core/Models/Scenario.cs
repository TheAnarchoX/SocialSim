namespace SocialSim.Core.Models;

/// <summary>
/// A scenario is a configured set of triggers and actions (viral events, crises, interventions).
/// </summary>
public sealed class Scenario
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0";

    public ScenarioType ScenarioType { get; set; } = ScenarioType.Custom;

    public bool Enabled { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    public List<ScenarioTrigger> Triggers { get; set; } = new();

    public List<ScenarioAction> Actions { get; set; } = new();

    public List<ScenarioMetricRule> Metrics { get; set; } = new();

    public ScenarioConstraints Constraints { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum ScenarioType
{
    Viral,
    Marketing,
    Crisis,
    Custom
}

public sealed class ScenarioTrigger
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; set; } = new();
}

public sealed class ScenarioAction
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; set; } = new();
}

public sealed class ScenarioMetricRule
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object?> Parameters { get; set; } = new();
}

public sealed class ScenarioConstraints
{
    public int? MaxAffectedAgents { get; set; }
    public int? MaxPostsInjected { get; set; }
    public Dictionary<string, object?> Custom { get; set; } = new();
}
