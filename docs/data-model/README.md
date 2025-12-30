# SocialSim Data Model Documentation

This directory contains the comprehensive data model design for SocialSim's social network.

## Documents

- [ERD Overview](./ERD.md) - Complete entity relationship diagram with visual representations
- [Core Entities](./core-entities.md) - Detailed specifications for Users, Posts, Comments, Media
- [Relationships](./relationships.md) - Social relationships (Follow, Block, Mute, Report)
- [Engagement Models](./engagement-models.md) - Likes, Shares, Quotes, Bookmarks
- [Content Hierarchies](./content-hierarchies.md) - Threads, Conversations, Quote chains
- [Privacy & Visibility](./privacy-visibility.md) - Access control and visibility rules
- [PostgreSQL Schema](./postgresql-schema.sql) - Complete SQL DDL for relational database
- [Neo4j Model](./neo4j-model.md) - Graph database model with Cypher examples

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
