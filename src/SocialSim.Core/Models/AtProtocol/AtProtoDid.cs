namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// AT Protocol DID (e.g., did:plc:..., did:web:...).
///
/// Model-level constraints (not enforced here):
/// - Stored normalized to lowercase.
/// - No whitespace.
/// </summary>
public sealed class AtProtoDid
{
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Optional parsed DID method (for validation-friendly modeling).
    /// </summary>
    public AtProtoDidMethod? Method { get; set; }

    /// <summary>
    /// Optional parsed identifier portion (method-specific).
    /// </summary>
    public string? Identifier { get; set; }
}

public enum AtProtoDidMethod
{
    Plc,
    Web,
    Other
}
