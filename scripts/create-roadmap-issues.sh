#!/bin/bash

# Script to create GitHub issues from ROADMAP.md with sub-issues
# This script creates parent issues for each phase section and sub-issues for major tasks
# Run this script with: bash scripts/create-roadmap-issues.sh

REPO="TheAnarchoX/SocialSim"

echo "Creating GitHub issues for SocialSim Roadmap..."
echo "Repository: $REPO"
echo ""
echo "This script creates parent issues (epic) and sub-issues for each task."
echo ""

# Phase 1: Data Foundation
echo "=========================================="
echo "Phase 1: Data Foundation"
echo "=========================================="

# Phase 1.1: Social Network Data Model Design
echo "Creating Phase 1.1 issues..."
PHASE_1_1_URL=$(gh issue create \
  --repo "$REPO" \
  --title "[Phase 1.1] Social Network Data Model Design" \
  --label "phase-1,data-model,critical,epic" \
  --body "## Overview
Part of Phase 1: Data Foundation (see ROADMAP.md)

Design comprehensive data model for the social network including entities, relationships, and schemas.

## Milestone
Data model approved and documented

## Sub-Issues
This is a parent issue. Individual tasks are tracked in sub-issues.

## Reference
See ROADMAP.md - Phase 1.1")
PHASE_1_1=$(echo "$PHASE_1_1_URL" | grep -oP '\d+$')

echo "  Created parent issue #${PHASE_1_1}"

gh issue create \
  --repo "$REPO" \
  --title "Design comprehensive ERD for social network" \
  --label "phase-1,data-model,critical" \
  --body "## Parent Issue
#${PHASE_1_1}

## Tasks
- [ ] Define core entities (Users, Posts, Comments, Reactions, Media)
- [ ] Define relationship types (Follow, Block, Mute, Report)
- [ ] Design engagement models (Likes, Shares, Quotes, Bookmarks)
- [ ] Model content hierarchies (Threads, Conversations, Quote chains)
- [ ] Design privacy & visibility rules (Public, Followers-only, Private, Mentions)"
echo "    Created sub-issue: Design comprehensive ERD"

gh issue create \
  --repo "$REPO" \
  --title "Design graph data model for Neo4J" \
  --label "phase-1,data-model,neo4j,critical" \
  --body "## Parent Issue
#${PHASE_1_1}

## Tasks
- [ ] Define node types and properties
- [ ] Define relationship types and weights
- [ ] Design indexes for performance
- [ ] Plan for temporal data (relationship history, follower count over time)"
echo "    Created sub-issue: Design graph data model for Neo4J"

gh issue create \
  --repo "$REPO" \
  --title "Design relational schema for PostgreSQL" \
  --label "phase-1,data-model,postgresql,critical" \
  --body "## Parent Issue
#${PHASE_1_1}

## Tasks
- [ ] Normalize data for consistency
- [ ] Plan denormalization for performance where needed
- [ ] Design audit/history tables
- [ ] Define database constraints and triggers"
echo "    Created sub-issue: Design relational schema for PostgreSQL"

gh issue create \
  --repo "$REPO" \
  --title "Document and validate data model" \
  --label "phase-1,data-model,documentation,critical" \
  --body "## Parent Issue
#${PHASE_1_1}

## Tasks
- [ ] Document data model with examples and use cases
- [ ] Review and validate with stakeholders"
echo "    Created sub-issue: Document and validate data model"

# Phase 1.2: Simulation Data Model Design
echo "Creating Phase 1.2 issues..."
PHASE_1_2_URL=$(gh issue create \
  --repo "$REPO" \
  --title "[Phase 1.2] Simulation Data Model Design" \
  --label "phase-1,data-model,simulation,critical,epic" \
  --body "## Overview
Part of Phase 1: Data Foundation (see ROADMAP.md)

Design comprehensive data model for the simulation including agents, behaviors, and scenarios.

## Milestone
Simulation model approved and documented

## Sub-Issues
This is a parent issue. Individual tasks are tracked in sub-issues.

## Reference
See ROADMAP.md - Phase 1.2")
PHASE_1_2=$(echo "$PHASE_1_2_URL" | grep -oP '\d+$')

echo "  Created parent issue #${PHASE_1_2}"

gh issue create \
  --repo "$REPO" \
  --title "Design agent behavior models" \
  --label "phase-1,data-model,simulation,critical" \
  --body "## Parent Issue
#${PHASE_1_2}

## Tasks
- [ ] Personality traits (Introvert/Extrovert, Positive/Negative sentiment)
- [ ] Activity patterns (Time of day, frequency distributions)
- [ ] Content preferences (Topics, media types, length)
- [ ] Social behavior (Follower strategy, engagement style)
- [ ] Influence susceptibility (How easily swayed by trends/influencers)"
echo "    Created sub-issue: Design agent behavior models"

gh issue create \
  --repo "$REPO" \
  --title "Design simulation state models" \
  --label "phase-1,data-model,simulation,critical" \
  --body "## Parent Issue
#${PHASE_1_2}

## Tasks
- [ ] Simulation configuration (Time scale, number of agents, scenarios)
- [ ] Real-time metrics (Active users, posts/min, engagement rates)
- [ ] Historical snapshots (Periodic state saves)"
echo "    Created sub-issue: Design simulation state models"

gh issue create \
  --repo "$REPO" \
  --title "Design event models" \
  --label "phase-1,data-model,simulation,critical" \
  --body "## Parent Issue
#${PHASE_1_2}

## Tasks
- [ ] Event taxonomy (Social, Content, System events)
- [ ] Event metadata (Timestamp, source, causality chain)
- [ ] Event aggregation patterns"
echo "    Created sub-issue: Design event models"

gh issue create \
  --repo "$REPO" \
  --title "Design scenario & campaign models" \
  --label "phase-1,data-model,simulation,critical" \
  --body "## Parent Issue
#${PHASE_1_2}

## Tasks
- [ ] Viral event configurations (Trending topics, memes)
- [ ] Marketing campaigns (Promoted content, influencer activations)
- [ ] Crisis simulations (Misinformation spread, coordinated attacks)"
echo "    Created sub-issue: Design scenario & campaign models"

# Phase 1.3: AT Protocol Data Model Extension
echo "Creating Phase 1.3 issues..."
PHASE_1_3_URL=$(gh issue create \
  --repo "$REPO" \
  --title "[Phase 1.3] AT Protocol Data Model Extension" \
  --label "phase-1,data-model,at-protocol,critical,epic" \
  --body "## Overview
Part of Phase 1: Data Foundation (see ROADMAP.md)

Extend data models to support AT Protocol (Bluesky) primitives, federation, and custom domain handles.

## Milestone
AT Protocol model extensions documented

## Documentation
See docs/AT_PROTOCOL_CUSTOM_HANDLE.md for custom domain setup

## Sub-Issues
This is a parent issue. Individual tasks are tracked in sub-issues.

## Reference
See ROADMAP.md - Phase 1.3")
PHASE_1_3=$(echo "$PHASE_1_3_URL" | grep -oP '\d+$')

echo "  Created parent issue #${PHASE_1_3}"

gh issue create \
  --repo "$REPO" \
  --title "Map AT Protocol primitives to internal models" \
  --label "phase-1,data-model,at-protocol,critical" \
  --body "## Parent Issue
#${PHASE_1_3}

## Tasks
- [ ] DID (Decentralized Identifiers) integration
- [ ] Handle resolution and verification (DNS TXT and HTTPS well-known)
- [ ] Custom domain handle support (e.g., @theanarchox.net)
- [ ] Repository structure (Collections, Records)
- [ ] Content addressing (CIDs, rkeys)
- [ ] Lexicon definitions for custom record types

## Documentation
See docs/AT_PROTOCOL_CUSTOM_HANDLE.md"
echo "    Created sub-issue: Map AT Protocol primitives to internal models"

gh issue create \
  --repo "$REPO" \
  --title "Design AT Protocol repository layout" \
  --label "phase-1,data-model,at-protocol,critical" \
  --body "## Parent Issue
#${PHASE_1_3}

## Tasks
- [ ] Collection schemas (posts, likes, follows, blocks, etc.)
- [ ] Record versioning and history
- [ ] Blob storage for media"
echo "    Created sub-issue: Design AT Protocol repository layout"

gh issue create \
  --repo "$REPO" \
  --title "Design federation models" \
  --label "phase-1,data-model,at-protocol,critical" \
  --body "## Parent Issue
#${PHASE_1_3}

## Tasks
- [ ] PDS (Personal Data Server) metadata
- [ ] Cross-instance references
- [ ] Synchronization state"
echo "    Created sub-issue: Design federation models"

gh issue create \
  --repo "$REPO" \
  --title "Design custom domain handle configuration" \
  --label "phase-1,data-model,at-protocol,critical" \
  --body "## Parent Issue
#${PHASE_1_3}

## Tasks
- [ ] Domain-to-DID mapping schema
- [ ] Handle generation patterns for simulation agents
- [ ] Multi-domain support for different agent types

## Documentation
See docs/AT_PROTOCOL_CUSTOM_HANDLE.md"
echo "    Created sub-issue: Design custom domain handle configuration"

# Phase 2: Storage & Persistence Layer
echo ""
echo "=========================================="
echo "Phase 2: Storage & Persistence Layer"
echo "=========================================="

# Phase 2.1: Neo4J Repository Layer
echo "Creating Phase 2.1 issues..."
PHASE_2_1_URL=$(gh issue create \
  --repo "$REPO" \
  --title "[Phase 2.1] Neo4J Repository Layer & Custom Query Builder" \
  --label "phase-2,neo4j,infrastructure,critical,epic" \
  --body "## Overview
Part of Phase 2: Storage & Persistence Layer (see ROADMAP.md)

Build custom ORM/query builder for Neo4J with fluent API and repository pattern.

## Milestone
Neo4J persistence layer complete

## Dependencies
- Requires: Phase 1 completion
- Blocks: Phase 3, 4, 5

## Sub-Issues
This is a parent issue. Individual tasks are tracked in sub-issues.

## Reference
See ROADMAP.md - Phase 2.1")
PHASE_2_1=$(echo "$PHASE_2_1_URL" | grep -oP '\d+$')

echo "  Created parent issue #${PHASE_2_1}"

gh issue create \
  --repo "$REPO" \
  --title "Build custom query builder for Neo4J" \
  --label "phase-2,neo4j,infrastructure,critical" \
  --body "## Parent Issue
#${PHASE_2_1}

## Tasks
- [ ] Fluent API for Cypher query construction
- [ ] Type-safe node and relationship builders
- [ ] Support for complex pattern matching
- [ ] Query optimization helpers"
echo "    Created sub-issue: Build custom query builder for Neo4J"

gh issue create \
  --repo "$REPO" \
  --title "Implement generic repository pattern for graph data" \
  --label "phase-2,neo4j,infrastructure,critical" \
  --body "## Parent Issue
#${PHASE_2_1}

## Tasks
- [ ] CRUD operations for nodes
- [ ] Relationship management (create, delete, update weights)
- [ ] Traversal operations (shortest path, degree of separation)
- [ ] Batch operations for performance"
echo "    Created sub-issue: Implement generic repository pattern for graph data"

gh issue create \
  --repo "$REPO" \
  --title "Create specialized Neo4J repositories" \
  --label "phase-2,neo4j,infrastructure,critical" \
  --body "## Parent Issue
#${PHASE_2_1}

## Tasks
- [ ] SocialGraphRepository (follow graph operations)
- [ ] InfluenceRepository (influence calculation and caching)
- [ ] CommunityRepository (community detection)
- [ ] EngagementRepository (content interaction patterns)"
echo "    Created sub-issue: Create specialized Neo4J repositories"

gh issue create \
  --repo "$REPO" \
  --title "Implement caching layer over Neo4J" \
  --label "phase-2,neo4j,redis,infrastructure" \
  --body "## Parent Issue
#${PHASE_2_1}

## Tasks
- [ ] Redis-backed cache for hot paths
- [ ] Cache invalidation strategies
- [ ] Read-through/write-through patterns"
echo "    Created sub-issue: Implement caching layer over Neo4J"

gh issue create \
  --repo "$REPO" \
  --title "Add comprehensive unit tests for Neo4J repositories" \
  --label "phase-2,neo4j,testing" \
  --body "## Parent Issue
#${PHASE_2_1}

## Tasks
- [ ] Unit tests for query builder
- [ ] Unit tests for repositories
- [ ] Integration tests with test database"
echo "    Created sub-issue: Add comprehensive unit tests for Neo4J repositories"

# Phase 2.2: PostgreSQL Repository Layer
echo "Creating Phase 2.2 issues..."
PHASE_2_2_URL=$(gh issue create \
  --repo "$REPO" \
  --title "[Phase 2.2] PostgreSQL Repository Layer (EF Core)" \
  --label "phase-2,postgresql,ef-core,infrastructure,critical,epic" \
  --body "## Overview
Part of Phase 2: Storage & Persistence Layer (see ROADMAP.md)

Implement Entity Framework Core repositories for relational data.

## Milestone
PostgreSQL persistence layer complete

## Dependencies
Requires: Phase 1 completion

## Sub-Issues
This is a parent issue. Individual tasks are tracked in sub-issues.

## Reference
See ROADMAP.md - Phase 2.2")
PHASE_2_2=$(echo "$PHASE_2_2_URL" | grep -oP '\d+$')

echo "  Created parent issue #${PHASE_2_2}"

gh issue create \
  --repo "$REPO" \
  --title "Create Entity Framework Core DbContext" \
  --label "phase-2,postgresql,ef-core,infrastructure,critical" \
  --body "## Parent Issue
#${PHASE_2_2}

## Tasks
- [ ] Configure all entities from data model
- [ ] Set up relationships and navigation properties
- [ ] Configure indexes for performance
- [ ] Add query filters for soft deletes"
echo "    Created sub-issue: Create Entity Framework Core DbContext"

gh issue create \
  --repo "$REPO" \
  --title "Implement Repository pattern for EF Core" \
  --label "phase-2,postgresql,ef-core,infrastructure,critical" \
  --body "## Parent Issue
#${PHASE_2_2}

## Tasks
- [ ] Generic repository for common CRUD
- [ ] Specialized repositories per aggregate root
- [ ] Unit of Work pattern for transactions"
echo "    Created sub-issue: Implement Repository pattern for EF Core"

gh issue create \
  --repo "$REPO" \
  --title "Create EF Core migrations" \
  --label "phase-2,postgresql,ef-core,infrastructure,critical" \
  --body "## Parent Issue
#${PHASE_2_2}

## Tasks
- [ ] Initial database schema
- [ ] Seed data for development
- [ ] Migration scripts for production"
echo "    Created sub-issue: Create EF Core migrations"

gh issue create \
  --repo "$REPO" \
  --title "Implement change tracking and audit" \
  --label "phase-2,postgresql,ef-core,infrastructure" \
  --body "## Parent Issue
#${PHASE_2_2}

## Tasks
- [ ] CreatedAt, UpdatedAt, DeletedAt timestamps
- [ ] Audit log tables
- [ ] Change history tracking"
echo "    Created sub-issue: Implement change tracking and audit"

gh issue create \
  --repo "$REPO" \
  --title "Add database seeding utilities" \
  --label "phase-2,postgresql,ef-core,infrastructure" \
  --body "## Parent Issue
#${PHASE_2_2}

## Tasks
- [ ] Test data generators
- [ ] Realistic sample data sets"
echo "    Created sub-issue: Add database seeding utilities"

# Phase 2.3: Redis Integration
echo "Creating Phase 2.3 issues..."
PHASE_2_3_URL=$(gh issue create \
  --repo "$REPO" \
  --title "[Phase 2.3] Redis Integration" \
  --label "phase-2,redis,infrastructure,epic" \
  --body "## Overview
Part of Phase 2: Storage & Persistence Layer (see ROADMAP.md)

Implement Redis for pub/sub, caching, and rate limiting.

## Milestone
Redis integration complete

## Sub-Issues
This is a parent issue. Individual tasks are tracked in sub-issues.

## Reference
See ROADMAP.md - Phase 2.3")
PHASE_2_3=$(echo "$PHASE_2_3_URL" | grep -oP '\d+$')

echo "  Created parent issue #${PHASE_2_3}"

gh issue create \
  --repo "$REPO" \
  --title "Implement Redis pub/sub for events" \
  --label "phase-2,redis,infrastructure" \
  --body "## Parent Issue
#${PHASE_2_3}

## Tasks
- [ ] Event serialization/deserialization
- [ ] Channel management
- [ ] Subscriber registration"
echo "    Created sub-issue: Implement Redis pub/sub for events"

gh issue create \
  --repo "$REPO" \
  --title "Create caching abstractions" \
  --label "phase-2,redis,infrastructure" \
  --body "## Parent Issue
#${PHASE_2_3}

## Tasks
- [ ] Distributed cache interface
- [ ] Cache-aside pattern implementation
- [ ] Cache key strategies"
echo "    Created sub-issue: Create caching abstractions"

gh issue create \
  --repo "$REPO" \
  --title "Implement rate limiting" \
  --label "phase-2,redis,infrastructure" \
  --body "## Parent Issue
#${PHASE_2_3}

## Tasks
- [ ] Sliding window rate limiters
- [ ] Per-user and per-endpoint limits"
echo "    Created sub-issue: Implement rate limiting"

gh issue create \
  --repo "$REPO" \
  --title "Add session management" \
  --label "phase-2,redis,infrastructure" \
  --body "## Parent Issue
#${PHASE_2_3}

## Tasks
- [ ] Distributed session store
- [ ] Session timeout handling"
echo "    Created sub-issue: Add session management"

# Phase 2.4: Data Access Testing & Performance
echo "Creating Phase 2.4 issues..."
PHASE_2_4_URL=$(gh issue create \
  --repo "$REPO" \
  --title "[Phase 2.4] Data Access Testing & Performance" \
  --label "phase-2,testing,performance,epic" \
  --body "## Overview
Part of Phase 2: Storage & Persistence Layer (see ROADMAP.md)

Comprehensive testing and performance validation of data access layer.

## Milestone
Performance validated and optimized

## Dependencies
Requires: Phase 2.1, 2.2, 2.3 completion

## Sub-Issues
This is a parent issue. Individual tasks are tracked in sub-issues.

## Reference
See ROADMAP.md - Phase 2.4")
PHASE_2_4=$(echo "$PHASE_2_4_URL" | grep -oP '\d+$')

echo "  Created parent issue #${PHASE_2_4}"

gh issue create \
  --repo "$REPO" \
  --title "Create integration tests for all repositories" \
  --label "phase-2,testing" \
  --body "## Parent Issue
#${PHASE_2_4}

## Tasks
- [ ] Neo4J repository tests
- [ ] EF Core repository tests
- [ ] Redis integration tests"
echo "    Created sub-issue: Create integration tests for all repositories"

gh issue create \
  --repo "$REPO" \
  --title "Benchmark query performance" \
  --label "phase-2,performance" \
  --body "## Parent Issue
#${PHASE_2_4}

## Tasks
- [ ] Identify slow queries
- [ ] Add missing indexes
- [ ] Optimize query patterns"
echo "    Created sub-issue: Benchmark query performance"

gh issue create \
  --repo "$REPO" \
  --title "Load testing with realistic data volumes" \
  --label "phase-2,performance" \
  --body "## Parent Issue
#${PHASE_2_4}

## Tasks
- [ ] 1M+ agents
- [ ] 10M+ posts
- [ ] 100M+ relationships"
echo "    Created sub-issue: Load testing with realistic data volumes"

echo ""
echo "=========================================="
echo "✅ Phase 1 and Phase 2 issues created!"
echo "=========================================="
echo ""
echo "Summary:"
echo "- Created parent issues (marked with 'epic' label)"
echo "- Created sub-issues for each major task"
echo "- Sub-issues reference their parent with #issue_number"
echo ""
echo "Note: This script currently covers Phases 1-2."
echo "To add Phases 3-7, extend this script following the same pattern."
echo ""
echo "To view issues: gh issue list --repo $REPO --label epic"
echo "Or visit: https://github.com/$REPO/issues"
