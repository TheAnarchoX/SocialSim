namespace SocialSim.Core.Models;

/// <summary>
/// Content generation and consumption preferences for an agent.
/// </summary>
public sealed class ContentPreferences
{
    /// <summary>
    /// Topic interests with weights (0.0 to 1.0). Keys are simulation-defined.
    /// </summary>
    public Dictionary<string, double> TopicInterests { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Preference weights by media type.
    /// </summary>
    public Dictionary<PreferredMediaType, double> MediaTypeWeights { get; set; } = new();

    /// <summary>
    /// Content length preferences.
    /// </summary>
    public ContentLengthPreference ContentLength { get; set; } = new();

    /// <summary>
    /// Language and tone preferences.
    /// </summary>
    public LanguageTonePreference LanguageTone { get; set; } = new();
}

public enum PreferredMediaType
{
    Text,
    Image,
    Video
}

public sealed class ContentLengthPreference
{
    /// <summary>
    /// Mean desired content length in characters.
    /// </summary>
    public int MeanChars { get; set; } = 180;

    /// <summary>
    /// Standard deviation in characters.
    /// </summary>
    public int StdDevChars { get; set; } = 80;

    /// <summary>
    /// Minimum length in characters.
    /// </summary>
    public int MinChars { get; set; } = 1;

    /// <summary>
    /// Maximum length in characters.
    /// </summary>
    public int MaxChars { get; set; } = 300;
}

public sealed class LanguageTonePreference
{
    /// <summary>
    /// Primary language code (BCP-47-ish, e.g., "en", "en-US").
    /// </summary>
    public string PrimaryLanguage { get; set; } = "en";

    /// <summary>
    /// Additional languages and weights.
    /// </summary>
    public Dictionary<string, double> LanguageWeights { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Tone preferences (0.0 to 1.0), where keys may include: Positive, Neutral, Negative,
    /// Humorous, Formal, Informal, Sarcastic.
    /// </summary>
    public Dictionary<string, double> ToneWeights { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
