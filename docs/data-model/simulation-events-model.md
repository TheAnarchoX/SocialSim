# Simulation Events Model Specification

This document defines the **event taxonomy**, **metadata**, and **aggregation patterns** for SocialSim simulation events (Phase 1.2). Events are the backbone of observability, replay, and real-time UI updates.

## Design Principles

1. **Event-first**: all simulation actions are represented as events.
2. **Reproducible**: events must be derivable from config + seed + tick.
3. **Traceable**: events must include causal context (which prior event(s) contributed).
4. **Streamable**: events should be compact and JSON-serializable for Redis + SignalR.
5. **Extensible**: new event types should not break old clients.

---

## 1. Event Envelope (Base Metadata)

All events share a common envelope.

### 1.1 Required Metadata

- `id`: UUID
- `eventType`: stable identifier (string)
- `timestampUtc`: wall-clock timestamp when emitted
- `runId`: simulation run identifier
- `tick`: simulation tick when the event occurred

### 1.2 Causality & Correlation

- `causedByEventId`: optional immediate cause
- `causalityChain`: optional list of prior event ids (bounded length)
- `correlationId`: optional id for grouping (e.g., scenario activation)

### 1.3 Uncertainty

- `probability`: optional probability assigned to the chosen branch/action
- `confidence`: optional confidence score for classification/heuristics

### 1.4 Agent State at Event Time

Events may include a compact snapshot:

- `agentState`: JSON object (minimal)
  - Example keys: `sentiment.current`, `opinion.TechOptimism`, `engagementRate`, `timezoneOffset`

> Guideline: avoid large payloads in the event stream; store heavy snapshots in checkpoints.

---

## 2. Taxonomy

### 2.1 Social Events

- `AgentFollowed`
- `AgentUnfollowed`
- `AgentBlocked`
- `AgentUnblocked`
- `AgentMuted`
- `AgentUnmuted`
- `AgentReported` (user/content)
- `AgentMentioned` (mention/tag)
- `DirectMessageSent` (optional; enable per config)

**Reason codes** (examples):
- Follow/unfollow: `FollowBack`, `Recommendation`, `SharedInterest`, `Spam`, `LowQualityContent`, `OpinionDivergence`, `Inactivity`
- Block/mute/report: `Harassment`, `Spam`, `Misinformation`, `NSFW`, `Impersonation`, `Other`

### 2.2 Content Events

- `PostCreated`
- `PostEdited`
- `PostDeleted`
- `PostFlagged`
- `PostModerated` (action taken)
- `PostEngagement`
  - Types: Like | Repost | Quote | Reply
- `ViralThresholdCrossed`
  - Indicates a post/topic passed a configured viral threshold

### 2.3 System Events

- `SimulationStarted`
- `SimulationPaused`
- `SimulationResumed`
- `SimulationStopped`
- `CheckpointCreated`
- `CheckpointRestored`
- `ScenarioTriggered`
- `AgentSpawned`
- `AgentDeactivated`
- `SimulationWarning`
- `SimulationError`

---

## 3. Event Schemas (Examples)

### 3.1 Follow Event

```json
{
  "id": "e0a0a7b0-8a8b-4e8e-9a90-1a0b6b8b7f11",
  "eventType": "AgentFollowed",
  "timestampUtc": "2026-01-01T00:00:05Z",
  "runId": "c4a6b0f5-3a89-4f48-a113-5e3d0b5d3a8e",
  "tick": 20,
  "sourceAgentId": "a7b6d4c2-9c72-4ed8-8a4a-bd117a4c36d2",
  "targetAgentId": "bba8d6f0-0c2d-43c8-9b86-0a3e3ac9d7e1",
  "reason": "Recommendation",
  "probability": 0.31,
  "causedByEventId": "6b2a45f0-5f17-4c5c-8f5c-1a5b1f2ccaaa",
  "agentState": {
    "sentiment.current": 0.18,
    "opinion.TechOptimism": 0.62
  }
}
```

### 3.2 Engagement Event

```json
{
  "id": "1c7f8c7c-c9d5-45f0-bccb-381a6c5ff2e7",
  "eventType": "PostEngagement",
  "timestampUtc": "2026-01-01T00:10:10Z",
  "runId": "c4a6b0f5-3a89-4f48-a113-5e3d0b5d3a8e",
  "tick": 2400,
  "agentId": "a7b6d4c2-9c72-4ed8-8a4a-bd117a4c36d2",
  "postId": "9a1f6a12-2c47-4d4e-8a8d-98c1f2bbce21",
  "type": "Like",
  "probability": 0.54
}
```

---

## 4. Aggregation Patterns

Event aggregation supports dashboards and query efficiency.

### 4.1 Time-Bucket Aggregations

- Posts created per minute
- Engagements per minute by type
- Active agents per minute

### 4.2 Agent-Level Aggregations

- Per-agent posting rate
- Per-agent engagement distribution (like vs reply)
- Per-agent sentiment trend

### 4.3 Topic-Level Aggregations

- Trending topics (top-k per bucket)
- Topic sentiment
- Topic adoption curves

### 4.4 Real-Time Streaming Aggregations

- Redis consumer maintains rolling counters
- PostgreSQL stores sampled snapshots (`simulation_metrics`)

---

## 5. Compatibility Guidelines

- Clients should ignore unknown `eventType` values.
- Additive fields are allowed; avoid breaking changes.
- Prefer stable enums serialized as strings.

---

## 6. Open Questions

1. Should we cap `causalityChain` length (e.g., 32) to bound payload sizes?
2. Do we store events in PostgreSQL long-term, or rely on checkpoints + metrics for persistence?
