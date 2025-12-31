using System.Text.Json;

namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// Repository metadata for a DID (PDS endpoint, head CID, rev, etc.).
/// </summary>
public sealed class AtProtoRepository
{
    public Guid Id { get; set; }

    public string Did { get; set; } = string.Empty;

    public string PdsEndpoint { get; set; } = string.Empty;

    public AtProtoRepositoryStatus Status { get; set; } = AtProtoRepositoryStatus.Active;

    public string? HeadCid { get; set; }

    public string? Rev { get; set; }

    public long? LastCommitSeq { get; set; }

    public DateTime? LastCommitAt { get; set; }

    public JsonElement? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum AtProtoRepositoryStatus
{
    Active,
    Suspended,
    Tombstoned
}
