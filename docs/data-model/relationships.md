# Social Relationships

This document defines the social relationship entities for user-to-user interactions.

## Storage Strategy

Social relationships are stored in **both** PostgreSQL and Neo4j:

- **Neo4j (Primary)**: Graph queries, network analysis, traversals
- **PostgreSQL (Secondary)**: Caching, denormalization for API queries, audit trail

This polyglot persistence approach optimizes for both graph operations and relational queries.

---

## 1. Follow Relationship

Represents one user following another.

### PostgreSQL Table: `Follows`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `FollowerId` | UUID | FK→User.Id, NOT NULL | User doing the following |
| `FollowingId` | UUID | FK→User.Id, NOT NULL | User being followed |
| `Status` | ENUM | NOT NULL, DEFAULT 'Active' | Active, Pending, Rejected |
| `IsMuted` | BOOLEAN | NOT NULL, DEFAULT FALSE | Follow but mute from feed |
| `NotificationsEnabled` | BOOLEAN | NOT NULL, DEFAULT TRUE | Receive notifications |
| `CreatedAt` | TIMESTAMP | NOT NULL | When follow occurred |
| `UpdatedAt` | TIMESTAMP | NOT NULL | Last status change |

### Neo4j Relationship: `FOLLOWS`

```cypher
(:User {id: "follower-uuid"})-[:FOLLOWS {
  createdAt: datetime("2025-01-15T10:30:00Z"),
  strength: 0.75,                    // Calculated based on engagement
  notificationsEnabled: true,
  isMuted: false
}]->(:User {id: "following-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- UNIQUE: `(FollowerId, FollowingId)`
- INDEX: `FollowerId, CreatedAt DESC` (user's following list)
- INDEX: `FollowingId, CreatedAt DESC` (user's followers list)
- INDEX: `Status` (pending follow requests)

**Neo4j:**
- Index on `User.id`
- Index on `FOLLOWS.createdAt`

### Business Rules

1. User cannot follow themselves
2. Duplicate follows prevented by unique constraint
3. Private accounts require follow approval (Status='Pending')
4. Following count cached on User entity
5. Unfollowing sets status to rejected and soft-deletes

### Graph Properties

- **strength**: Calculated from engagement (likes, replies, views)
- **reciprocal**: Whether follow is mutual
- **category**: Optional categorization (close_friend, acquaintance, etc.)

---

## 2. Block Relationship

Represents one user blocking another.

### PostgreSQL Table: `Blocks`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `BlockerId` | UUID | FK→User.Id, NOT NULL | User doing the blocking |
| `BlockedId` | UUID | FK→User.Id, NOT NULL | User being blocked |
| `Reason` | ENUM | NULLABLE | Spam, Harassment, Other |
| `Notes` | TEXT | NULLABLE | Private notes about block |
| `CreatedAt` | TIMESTAMP | NOT NULL | When block occurred |

### Neo4j Relationship: `BLOCKS`

```cypher
(:User {id: "blocker-uuid"})-[:BLOCKS {
  createdAt: datetime("2025-01-15T10:30:00Z"),
  reason: "harassment"
}]->(:User {id: "blocked-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- UNIQUE: `(BlockerId, BlockedId)`
- INDEX: `BlockerId, CreatedAt DESC` (user's block list)
- INDEX: `BlockedId` (for reverse lookups)

### Business Rules

1. Blocking automatically unfollows both directions
2. Blocked users cannot see blocker's content
3. Blocked users cannot interact with blocker
4. Blocking is unidirectional (A blocks B ≠ B blocks A)
5. System tracks block patterns for abuse detection

### Effects of Blocking

- Hide all content from blocked user
- Prevent blocked user from following
- Remove existing follows in both directions
- Prevent mentions, replies, quotes
- Blocked user cannot view blocker's profile

---

## 3. Mute Relationship

Represents one user muting another (softer than block).

### PostgreSQL Table: `Mutes`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `MuterId` | UUID | FK→User.Id, NOT NULL | User doing the muting |
| `MutedId` | UUID | FK→User.Id, NOT NULL | User being muted |
| `MuteType` | ENUM | NOT NULL, DEFAULT 'All' | All, Replies, Reposts |
| `ExpiresAt` | TIMESTAMP | NULLABLE | Temporary mute expiration |
| `CreatedAt` | TIMESTAMP | NOT NULL | When mute occurred |

### Neo4j Relationship: `MUTES`

```cypher
(:User {id: "muter-uuid"})-[:MUTES {
  createdAt: datetime("2025-01-15T10:30:00Z"),
  muteType: "all",
  expiresAt: datetime("2025-01-22T10:30:00Z")  // Optional
}]->(:User {id: "muted-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- UNIQUE: `(MuterId, MutedId)`
- INDEX: `MuterId, CreatedAt DESC` (user's mute list)
- INDEX: `ExpiresAt` (for cleanup of expired mutes)

### Business Rules

1. Muting does not affect follow relationships
2. Muted user is not notified
3. Mute can be temporary or permanent
4. Mute types:
   - **All**: Hide all content from user
   - **Replies**: Hide only replies from user
   - **Reposts**: Hide only reposts from user
5. Expired mutes automatically removed by background job

### Effects of Muting

- Hide muted user's posts from feeds (based on MuteType)
- Muted user can still see muter's content
- Muted user can still interact (but muter won't see it)
- Mute is unidirectional and private

---

## 4. Report Relationship

Represents one user reporting another user or content.

### PostgreSQL Table: `Reports`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `ReporterId` | UUID | FK→User.Id, NOT NULL | User filing report |
| `ReportedUserId` | UUID | FK→User.Id, NULLABLE | Reported user (for user reports) |
| `ReportedPostId` | UUID | FK→Post.Id, NULLABLE | Reported post (for content reports) |
| `ReportType` | ENUM | NOT NULL | User, Post, Comment |
| `Reason` | ENUM | NOT NULL | Spam, Harassment, Violence, NSFW, Misinformation, Other |
| `Description` | TEXT | NULLABLE | Additional details |
| `Status` | ENUM | NOT NULL, DEFAULT 'Pending' | Pending, UnderReview, Resolved, Dismissed |
| `Resolution` | TEXT | NULLABLE | Moderator resolution notes |
| `ResolvedById` | UUID | FK→User.Id, NULLABLE | Moderator who resolved |
| `ResolvedAt` | TIMESTAMP | NULLABLE | Resolution timestamp |
| `CreatedAt` | TIMESTAMP | NOT NULL | When report was filed |

### Neo4j Relationship: `REPORTS`

```cypher
(:User {id: "reporter-uuid"})-[:REPORTS {
  createdAt: datetime("2025-01-15T10:30:00Z"),
  reason: "spam",
  status: "pending"
}]->(:User {id: "reported-uuid"})

// Or for content reports:
(:User {id: "reporter-uuid"})-[:REPORTS {
  createdAt: datetime("2025-01-15T10:30:00Z"),
  reason: "harassment",
  status: "pending"
}]->(:Post {id: "reported-post-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- INDEX: `ReporterId, CreatedAt DESC` (user's reports)
- INDEX: `ReportedUserId, CreatedAt DESC` (reports against user)
- INDEX: `ReportedPostId, CreatedAt DESC` (reports against post)
- INDEX: `Status, CreatedAt ASC` (pending reports queue)
- INDEX: `Reason, Status` (categorized reports)

### Business Rules

1. Must report either user OR post, not both
2. Duplicate reports from same user are prevented
3. Multiple reports on same target trigger priority review
4. Anonymous reporting not supported (accountability)
5. False reports can lead to reporter penalties

### Report Reasons

- **Spam**: Unsolicited commercial content, repetitive posts
- **Harassment**: Targeted harassment, bullying
- **Violence**: Threats, violent content
- **NSFW**: Adult content, graphic material
- **Misinformation**: False or misleading information
- **Impersonation**: Pretending to be someone else
- **SelfHarm**: Content promoting self-harm
- **Other**: Other violations of terms of service

---

## 5. Mention Relationship

Represents one user mentioning another in content.

### PostgreSQL Table: `Mentions`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post containing mention |
| `MentionerId` | UUID | FK→User.Id, NOT NULL | User who mentioned |
| `MentionedId` | UUID | FK→User.Id, NOT NULL | User being mentioned |
| `StartIndex` | INTEGER | NOT NULL | Byte position of mention start |
| `EndIndex` | INTEGER | NOT NULL | Byte position of mention end |
| `CreatedAt` | TIMESTAMP | NOT NULL | Mention timestamp (= post timestamp) |

### Neo4j Relationship: `MENTIONS`

```cypher
(:Post {id: "post-uuid"})-[:MENTIONS {
  createdAt: datetime("2025-01-15T10:30:00Z"),
  startIndex: 10,
  endIndex: 23
}]->(:User {id: "mentioned-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- INDEX: `PostId` (mentions in post)
- INDEX: `MentionedId, CreatedAt DESC` (mentions of user, for notifications)
- INDEX: `MentionerId, CreatedAt DESC` (mentions by user)

### Business Rules

1. Mentions trigger notifications
2. Mentions extracted from post content on creation
3. Mentions use AT Protocol facets format
4. Mentioned users can see post even if outside normal visibility
5. Muted/blocked users' mentions may not trigger notifications

---

## Relationship Summary

| Relationship | Directionality | Effect | Notification | Visibility |
|--------------|----------------|--------|--------------|------------|
| **Follow** | Unidirectional | See content in feed | Optional | Public/Private |
| **Block** | Unidirectional | Complete separation | No | Hidden |
| **Mute** | Unidirectional | Hide content | No | Hidden to muter only |
| **Report** | Unidirectional | Moderation review | To moderators | Private |
| **Mention** | Unidirectional | Tag in content | Yes | Based on post visibility |

## Graph Traversal Examples

### Find mutual follows (friends)

```cypher
MATCH (a:User {id: $userId})-[:FOLLOWS]->(b:User)-[:FOLLOWS]->(a)
RETURN b
```

### Find followers who aren't blocked

```cypher
MATCH (a:User {id: $userId})<-[:FOLLOWS]-(b:User)
WHERE NOT (a)-[:BLOCKS]->(b)
RETURN b
```

### Find second-degree connections

```cypher
MATCH (a:User {id: $userId})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(b:User)
WHERE NOT (a)-[:FOLLOWS]->(b)
  AND a <> b
RETURN DISTINCT b
LIMIT 50
```

### Calculate influence score

```cypher
MATCH (u:User {id: $userId})<-[:FOLLOWS]-(follower)
OPTIONAL MATCH (follower)<-[:FOLLOWS]-(secondDegree)
WITH u, count(DISTINCT follower) AS directFollowers,
     count(DISTINCT secondDegree) AS secondDegreeReach
RETURN u.id, 
       directFollowers,
       secondDegreeReach,
       (directFollowers * 1.0 + secondDegreeReach * 0.1) AS influenceScore
```

## Next Steps

See [engagement-models.md](./engagement-models.md) for content engagement entities (Likes, Shares, Bookmarks).
