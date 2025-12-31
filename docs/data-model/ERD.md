# Entity Relationship Diagram (ERD)

This document provides the complete entity relationship diagram for SocialSim's social network data model.

## Overview

SocialSim uses a **polyglot persistence** strategy:
- **PostgreSQL**: Relational data (users, posts, engagement)
- **Neo4j**: Graph data (social relationships, network analysis)
- **Redis**: Caching and real-time data (counters, pub/sub)

## ERD Legend

```
┌─────────────┐
│   Entity    │  = Database Table/Collection
├─────────────┤
│ • Field     │  = Attribute
│ • Field     │
└─────────────┘

────────────────  = One-to-Many Relationship
═══════════════   = Many-to-Many Relationship
- - - - - - - -   = Optional Relationship
```

---

## Core Entities ERD

### User and Profile System

```
┌──────────────────────────────────────────────────┐
│                   Users (SocialAgent)             │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • DecentralizedId (DID, UNIQUE)                   │
│ • Handle (VARCHAR, UNIQUE)                        │
│ • DisplayName (VARCHAR)                           │
│ • Bio (TEXT)                                      │
│ • AvatarUrl (VARCHAR)                             │
│ • BannerUrl (VARCHAR)                             │
│ • Email (VARCHAR, UNIQUE)                         │
│ • PasswordHash (VARCHAR)                          │
│ • IsPrivate (BOOLEAN)                             │
│ • IsVerified (BOOLEAN)                            │
│ • IsSimulated (BOOLEAN)                           │
│ • FollowerCount (INTEGER)                         │
│ • FollowingCount (INTEGER)                        │
│ • PostCount (INTEGER)                             │
│ • CreatedAt (TIMESTAMP)                           │
│ • UpdatedAt (TIMESTAMP)                           │
│ • LastActiveAt (TIMESTAMP)                        │
└──────────────────────────────────────────────────┘
         │
         │ 1:N
         │
         ▼
┌──────────────────────────────────────────────────┐
│              UserSettings                         │
├──────────────────────────────────────────────────┤
│ • UserId (UUID, PK, FK)                           │
│ • DefaultPostVisibility (ENUM)                    │
│ • AllowFollowRequests (BOOLEAN)                   │
│ • AllowMentions (ENUM)                            │
│ • AllowDirectMessages (ENUM)                      │
│ • ShowFollowers (BOOLEAN)                         │
│ • ShowFollowing (BOOLEAN)                         │
│ • UpdatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
```

### Content System

```
┌──────────────────────────────────────────────────┐
│                      Posts                        │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • AuthorId (UUID, FK → Users.Id)                  │
│ • RecordKey (VARCHAR) [AT Protocol]               │
│ • ContentId (VARCHAR) [AT Protocol CID]           │
│ • Content (TEXT)                                  │
│ • ContentType (ENUM)                              │
│ • Language (VARCHAR)                              │
│ • Visibility (ENUM)                               │
│ • IsReply (BOOLEAN)                               │
│ • ReplyToPostId (UUID, FK → Posts.Id)             │
│ • ThreadId (UUID, FK → Threads.Id)                │
│ • RootPostId (UUID, FK → Posts.Id)                │
│ • QuotedPostId (UUID, FK → Posts.Id)              │
│ • RepostOfPostId (UUID, FK → Posts.Id)            │
│ • LikeCount (INTEGER)                             │
│ • RepostCount (INTEGER)                           │
│ • QuoteCount (INTEGER)                            │
│ • ReplyCount (INTEGER)                            │
│ • ViewCount (INTEGER)                             │
│ • EngagementScore (FLOAT)                         │
│ • IsSensitive (BOOLEAN)                           │
│ • ContentWarnings (VARCHAR[])                     │
│ • CreatedAt (TIMESTAMP)                           │
│ • UpdatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
         │
         │ N:M (through PostMedia)
         │
         ▼
┌──────────────────────────────────────────────────┐
│                     Media                         │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • UploadedById (UUID, FK → Users.Id)              │
│ • BlobId (VARCHAR) [Storage reference]            │
│ • MediaType (ENUM)                                │
│ • MimeType (VARCHAR)                              │
│ • FileName (VARCHAR)                              │
│ • FileSizeBytes (BIGINT)                          │
│ • Width (INTEGER)                                 │
│ • Height (INTEGER)                                │
│ • StorageUrl (VARCHAR)                            │
│ • ThumbnailUrl (VARCHAR)                          │
│ • AltText (TEXT)                                  │
│ • IsProcessed (BOOLEAN)                           │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│                   PostMedia                       │
│                (Junction Table)                   │
├──────────────────────────────────────────────────┤
│ • PostId (UUID, PK, FK → Posts.Id)                │
│ • MediaId (UUID, PK, FK → Media.Id)               │
│ • DisplayOrder (INTEGER)                          │
└──────────────────────────────────────────────────┘
```

### Thread System

```
┌──────────────────────────────────────────────────┐
│                    Threads                        │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • RootPostId (UUID, FK → Posts.Id)                │
│ • Title (VARCHAR)                                 │
│ • ParticipantCount (INTEGER)                      │
│ • PostCount (INTEGER)                             │
│ • ViewCount (INTEGER)                             │
│ • LastActivityAt (TIMESTAMP)                      │
│ • IsLocked (BOOLEAN)                              │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
         │
         │ 1:N
         │
         ▼
┌──────────────────────────────────────────────────┐
│              ThreadParticipants                   │
├──────────────────────────────────────────────────┤
│ • ThreadId (UUID, PK, FK → Threads.Id)            │
│ • UserId (UUID, PK, FK → Users.Id)                │
│ • PostCount (INTEGER)                             │
│ • FirstPostAt (TIMESTAMP)                         │
│ • LastPostAt (TIMESTAMP)                          │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│            ThreadSubscriptions                    │
├──────────────────────────────────────────────────┤
│ • ThreadId (UUID, PK, FK → Threads.Id)            │
│ • UserId (UUID, PK, FK → Users.Id)                │
│ • NotificationLevel (ENUM)                        │
│ • IsMuted (BOOLEAN)                               │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
```

---

## Social Relationships ERD

### Follow Graph

```
┌──────────────────────────────────────────────────┐
│                    Follows                        │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • FollowerId (UUID, FK → Users.Id)                │
│ • FollowingId (UUID, FK → Users.Id)               │
│ • Status (ENUM: Active, Pending, Rejected)        │
│ • IsMuted (BOOLEAN)                               │
│ • NotificationsEnabled (BOOLEAN)                  │
│ • CreatedAt (TIMESTAMP)                           │
│ • UpdatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
         │                                  │
         │ FK: FollowerId                   │ FK: FollowingId
         │                                  │
         ▼                                  ▼
    ┌─────────┐                        ┌─────────┐
    │  Users  │                        │  Users  │
    └─────────┘                        └─────────┘

Neo4j Representation:
    (User)-[:FOLLOWS {strength, createdAt}]->(User)
```

### Block, Mute, Report

```
┌──────────────────────────────────────────────────┐
│                    Blocks                         │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • BlockerId (UUID, FK → Users.Id)                 │
│ • BlockedId (UUID, FK → Users.Id)                 │
│ • Reason (ENUM)                                   │
│ • Notes (TEXT)                                    │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│                     Mutes                         │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • MuterId (UUID, FK → Users.Id)                   │
│ • MutedId (UUID, FK → Users.Id)                   │
│ • MuteType (ENUM: All, Replies, Reposts)          │
│ • ExpiresAt (TIMESTAMP, NULLABLE)                 │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│                    Reports                        │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • ReporterId (UUID, FK → Users.Id)                │
│ • ReportedUserId (UUID, FK → Users.Id, NULLABLE)  │
│ • ReportedPostId (UUID, FK → Posts.Id, NULLABLE)  │
│ • ReportType (ENUM)                               │
│ • Reason (ENUM)                                   │
│ • Description (TEXT)                              │
│ • Status (ENUM: Pending, Resolved, Dismissed)     │
│ • ResolvedById (UUID, FK → Users.Id, NULLABLE)    │
│ • ResolvedAt (TIMESTAMP)                          │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
```

---

## Engagement System ERD

### Likes, Reposts, Quotes, Bookmarks

```
┌──────────────────────────────────────────────────┐
│                     Likes                         │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • UserId (UUID, FK → Users.Id)                    │
│ • PostId (UUID, FK → Posts.Id)                    │
│ • ReactionType (ENUM: Like, Love, Laugh, etc.)    │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│                   Reposts                         │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • UserId (UUID, FK → Users.Id)                    │
│ • OriginalPostId (UUID, FK → Posts.Id)            │
│ • RepostPostId (UUID, FK → Posts.Id)              │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│                    Quotes                         │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • UserId (UUID, FK → Users.Id)                    │
│ • QuotedPostId (UUID, FK → Posts.Id)              │
│ • QuotePostId (UUID, FK → Posts.Id)               │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│                  Bookmarks                        │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • UserId (UUID, FK → Users.Id)                    │
│ • PostId (UUID, FK → Posts.Id)                    │
│ • FolderId (UUID, FK → BookmarkFolders.Id)        │
│ • Notes (TEXT)                                    │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
         │
         │ N:1 (optional)
         │
         ▼
┌──────────────────────────────────────────────────┐
│              BookmarkFolders                      │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • UserId (UUID, FK → Users.Id)                    │
│ • Name (VARCHAR)                                  │
│ • Description (TEXT)                              │
│ • Color (VARCHAR)                                 │
│ • SortOrder (INTEGER)                             │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│                  Mentions                         │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • PostId (UUID, FK → Posts.Id)                    │
│ • MentionerId (UUID, FK → Users.Id)               │
│ • MentionedId (UUID, FK → Users.Id)               │
│ • StartIndex (INTEGER)                            │
│ • EndIndex (INTEGER)                              │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
```

---

## Privacy & Circles ERD

```
┌──────────────────────────────────────────────────┐
│                   Circles                         │
│              (Close Friends Lists)                │
├──────────────────────────────────────────────────┤
│ • Id (UUID, PK)                                   │
│ • OwnerId (UUID, FK → Users.Id)                   │
│ • Name (VARCHAR)                                  │
│ • Description (TEXT)                              │
│ • Color (VARCHAR)                                 │
│ • MemberCount (INTEGER)                           │
│ • CreatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
         │
         │ 1:N
         │
         ▼
┌──────────────────────────────────────────────────┐
│              CircleMembers                        │
├──────────────────────────────────────────────────┤
│ • CircleId (UUID, PK, FK → Circles.Id)            │
│ • MemberId (UUID, PK, FK → Users.Id)              │
│ • AddedAt (TIMESTAMP)                             │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│          UserContentFilters                       │
├──────────────────────────────────────────────────┤
│ • UserId (UUID, PK, FK → Users.Id)                │
│ • HideNSFW (BOOLEAN)                              │
│ • HideViolence (BOOLEAN)                          │
│ • HideSpoilers (BOOLEAN)                          │
│ • BlurSensitiveMedia (BOOLEAN)                    │
│ • UpdatedAt (TIMESTAMP)                           │
└──────────────────────────────────────────────────┘
```

---

## Complete Relationship Map

### PostgreSQL Full ERD

```
┌─────────┐
│  Users  │────────┐
└─────────┘        │
     │             │
     │ 1:N         │ 1:N
     │             │
     ▼             ▼
┌─────────┐   ┌─────────────┐
│  Posts  │   │ UserSettings│
└─────────┘   └─────────────┘
     │
     │ N:M
     │
     ▼
┌─────────┐
│  Media  │
└─────────┘

Users ──1:N──> Posts (AuthorId)
Users ──1:N──> Follows (FollowerId, FollowingId)
Users ──1:N──> Blocks (BlockerId, BlockedId)
Users ──1:N──> Mutes (MuterId, MutedId)
Users ──1:N──> Likes (UserId)
Users ──1:N──> Bookmarks (UserId)
Users ──1:N──> Circles (OwnerId)
Users ──N:M──> Circles (through CircleMembers)
Posts ──1:N──> Posts (ReplyToPostId, self-ref)
Posts ──1:N──> Posts (QuotedPostId, self-ref)
Posts ──1:N──> Likes (PostId)
Posts ──1:N──> Reposts (OriginalPostId)
Posts ──1:N──> Mentions (PostId)
Posts ──1:N──> Threads (RootPostId)
Posts ──N:M──> Media (through PostMedia)
Threads ──1:N──> Posts (ThreadId)
Threads ──N:M──> Users (through ThreadParticipants)
```

### Neo4j Graph ERD

```
Nodes:
  (:User {id, handle, followerCount, influenceScore})
  (:Post {id, contentId, engagementScore})
  (:Thread {id, participantCount})

Relationships:
  (:User)-[:FOLLOWS {strength, createdAt}]->(:User)
  (:User)-[:BLOCKS {reason, createdAt}]->(:User)
  (:User)-[:MUTES {muteType, expiresAt}]->(:User)
  (:User)-[:POSTS {createdAt}]->(:Post)
  (:User)-[:LIKES {reactionType, createdAt}]->(:Post)
  (:User)-[:REPOSTS {createdAt}]->(:Post)
  (:Post)-[:QUOTES {createdAt}]->(:Post)
  (:Post)-[:REPLIES_TO {createdAt}]->(:Post)
  (:Post)-[:BELONGS_TO_THREAD]->(:Thread)
  (:User)-[:PARTICIPATES_IN {postCount}]->(:Thread)
  (:Post)-[:MENTIONS {startIndex, endIndex}]->(:User)
```

---

## Data Model Statistics

### Estimated Table Sizes (for 1M users)

| Table | Estimated Rows | Storage Size | Growth Rate |
|-------|----------------|--------------|-------------|
| Users | 1,000,000 | 500 MB | Low (user growth) |
| Posts | 50,000,000 | 25 GB | High (daily content) |
| Follows | 10,000,000 | 800 MB | Medium (network growth) |
| Likes | 200,000,000 | 10 GB | Very High (engagement) |
| Media | 5,000,000 | 2 GB (metadata only) | Medium |
| Threads | 10,000,000 | 500 MB | Medium |
| Mentions | 20,000,000 | 1 GB | High |
| Blocks | 500,000 | 50 MB | Low |
| Mutes | 1,000,000 | 80 MB | Low |
| Reposts | 10,000,000 | 800 MB | Medium |
| Bookmarks | 15,000,000 | 1 GB | Medium |

### Index Storage Overhead

Indexes add approximately **40-60%** storage overhead to base table sizes.

---

## Cardinality Relationships

```
User : Posts          = 1 : 50        (average 50 posts per user)
User : Followers      = 1 : 10        (average 10 followers)
User : Following      = 1 : 10        (average following 10)
Post : Likes          = 1 : 4         (average 4 likes per post)
Post : Replies        = 1 : 2         (average 2 replies)
Thread : Posts        = 1 : 5         (average 5 posts per thread)
Thread : Participants = 1 : 3         (average 3 participants)
User : Bookmarks      = 1 : 15        (power users bookmark heavily)
```

---

## Normalization & Denormalization

### Normalized Data (PostgreSQL)

- Users, Posts, Media: **3NF** (third normal form)
- No redundant data in core entities
- Counts calculated via aggregation

### Denormalized Data (for Performance)

Cached counts on entities:
- `User.FollowerCount`, `User.FollowingCount`, `User.PostCount`
- `Post.LikeCount`, `Post.RepostCount`, `Post.ReplyCount`
- `Thread.ParticipantCount`, `Thread.PostCount`

These are **eventually consistent** and updated via:
1. Immediate increment in Redis
2. Periodic batch sync to PostgreSQL (every 5 min)
3. Nightly reconciliation from source of truth

---

## Data Integrity Constraints

### Foreign Key Constraints

All FK relationships enforced with:
- `ON DELETE CASCADE` for dependent entities (e.g., Likes when Post deleted)
- `ON DELETE SET NULL` for optional references
- `ON DELETE RESTRICT` for critical references (prevent orphans)

### Check Constraints

```sql
-- User constraints
ALTER TABLE Users ADD CONSTRAINT chk_follower_count 
    CHECK (FollowerCount >= 0);

-- Post constraints
ALTER TABLE Posts ADD CONSTRAINT chk_visibility 
    CHECK (Visibility IN ('Public', 'FollowersOnly', 'Private', 'Mentions', 'CircleOnly'));

-- Reply constraints
ALTER TABLE Posts ADD CONSTRAINT chk_reply_consistency 
    CHECK ((IsReply = false AND ReplyToPostId IS NULL) OR 
           (IsReply = true AND ReplyToPostId IS NOT NULL));
```

### Unique Constraints

```sql
-- Prevent duplicate follows
ALTER TABLE Follows ADD CONSTRAINT uq_follow 
    UNIQUE (FollowerId, FollowingId);

-- Prevent duplicate likes
ALTER TABLE Likes ADD CONSTRAINT uq_like 
    UNIQUE (UserId, PostId);

-- Unique handles and DIDs
ALTER TABLE Users ADD CONSTRAINT uq_handle UNIQUE (Handle);
ALTER TABLE Users ADD CONSTRAINT uq_did UNIQUE (DecentralizedId);
```

---

## Composite Keys

```sql
-- Many-to-many junction tables
PRIMARY KEY (PostId, MediaId)         -- PostMedia
PRIMARY KEY (ThreadId, UserId)        -- ThreadParticipants
PRIMARY KEY (CircleId, MemberId)      -- CircleMembers
PRIMARY KEY (UserId, LanguageCode)    -- UserLanguagePreferences
```

---

## Next Steps

1. **PostgreSQL Schema**: See [postgresql-schema.sql](./postgresql-schema.sql) for complete DDL
2. **Neo4j Model**: See [neo4j-model.md](./neo4j-model.md) for graph schema and Cypher examples
3. **Sample Data**: See [sample-data.md](./sample-data.md) for realistic test datasets
4. **Migration Plan**: See [migration-plan.md](./migration-plan.md) for deployment strategy

---

## Visual ERD Tools

This ERD can be visualized using:

1. **Mermaid.js** (in Markdown):
   - See [erd-mermaid.md](./erd-mermaid.md) for interactive diagrams
   
2. **dbdiagram.io**:
   - Import [postgresql-schema.sql](./postgresql-schema.sql) to generate interactive ERD
   - Visit https://dbdiagram.io/d and paste the schema
   
3. **Draw.io / Lucidchart**:
   - Use the schema definitions to create custom diagrams
   - Export Mermaid diagrams as images for inclusion

4. **pgAdmin / DBeaver**:
   - Auto-generate ERD from PostgreSQL schema after migration
   - Connect to database and use built-in visualization tools
