using System.Text.Json;

namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// Optional lexicon-like schema registry entry for validating records.
/// </summary>
public sealed class AtProtoLexiconSchema
{
    public Guid Id { get; set; }

    public string Nsid { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public JsonElement Schema { get; set; }

    public string? Sha256 { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
