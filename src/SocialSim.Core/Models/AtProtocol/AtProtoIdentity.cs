using System.Text.Json;

namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// Canonical AT Protocol identity (DID) with handle linkage.
/// </summary>
public sealed class AtProtoIdentity
{
    public Guid Id { get; set; }

    public string Did { get; set; } = string.Empty;

    public string? PrimaryHandle { get; set; }

    /// <summary>
    /// Optional linkage to an internal simulated/local user.
    /// </summary>
    public Guid? UserId { get; set; }

    public bool IsSimulated { get; set; }

    public AtProtoVerificationState Verification { get; set; } = new();

    /// <summary>
    /// Free-form metadata for auditability (e.g., generation algorithm/version/seed inputs, DID document snapshot).
    /// </summary>
    public JsonElement? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
