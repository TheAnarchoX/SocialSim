# SocialSim Copilot Instructions

## Architecture Overview

SocialSim is a **.NET Aspire** distributed application simulating social networks with AT Protocol (Bluesky) support.

### Project Structure

| Project | Purpose |
|---------|---------|
| `SocialSim.AppHost` | Aspire orchestrator - configures PostgreSQL, Neo4j, Redis containers and service discovery |
| `SocialSim.Core` | Domain models (`SocialAgent`, `Post`, `SocialConnection`) and events - shared across all services |
| `SocialSim.Api` | ASP.NET Core API with SignalR hub (`/simulationHub`) for real-time updates |
| `SocialSim.SimulationWorker` | Background worker running simulation ticks every 5 seconds |
| `SocialSim.ServiceDefaults` | Shared Aspire configuration (OpenTelemetry, health checks, resilience) |

### Data Flow

```
SimulationWorker → generates events → Redis pub/sub → API → SignalR → Clients
                                    ↓
                         PostgreSQL (agents, posts) + Neo4j (social graph)
```

## Development Commands

```bash
# Run the full stack (recommended)
dotnet run --project src/SocialSim.AppHost

# Build all projects
dotnet build SocialSim.sln

# Alternative: Docker infrastructure only (requires .env file from .env.example)
docker-compose up -d
```

## Key Patterns

### Event-Driven Architecture

All simulation actions use events in `SocialSim.Core/Events/SimulationEvents.cs`:
- `PostCreatedEvent`, `AgentFollowedEvent`, `PostEngagementEvent`
- Events inherit from `SimulationEvent` base class with `Id`, `Timestamp`, `EventType`

### Database Strategy (Polyglot Persistence)

- **PostgreSQL + EF Core**: Agents, posts, metadata (`SocialSimDbContext`)
- **Neo4j + Cypher**: Social graph, connections, network analysis (raw driver, no ORM)
- **Redis**: Event distribution, caching, pub/sub

### AT Protocol Integration

Domain models include AT Protocol fields for Bluesky compatibility:
- `SocialAgent`: `DecentralizedId` (DID), `Handle`, `ProtocolType`
- `Post`: `RecordKey` (rkey), `ContentId` (CID)
- Configure via `ATProtocolOptions` in `appsettings.json` under `ATProtocol` section

### SignalR Hub

`SimulationHub` at `/simulationHub` broadcasts events to clients:
- `BroadcastEvent(SimulationEvent)` - send to all clients
- `SubscribeToAgent(agentId)` - join agent-specific group

## Conventions

- Use `AgentBehavior` class for behavioral traits (posting frequency, engagement rate, influence)
- Connection types: `Follow`, `Friend`, `Block`, `Mute` (see `ConnectionType` enum)
- All agents use custom domain handles via `ATProtocolOptions.HandleFormat`

### Documentation Hygiene (Always)

- When adding or renaming model docs under `docs/` (especially `docs/data-model/`), also update the relevant index files so the change is reflected:
    - `docs/data-model/README.md` (primary index)
    - Any phase milestone docs (e.g., `docs/data-model/STAKEHOLDER_REVIEW.md`) if applicable
- When adding new persisted entities/tables, keep the documentation trio consistent:
    - C# model classes in `src/SocialSim.Core/Models/`
    - Schema docs in `docs/data-model/postgresql-schema.sql`
    - The `docs/data-model/README.md` link list

## Configuration

Neo4j password must be set in `appsettings.Development.json`:
```json
{ "Neo4j": { "Password": "your-password", "Username": "neo4j" } }
```

## Project Status

Currently in **Phase 1: Data Foundation** - core models exist but repositories and full simulation logic are TODO. See `ROADMAP.md` for detailed phases.
