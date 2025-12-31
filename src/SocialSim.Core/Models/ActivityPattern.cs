namespace SocialSim.Core.Models;

/// <summary>
/// Time-based activity patterns for an agent.
/// Defines when an agent is likely to be active and how sessions behave.
/// </summary>
public sealed class ActivityPattern
{
    /// <summary>
    /// Relative activity probability by hour (0-23). Values are 0.0-1.0.
    /// Interpreted as weights, not absolute probabilities.
    /// </summary>
    public double[] HourlyActivity { get; set; } = new double[24];

    /// <summary>
    /// Timezone offset from UTC in hours.
    /// </summary>
    public int TimezoneOffsetHours { get; set; } = 0;

    /// <summary>
    /// Relative activity multiplier by day of week (0=Sunday..6=Saturday).
    /// </summary>
    public double[] DayOfWeekMultiplier { get; set; } = new double[7];

    /// <summary>
    /// Posting frequency distribution (e.g., Poisson, bursty).
    /// </summary>
    public PostingFrequencyModel PostingFrequency { get; set; } = new();

    /// <summary>
    /// Session duration and cadence model.
    /// </summary>
    public SessionBehavior SessionBehavior { get; set; } = new();
}

public sealed class PostingFrequencyModel
{
    public PostingFrequencyDistribution Distribution { get; set; } = PostingFrequencyDistribution.Poisson;

    /// <summary>
    /// Average posts per day. Used as a baseline rate for most distributions.
    /// </summary>
    public double MeanPostsPerDay { get; set; } = 1.0;

    /// <summary>
    /// Burstiness factor (0.0 to 1.0) for bursty models.
    /// </summary>
    public double BurstyFactor { get; set; } = 0.2;
}

public enum PostingFrequencyDistribution
{
    Poisson,
    Bursty,
    Custom
}

public sealed class SessionBehavior
{
    /// <summary>
    /// Average number of sessions per day.
    /// </summary>
    public double MeanSessionsPerDay { get; set; } = 2.0;

    /// <summary>
    /// Mean duration of a session.
    /// </summary>
    public TimeSpan MeanSessionDuration { get; set; } = TimeSpan.FromMinutes(12);

    /// <summary>
    /// Standard deviation of session duration.
    /// </summary>
    public TimeSpan SessionDurationStdDev { get; set; } = TimeSpan.FromMinutes(6);

    /// <summary>
    /// Minimum session duration.
    /// </summary>
    public TimeSpan MinSessionDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum session duration.
    /// </summary>
    public TimeSpan MaxSessionDuration { get; set; } = TimeSpan.FromHours(2);
}
