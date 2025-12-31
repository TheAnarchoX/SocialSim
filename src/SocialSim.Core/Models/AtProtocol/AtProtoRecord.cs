using System.Text.Json;

namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// AT Protocol record (collection + rkey) stored as JSON payload with CID.
/// </summary>
public sealed class AtProtoRecord
{
    public Guid Id { get; set; }

    public string Did { get; set; } = string.Empty;

    public string CollectionNsid { get; set; } = string.Empty;

    public string Rkey { get; set; } = string.Empty;

    public string Cid { get; set; } = string.Empty;

    /// <summary>
    /// Typically equals the record's $type (e.g., app.bsky.feed.post).
    /// </summary>
    public string? RecordType { get; set; }

    /// <summary>
    /// Raw record payload. Validation against lexicon-like schemas is out-of-scope in Phase 1.3.
    /// </summary>
    public JsonElement Payload { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
