namespace SocialSim.Core.Models.AtProtocol;

/// <summary>
/// Verification status details for handle/DID and federation sync.
/// Data-only; resolution/verification logic is out of scope.
/// </summary>
public sealed class AtProtoVerificationState
{
    public AtProtoVerificationStatus Status { get; set; } = AtProtoVerificationStatus.Pending;

    public AtProtoVerificationMethod? Method { get; set; }

    public DateTime? VerifiedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? NextCheckAt { get; set; }

    public int AttemptCount { get; set; }
    public int ConsecutiveFailureCount { get; set; }

    public string? LastFailureCode { get; set; }
    public string? LastFailureReason { get; set; }

    /// <summary>
    /// Optional TTL derived from DNS or policy.
    /// </summary>
    public int? TtlSeconds { get; set; }
}

public enum AtProtoVerificationStatus
{
    Pending,
    Verified,
    Expired,
    Failed
}

public enum AtProtoVerificationMethod
{
    DnsTxt,
    WellKnown
}
