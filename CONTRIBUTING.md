# Contributing to SocialSim

Thank you for your interest in contributing to SocialSim! This document provides guidelines and information for contributors.

## Development Setup

1. Install [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
3. Clone the repository
4. Run `dotnet restore` in the repository root
5. Run `dotnet build` to ensure everything compiles

## Architecture Principles

### Event-Driven Design

The simulation uses an event-driven architecture where:

- Agent actions generate events
- Events are published to Redis for distribution
- Workers and services subscribe to relevant events
- SignalR broadcasts events to connected clients

### Database Strategy

#### PostgreSQL (Relational Data)

- Agent profiles and metadata
- Posts and content
- Simulation configurations
- Use Entity Framework Core for data access

#### Neo4J (Graph Data)

- Social connections (follow graph)
- Network analysis and pathfinding
- Community detection
- Influence propagation
- Use Cypher queries via Neo4j.Driver

#### Redis (Caching & Pub/Sub)

- Event distribution
- Caching frequently accessed data
- Real-time leaderboards
- Session state

## AT Protocol Considerations

SocialSim is designed to potentially integrate with the AT Protocol (ATProto) used by Bluesky. Here's what to consider:

### Current AT Protocol Support

The domain models include fields for AT Protocol concepts:

- **DID (Decentralized Identifier)**: `SocialAgent.DecentralizedId`
- **Handle**: `SocialAgent.Handle` (e.g., user.bsky.social)
- **rkey (Record Key)**: `Post.RecordKey`
- **CID (Content Identifier)**: `Post.ContentId`
- **Protocol Type**: `SocialAgent.ProtocolType` enum

### Future AT Protocol Integration

When implementing full AT Protocol support, consider:

1. **Personal Data Server (PDS)**
   - Each agent could have their own PDS
   - Store agent data in AT Protocol repo structure
   - Implement lexicons for custom record types

2. **AppView**
   - Aggregate data from multiple PDSs
   - Provide feed algorithms
   - Handle content moderation

3. **Feed Generator**
   - Create custom algorithms for content discovery
   - Implement viral event simulation
   - Test trending topic algorithms

4. **Federation**
   - Multiple SocialSim instances could federate
   - Share agents and content across instances
   - Test distributed social network scenarios

5. **Lexicons**
   - Define custom record types for simulation data
   - Simulation scenarios
   - Agent behavior configurations
   - Network analytics data

### AT Protocol Resources

- [AT Protocol Specs](https://atproto.com/specs/atp)
- [Bluesky Developer Docs](https://docs.bsky.app/)
- [AT Protocol GitHub](https://github.com/bluesky-social/atproto)

## Code Style

- Follow standard C# conventions
- Use async/await for I/O operations
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Use dependency injection

## Testing

- Write unit tests for business logic
- Integration tests for database operations
- End-to-end tests for simulation scenarios
- Performance tests for graph queries

## Submitting Changes

1. Create a feature branch
2. Make your changes
3. Write/update tests
4. Update documentation
5. Submit a pull request

## Ideas for Contributions

### Core Features

- [ ] Implement graph algorithms in Neo4J (PageRank, community detection)
- [ ] Add more sophisticated agent behaviors
- [ ] Create viral event scenarios
- [ ] Implement influence propagation models

### AT Protocol

- [ ] Full AT Protocol PDS implementation
- [ ] Federation between multiple instances
- [ ] Custom feed algorithms
- [ ] DID:plc resolver integration

### Visualization

- [ ] Web-based network visualization
- [ ] Real-time dashboard
- [ ] Agent activity timelines
- [ ] Network metrics graphs

### Analysis

- [ ] Network topology analysis
- [ ] Information cascade tracking
- [ ] Echo chamber detection
- [ ] Bot detection algorithms

### Infrastructure

- [ ] Kubernetes deployment configs
- [ ] Performance optimization
- [ ] Horizontal scaling support
- [ ] Monitoring and alerting

## Questions?

Open an issue for:

- Feature requests
- Bug reports
- Architecture discussions
- AT Protocol integration ideas
