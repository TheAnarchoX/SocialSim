namespace SocialSim.Core.Models;

/// <summary>
/// Personality and opinion-related configuration for a simulated agent.
/// Values are intended to be probabilistic inputs (not deterministic rules).
/// </summary>
public sealed class AgentPersonality
{
    public BigFivePersonality BigFive { get; set; } = new();

    /// <summary>
    /// Opinion and political positioning for opinion dynamics simulations.
    /// </summary>
    public OpinionProfile Opinion { get; set; } = new();

    /// <summary>
    /// Sentiment bias and emotional dynamics.
    /// </summary>
    public SentimentProfile Sentiment { get; set; } = new();
}

/// <summary>
/// Big Five (OCEAN) personality traits.
/// Each trait is a continuous value from 0.0 to 1.0.
/// </summary>
public sealed class BigFivePersonality
{
    public double Openness { get; set; } = 0.5;
    public double Conscientiousness { get; set; } = 0.5;
    public double Extraversion { get; set; } = 0.5;
    public double Agreeableness { get; set; } = 0.5;
    public double Neuroticism { get; set; } = 0.5;
}

/// <summary>
/// Opinion positions and confidence levels.
/// Supports both a single political axis and arbitrary named dimensions.
/// </summary>
public sealed class OpinionProfile
{
    /// <summary>
    /// Optional single-axis political/opinion spectrum (-1.0 to 1.0).
    /// </summary>
    public double PoliticalSpectrum { get; set; } = 0.0;

    /// <summary>
    /// Named opinion dimensions with positions (-1.0 to 1.0).
    /// Dimension names are simulation-configurable.
    /// </summary>
    public Dictionary<string, double> Positions { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Confidence per opinion dimension (0.0 to 1.0).
    /// Higher confidence implies greater resistance to change.
    /// </summary>
    public Dictionary<string, double> Confidence { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// How strongly the agent expresses opinions (0.0 to 1.0).
    /// </summary>
    public double ExpressionIntensity { get; set; } = 0.5;

    /// <summary>
    /// Willingness to engage with opposing views (0.0 to 1.0).
    /// Lower values increase echo-chamber behavior.
    /// </summary>
    public double OppositionTolerance { get; set; } = 0.5;
}

/// <summary>
/// Sentiment bias and dynamics.
/// BaseSentiment is a long-term bias; CurrentSentiment is a runtime value.
/// </summary>
public sealed class SentimentProfile
{
    /// <summary>
    /// Long-term sentiment bias (-1.0 to 1.0).
    /// </summary>
    public double BaseSentiment { get; set; } = 0.0;

    /// <summary>
    /// Sentiment volatility (0.0 to 1.0).
    /// Higher values produce larger deviations from BaseSentiment.
    /// </summary>
    public double Volatility { get; set; } = 0.5;

    /// <summary>
    /// Recovery rate (0.0 to 1.0).
    /// Higher values return CurrentSentiment to BaseSentiment faster.
    /// </summary>
    public double RecoveryRate { get; set; } = 0.5;

    /// <summary>
    /// Runtime sentiment value (-1.0 to 1.0). Typically updated by the simulation engine.
    /// </summary>
    public double CurrentSentiment { get; set; } = 0.0;
}

/// <summary>
/// Social interaction tendencies.
/// </summary>
public sealed class SocialBehaviorProfile
{
    /// <summary>
    /// Probability of following back when followed (0.0 to 1.0).
    /// </summary>
    public double FollowBackRate { get; set; } = 0.3;

    /// <summary>
    /// Preference for reciprocal connections (0.0 to 1.0).
    /// </summary>
    public double ReciprocityPreference { get; set; } = 0.5;

    /// <summary>
    /// Engagement archetype.
    /// </summary>
    public EngagementStyle EngagementStyle { get; set; } = EngagementStyle.Mixed;

    /// <summary>
    /// Network position preference.
    /// </summary>
    public NetworkPositionPreference NetworkPreference { get; set; } = NetworkPositionPreference.Balanced;

    /// <summary>
    /// Probability to unfollow after negative experiences (0.0 to 1.0).
    /// Used as a high-level knob; specific triggers belong to simulation configuration.
    /// </summary>
    public double UnfollowSensitivity { get; set; } = 0.3;

    /// <summary>
    /// Minimum inactivity duration before considering unfollowing.
    /// </summary>
    public TimeSpan UnfollowAfterInactivity { get; set; } = TimeSpan.FromDays(30);
}

public enum EngagementStyle
{
    Lurker,
    Commenter,
    Creator,
    Mixed
}

public enum NetworkPositionPreference
{
    HubSeeker,
    Peripheral,
    Balanced
}

/// <summary>
/// Influence susceptibility and opinion adoption tendencies.
/// </summary>
public sealed class InfluenceSusceptibilityProfile
{
    /// <summary>
    /// Trend adoption rate (0.0 to 1.0).
    /// Higher values correspond to earlier adoption.
    /// </summary>
    public double TrendAdoptionRate { get; set; } = 0.5;

    /// <summary>
    /// Multiplier for influencer impact (0.0 to 5.0 typical).
    /// </summary>
    public double InfluencerEffectMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Echo chamber tendency (0.0 to 1.0).
    /// Higher values increase preference for similar opinions.
    /// </summary>
    public double EchoChamberTendency { get; set; } = 0.5;

    /// <summary>
    /// Resistance to opinion change (0.0 to 1.0).
    /// Higher values make opinions harder to shift.
    /// </summary>
    public double OpinionChangeResistance { get; set; } = 0.5;
}
