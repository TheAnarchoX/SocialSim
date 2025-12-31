namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// AT Protocol handle without a leading '@' (e.g., alice.example.com).
///
/// Model-level constraints (not enforced here):
/// - Stored normalized to lowercase.
/// - No whitespace.
/// - Globally unique.
/// </summary>
public sealed class AtProtoHandle
{
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Associated DID (canonical identity).
    /// </summary>
    public string? Did { get; set; }

    public bool IsPrimary { get; set; }

    public AtProtoVerificationState Verification { get; set; } = new();
}
