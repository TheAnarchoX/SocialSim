# SocialSim Data Model Documentation

This directory contains the comprehensive data model design for SocialSim's social network.

## Documents

### Core Design
- [ERD Overview](./ERD.md) - Complete entity relationship diagram with visual representations
- [Core Entities](./core-entities.md) - Detailed specifications for Users, Posts, Comments, Media
- [Relationships](./relationships.md) - Social relationships (Follow, Block, Mute, Report)
- [Engagement Models](./engagement-models.md) - Likes, Shares, Quotes, Bookmarks
- [Content Hierarchies](./content-hierarchies.md) - Threads, Conversations, Quote chains
- [Privacy & Visibility](./privacy-visibility.md) - Access control and visibility rules

### Simulation (Phase 1.2)
- [Agent Behavior Model](./agent-behavior-model.md) - Personality, activity patterns, content preferences, influence susceptibility
- [Simulation State Model](./simulation-state-model.md) - Configuration, run state, metrics, checkpoints
- [Simulation Events Model](./simulation-events-model.md) - Event taxonomy, metadata, and aggregation patterns
- [Scenarios & Campaigns Model](./scenarios-campaigns-model.md) - Viral events, marketing campaigns, crisis simulations, optional DSL

### AT Protocol (Phase 1.3)
- [AT Protocol Primitives](./at-protocol-primitives.md) - DID/handle/repo/record/rkey/CID mapping and schema evolution strategy
- [Handle Resolution & Verification](./at-protocol-handle-resolution.md) - DNS/HTTPS sources, verification state machine, caching fields
- [Federation & Sync State](./at-protocol-federation-model.md) - federation sources, cursors/checkpoints, commit log (data-only)

### Database Schemas
- [PostgreSQL Schema](./postgresql-schema.sql) - Complete SQL DDL for relational database
- [Neo4j Model](./neo4j-model.md) - Graph database model with Cypher examples and temporal data strategy
- [Neo4j Cypher Query Builder](./neo4j-cypher-query-builder.md) - Fluent API for constructing Cypher queries + parameters

### Performance & Validation
- [Neo4j Performance Validation](./neo4j-performance-validation.md) - Query validation, graph algorithms, benchmarking
- [PostgreSQL Performance Validation](./postgresql-performance-validation.md) - Index strategy, sample queries, constraint testing

### Project Management
- [Migration Plan](./migration-plan.md) - Database migration and deployment strategy
- [Sample Data](./sample-data.md) - Sample data for testing and development
- [Stakeholder Review](./STAKEHOLDER_REVIEW.md) - **Phase 1.1 completion summary and sign-off** ✅ APPROVED
- [Data Governance](./DATA_GOVERNANCE.md) - Data retention, compliance, and privacy policies (GDPR/CCPA)

## Key Design Principles

1. **Scalability First**: Designed to handle millions of users and billions of interactions
2. **Privacy by Design**: Built-in privacy controls at the entity level
3. **Temporal Awareness**: Track historical data and changes over time
4. **AT Protocol Ready**: Compatible with decentralized identity and content addressing
5. **Polyglot Persistence**: PostgreSQL for relational data, Neo4j for social graph
6. **Event-Driven**: All state changes emit events for real-time processing

## Quick Reference

### Entity Overview

| Entity | Storage | Purpose |
|--------|---------|---------|
| User (SocialAgent) | PostgreSQL + Neo4j | User profiles, authentication, metadata |
| Post | PostgreSQL | Content (text, media references) |
| Comment | PostgreSQL | Replies to posts |
| Media | PostgreSQL | Image/video metadata, blob references |
| Follow | Neo4j (primary), PostgreSQL (cache) | Follower/following relationships |
| Engagement | PostgreSQL | Likes, shares, bookmarks |
| Thread | PostgreSQL | Conversation grouping |

### Relationship Types (Graph)

- **FOLLOWS** - One user follows another
- **BLOCKS** - User blocks another user
- **MUTES** - User mutes another user
- **REPORTS** - User reports another user or content
- **MENTIONS** - User mentioned in content
- **QUOTED** - Post quotes another post
- **REPLIED_TO** - Comment/post replies to another

## Data Flow

```
User Action → Event → [PostgreSQL Write, Neo4j Write] → Cache Invalidation → Event Publish
                                                                            ↓
                                                                      SignalR Broadcast
```
