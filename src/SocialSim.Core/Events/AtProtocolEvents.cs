using System.Text.Json;
using SocialSim.Core.Models.AtProtocol;

namespace SocialSim.Core.Events;

public class HandleVerificationRequestedEvent : SimulationEvent
{
    public string Handle { get; set; } = string.Empty;
    public string? Did { get; set; }
    public AtProtoVerificationMethod? Method { get; set; }

    public HandleVerificationRequestedEvent()
    {
        EventType = nameof(HandleVerificationRequestedEvent);
    }
}

public class HandleVerificationCompletedEvent : SimulationEvent
{
    public string Handle { get; set; } = string.Empty;
    public string Did { get; set; } = string.Empty;
    public AtProtoVerificationMethod Method { get; set; }
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;

    public HandleVerificationCompletedEvent()
    {
        EventType = nameof(HandleVerificationCompletedEvent);
    }
}

public class HandleVerificationFailedEvent : SimulationEvent
{
    public string Handle { get; set; } = string.Empty;
    public string? Did { get; set; }
    public AtProtoVerificationMethod? Method { get; set; }

    public string? FailureCode { get; set; }
    public string? FailureReason { get; set; }

    public int AttemptCount { get; set; }
    public DateTime? NextCheckAt { get; set; }

    public HandleVerificationFailedEvent()
    {
        EventType = nameof(HandleVerificationFailedEvent);
    }
}

public class IdentityLinkedEvent : SimulationEvent
{
    public Guid UserId { get; set; }
    public string Did { get; set; } = string.Empty;
    public string? PrimaryHandle { get; set; }

    public IdentityLinkedEvent()
    {
        EventType = nameof(IdentityLinkedEvent);
    }
}

public class IdentityUnlinkedEvent : SimulationEvent
{
    public Guid UserId { get; set; }
    public string Did { get; set; } = string.Empty;

    public IdentityUnlinkedEvent()
    {
        EventType = nameof(IdentityUnlinkedEvent);
    }
}

public class RecordCreatedEvent : SimulationEvent
{
    public string Did { get; set; } = string.Empty;
    public string CollectionNsid { get; set; } = string.Empty;
    public string Rkey { get; set; } = string.Empty;
    public string Cid { get; set; } = string.Empty;
    public string? RecordType { get; set; }
    public JsonElement? Payload { get; set; }

    public RecordCreatedEvent()
    {
        EventType = nameof(RecordCreatedEvent);
    }
}

public class RecordUpdatedEvent : SimulationEvent
{
    public string Did { get; set; } = string.Empty;
    public string CollectionNsid { get; set; } = string.Empty;
    public string Rkey { get; set; } = string.Empty;
    public string NewCid { get; set; } = string.Empty;
    public string? PreviousCid { get; set; }
    public string? RecordType { get; set; }
    public JsonElement? Payload { get; set; }

    public RecordUpdatedEvent()
    {
        EventType = nameof(RecordUpdatedEvent);
    }
}

public class RecordDeletedEvent : SimulationEvent
{
    public string Did { get; set; } = string.Empty;
    public string CollectionNsid { get; set; } = string.Empty;
    public string Rkey { get; set; } = string.Empty;
    public string? PreviousCid { get; set; }
    public string? Reason { get; set; }

    public RecordDeletedEvent()
    {
        EventType = nameof(RecordDeletedEvent);
    }
}

public class SyncCheckpointAdvancedEvent : SimulationEvent
{
    public Guid SourceId { get; set; }
    public string? Did { get; set; }

    public string? PreviousCursor { get; set; }
    public string? NewCursor { get; set; }

    public long? PreviousSeq { get; set; }
    public long? NewSeq { get; set; }

    public SyncCheckpointAdvancedEvent()
    {
        EventType = nameof(SyncCheckpointAdvancedEvent);
    }
}

public class FederationSourceAddedEvent : SimulationEvent
{
    public Guid SourceId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string ServiceEndpoint { get; set; } = string.Empty;

    public FederationSourceAddedEvent()
    {
        EventType = nameof(FederationSourceAddedEvent);
    }
}

public class FederationSourceRemovedEvent : SimulationEvent
{
    public Guid SourceId { get; set; }
    public string? Reason { get; set; }

    public FederationSourceRemovedEvent()
    {
        EventType = nameof(FederationSourceRemovedEvent);
    }
}
