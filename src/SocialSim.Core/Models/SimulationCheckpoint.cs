namespace SocialSim.Core.Models;

/// <summary>
/// A checkpoint is a durable snapshot (or diff) of a simulation run at a specific tick.
/// The heavy payload is typically stored externally (blob/object storage) and referenced here.
/// </summary>
public sealed class SimulationCheckpoint
{
    public Guid Id { get; set; }

    public Guid RunId { get; set; }

    public long Tick { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public CheckpointFormat Format { get; set; } = CheckpointFormat.Json;

    /// <summary>
    /// Storage pointer (URI, blob id, or Postgres large-object reference).
    /// </summary>
    public string StatePointer { get; set; } = string.Empty;

    public string? ChecksumSha256 { get; set; }

    public long? SizeBytes { get; set; }

    public Guid? DiffFromCheckpointId { get; set; }

    public string? Notes { get; set; }
}

public enum CheckpointFormat
{
    Json,
    MessagePack,
    Custom
}
