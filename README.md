# SocialSim

This repository serves as both some fun and an experiment. It features an advanced and in-depth Social Network Simulation and tools to analyze and affect the network (think, creating hype, events etc). 

## Architecture

SocialSim is built using **.NET Aspire** as the orchestration framework, providing a modern, cloud-native architecture for distributed applications.

### Technology Stack

- **.NET 10.0** - Latest .NET platform
- **.NET Aspire** - Application orchestration and service discovery
- **PostgreSQL** - Relational database for storing agents, posts, and metadata
- **Neo4J** - Graph database for storing and analyzing the social network graph
- **Redis** - Caching and pub/sub for real-time event distribution
- **SignalR** - Real-time communication for live simulation updates
- **Entity Framework Core** - ORM for PostgreSQL

### AT Protocol Support

The simulation is designed with the **AT Protocol** (Bluesky) in mind, featuring:
- Decentralized Identifier (DID) support for agents
- Handle-based addressing
- Content addressing with CIDs and rkeys
- Protocol-agnostic agent architecture supporting traditional and decentralized social protocols

## Project Structure

```
SocialSim/
├── src/
│   ├── SocialSim.AppHost/          # Aspire orchestrator
│   ├── SocialSim.ServiceDefaults/  # Shared service configuration
│   ├── SocialSim.Core/             # Domain models and events
│   ├── SocialSim.Api/              # REST API and SignalR hub
│   └── SocialSim.SimulationWorker/ # Background simulation engine
└── README.md
```

### Components

#### SocialSim.AppHost
The Aspire application host that orchestrates all services and infrastructure:
- Configures PostgreSQL, Neo4J, and Redis containers
- Manages service discovery and connection strings
- Provides the Aspire dashboard for monitoring

#### SocialSim.ServiceDefaults
Shared configuration for all services:
- OpenTelemetry for distributed tracing
- Health checks
- Service discovery
- Resilience patterns

#### SocialSim.Core
Domain layer with core models:
- `SocialAgent` - Represents users/agents in the simulation with behavioral traits
- `SocialConnection` - Graph relationships between agents
- `Post` - Content created by agents
- `SimulationEvents` - Event-driven architecture for simulation actions

#### SocialSim.Api
ASP.NET Core Web API providing:
- REST endpoints for querying simulation state
- SignalR hub for real-time updates
- PostgreSQL for data persistence
- Neo4J integration for graph queries

#### SocialSim.SimulationWorker
Background worker service that:
- Runs the simulation engine
- Generates events based on agent behaviors
- Updates the social graph in Neo4J
- Publishes events via Redis

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started) (for running databases)

### Running the Application

1. Clone the repository:
   ```bash
   git clone https://github.com/TheAnarchoX/SocialSim.git
   cd SocialSim
   ```

2. Run the Aspire AppHost:
   ```bash
   dotnet run --project src/SocialSim.AppHost
   ```

3. The Aspire dashboard will open automatically, showing:
   - API service status
   - Simulation worker status
   - PostgreSQL, Neo4J, and Redis containers
   - Logs and traces

4. Access the API at `https://localhost:xxxx` (port shown in Aspire dashboard)

5. Connect to SignalR hub at `https://localhost:xxxx/simulationHub` for real-time updates

### Manual Infrastructure Setup (Alternative)

If you prefer to run infrastructure separately:

```bash
# PostgreSQL
docker run -d --name socialsim-postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres

# Neo4J
docker run -d --name socialsim-neo4j -e NEO4J_AUTH=neo4j/password123 -p 7474:7474 -p 7687:7687 neo4j

# Redis
docker run -d --name socialsim-redis -p 6379:6379 redis
```

Then run individual projects:
```bash
dotnet run --project src/SocialSim.Api
dotnet run --project src/SocialSim.SimulationWorker
```

## Simulation Features

### Agent Behaviors
Agents have configurable behavioral traits:
- **Posting Frequency** - How often they create content
- **Engagement Rate** - Likelihood to interact with posts
- **Influence Score** - Impact on other agents
- **Interests** - Topics that drive content generation

### Event-Driven Simulation
The simulation generates events:
- `PostCreatedEvent` - Agent creates new content
- `AgentFollowedEvent` - Agent follows another
- `PostEngagementEvent` - Agent likes/reposts/replies
- `SimulationScenarioEvent` - Trigger viral events, trending topics

### Real-Time Updates
Subscribe to the SignalR hub to receive live simulation events and visualize the network evolving in real-time.

## Development

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

## Future Enhancements

- Full AT Protocol integration (PDS, AppView, Feed Generator)
- Advanced network analysis algorithms
- Machine learning for agent behavior
- Web UI for visualization
- Scenario editor for creating custom simulations
- Federation support across multiple instances

## License

MIT License - see LICENSE file for details
