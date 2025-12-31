namespace SocialSim.Core.Models;

/// <summary>
/// A marketing campaign specification (promoted content, influencer activation, A/B tests).
/// </summary>
public sealed class Campaign
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0";

    public CampaignType CampaignType { get; set; } = CampaignType.PromotedPost;

    public bool Enabled { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    public CampaignBudget Budget { get; set; } = new();

    public AudienceCriteria TargetAudience { get; set; } = new();

    public PromotedContentSpec PromotedContent { get; set; } = new();

    public CampaignABTest? ABTest { get; set; }

    public List<ScenarioTrigger> Triggers { get; set; } = new();

    public List<ScenarioMetricRule> Metrics { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum CampaignType
{
    PromotedPost,
    InfluencerActivation,
    ABTest
}

public sealed class CampaignBudget
{
    public string Currency { get; set; } = "USD";
    public decimal Total { get; set; } = 0m;

    /// <summary>
    /// Optional hard cap on impressions.
    /// </summary>
    public long? ImpressionLimit { get; set; }

    /// <summary>
    /// Optional maximum impressions per agent.
    /// </summary>
    public int? MaxImpressionsPerAgent { get; set; }
}

public sealed class AudienceCriteria
{
    public List<string> InterestsAny { get; set; } = new();

    public RangeInt? FollowerCount { get; set; }

    /// <summary>
    /// Optional opinion constraints (dimension -> min/max).
    /// </summary>
    public Dictionary<string, RangeDouble> OpinionRanges { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> TimezonesAny { get; set; } = new();

    public Dictionary<string, object?> Custom { get; set; } = new();
}

public sealed class PromotedContentSpec
{
    public string TextTemplate { get; set; } = string.Empty;

    public string Tone { get; set; } = "Neutral";

    public PreferredMediaType MediaType { get; set; } = PreferredMediaType.Text;

    public Dictionary<string, object?> Parameters { get; set; } = new();
}

public sealed class CampaignABTest
{
    public List<CampaignVariant> Variants { get; set; } = new();

    /// <summary>
    /// Variant allocations (variant id -> weight). Should sum to 1.0.
    /// </summary>
    public Dictionary<string, double> Allocation { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class CampaignVariant
{
    public string Id { get; set; } = string.Empty;
    public PromotedContentSpec Content { get; set; } = new();
}

public sealed class RangeInt
{
    public int Min { get; set; }
    public int Max { get; set; }
}

public sealed class RangeDouble
{
    public double Min { get; set; }
    public double Max { get; set; }
}
