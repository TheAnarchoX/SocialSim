namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// Sync cursor/checkpoint for a federation source (data-only).
/// </summary>
public sealed class AtProtoSyncState
{
    public Guid Id { get; set; }

    public Guid SourceId { get; set; }

    public string? Did { get; set; }

    public AtProtoSyncStatus Status { get; set; } = AtProtoSyncStatus.Active;

    public string? Cursor { get; set; }

    public long? LastSeq { get; set; }

    public string? LastCommitCid { get; set; }

    public DateTime? LastSuccessAt { get; set; }

    public DateTime? LastAttemptAt { get; set; }

    public DateTime? NextAttemptAt { get; set; }

    public int FailureCount { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum AtProtoSyncStatus
{
    Active,
    Paused,
    Failed
}
