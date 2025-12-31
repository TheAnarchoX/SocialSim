# Engagement Models

This document defines how users engage with content through likes, shares, quotes, bookmarks, and reactions.

## Design Philosophy

Engagement data is **write-heavy** and **read-frequently**. We optimize for:

1. **Fast writes**: Engagement actions must be instant
2. **Accurate counts**: Counts must be consistent and queryable
3. **User history**: Users can view their own engagement history
4. **Analytics**: Aggregate engagement for trending algorithms

## Storage Strategy

- **PostgreSQL**: Individual engagement records, user engagement history
- **Redis**: Cached counts, rate limiting, real-time analytics
- **Neo4j**: Engagement relationships for network analysis

---

## 1. Like (Reaction)

Represents a user liking a post.

### PostgreSQL Table: `Likes`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `UserId` | UUID | FK→User.Id, NOT NULL | User who liked |
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post being liked |
| `ReactionType` | ENUM | NOT NULL, DEFAULT 'Like' | Like, Love, Laugh, Sad, Angry |
| `CreatedAt` | TIMESTAMP | NOT NULL | When like occurred |

### Neo4j Relationship: `LIKES`

```cypher
(:User {id: "user-uuid"})-[:LIKES {
  createdAt: datetime("2025-01-15T10:30:00Z"),
  reactionType: "like"
}]->(:Post {id: "post-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- UNIQUE: `(UserId, PostId)` (prevent duplicate likes)
- INDEX: `UserId, CreatedAt DESC` (user's like history)
- INDEX: `PostId, CreatedAt DESC` (post's likes)
- INDEX: `PostId, ReactionType` (likes by type)

### Business Rules

1. One like per user per post (unlike removes record)
2. Like notifications sent to post author
3. Like count cached on Post entity
4. Cannot like own posts (optional rule)
5. Cannot like deleted posts
6. Like triggers engagement score recalculation

### Reaction Types

Support for multiple reaction types (Facebook-style):

- **Like**: Standard thumbs up (👍)
- **Love**: Heart reaction (❤️)
- **Laugh**: Laughing face (😂)
- **Wow**: Surprised face (😮)
- **Sad**: Sad face (😢)
- **Angry**: Angry face (😠)

Each reaction type can be aggregated separately for sentiment analysis.

---

## 2. Repost (Share)

Represents a user reposting/sharing another user's post.

### PostgreSQL Table: `Reposts`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier (also the Post.Id of repost) |
| `UserId` | UUID | FK→User.Id, NOT NULL | User who reposted |
| `OriginalPostId` | UUID | FK→Post.Id, NOT NULL | Original post being shared |
| `RepostPostId` | UUID | FK→Post.Id, NOT NULL | The new post created by repost |
| `CreatedAt` | TIMESTAMP | NOT NULL | When repost occurred |

### Neo4j Relationship: `REPOSTS`

```cypher
(:User {id: "user-uuid"})-[:REPOSTS {
  createdAt: datetime("2025-01-15T10:30:00Z")
}]->(:Post {id: "original-post-uuid"})

// Chain reposts for virality tracking
(:Post {id: "repost-uuid"})-[:REPOST_OF]->(:Post {id: "original-post-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- UNIQUE: `(UserId, OriginalPostId)` (prevent duplicate reposts)
- INDEX: `UserId, CreatedAt DESC` (user's reposts)
- INDEX: `OriginalPostId, CreatedAt DESC` (reposts of a post)
- INDEX: `RepostPostId` (lookup repost details)

### Business Rules

1. Reposting creates a new Post entity with `RepostOfPostId` set
2. Repost inherits original post's content (immutable reference)
3. Repost count cached on original Post entity
4. Un-reposting deletes the repost Post
5. Visibility rules: Repost can be more public but not more private
6. Original author gets notification

### Repost vs Quote

- **Repost**: Share without commentary (simple amplification)
- **Quote**: Share with added commentary (see Quote section)

---

## 3. Quote Post

Represents a user quoting another post with their own commentary.

### PostgreSQL Table: `Quotes`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier (also the Post.Id of quote) |
| `UserId` | UUID | FK→User.Id, NOT NULL | User who quoted |
| `QuotedPostId` | UUID | FK→Post.Id, NOT NULL | Post being quoted |
| `QuotePostId` | UUID | FK→Post.Id, NOT NULL | The new post with quote + commentary |
| `CreatedAt` | TIMESTAMP | NOT NULL | When quote occurred |

### Neo4j Relationship: `QUOTES`

```cypher
(:User {id: "user-uuid"})-[:QUOTES {
  createdAt: datetime("2025-01-15T10:30:00Z")
}]->(:Post {id: "quoted-post-uuid"})

// Link quote chain
(:Post {id: "quote-post-uuid"})-[:QUOTES]->(:Post {id: "quoted-post-uuid"})
```

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- INDEX: `UserId, CreatedAt DESC` (user's quotes)
- INDEX: `QuotedPostId, CreatedAt DESC` (quotes of a post)
- INDEX: `QuotePostId` (lookup quote details)

### Business Rules

1. Quote creates new Post entity with both content and `QuotedPostId`
2. Quote count cached on original Post entity
3. Quoted post embedded in quote post
4. Original author gets notification
5. Quote chains tracked for conversation analysis
6. Deleted quoted posts show as [unavailable] in quote

### Quote Post Structure

A quote post contains:
- New content from quoting user
- Embedded reference to quoted post
- All standard post metadata

Example:
```json
{
  "id": "quote-post-uuid",
  "authorId": "user-uuid",
  "content": "This is so true! Everyone should read this.",
  "quotedPostId": "original-post-uuid",
  "quotedPost": {
    "id": "original-post-uuid",
    "authorId": "original-author-uuid",
    "content": "Hot take: The sky is blue.",
    "createdAt": "2025-01-15T09:00:00Z"
  },
  "createdAt": "2025-01-15T10:30:00Z"
}
```

---

## 4. Bookmark (Save)

Represents a user bookmarking a post for later.

### PostgreSQL Table: `Bookmarks`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `UserId` | UUID | FK→User.Id, NOT NULL | User who bookmarked |
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post being bookmarked |
| `FolderId` | UUID | FK→BookmarkFolder.Id, NULLABLE | Optional folder/collection |
| `Notes` | TEXT | NULLABLE | Private notes about bookmark |
| `CreatedAt` | TIMESTAMP | NOT NULL | When bookmark occurred |

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- UNIQUE: `(UserId, PostId)` (prevent duplicate bookmarks)
- INDEX: `UserId, CreatedAt DESC` (user's bookmarks)
- INDEX: `FolderId, CreatedAt DESC` (bookmarks in folder)
- INDEX: `PostId` (bookmark count per post - optional)

### Business Rules

1. Bookmarks are **private** (not visible to others)
2. No notification sent to post author
3. Bookmark count not typically displayed publicly
4. Bookmarks persist even if post is deleted (for user's reference)
5. Users can organize bookmarks into folders/collections

---

## 5. Bookmark Folder (Collection)

Represents a user's bookmark organization.

### PostgreSQL Table: `BookmarkFolders`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `UserId` | UUID | FK→User.Id, NOT NULL | Folder owner |
| `Name` | VARCHAR(128) | NOT NULL | Folder name |
| `Description` | TEXT | NULLABLE | Folder description |
| `Color` | VARCHAR(7) | NULLABLE | Hex color for UI |
| `Icon` | VARCHAR(32) | NULLABLE | Icon identifier |
| `SortOrder` | INTEGER | NOT NULL, DEFAULT 0 | Display order |
| `CreatedAt` | TIMESTAMP | NOT NULL | Creation timestamp |
| `UpdatedAt` | TIMESTAMP | NOT NULL | Last update timestamp |

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- INDEX: `UserId, SortOrder ASC` (user's folders)

### Business Rules

1. Folders are user-specific and private
2. Default "Uncategorized" folder for bookmarks without folder
3. Folders can be renamed, reordered, deleted
4. Deleting folder moves bookmarks to uncategorized

---

## 6. View Tracking

Represents a user viewing a post (for analytics and recommendation).

### PostgreSQL Table: `PostViews`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `UserId` | UUID | FK→User.Id, NULLABLE | Viewer (null for anonymous) |
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post viewed |
| `ViewDurationSeconds` | FLOAT | NULLABLE | How long post was viewed |
| `ViewedAt` | TIMESTAMP | NOT NULL | View timestamp |
| `SessionId` | UUID | NULLABLE | Session identifier |

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- INDEX: `PostId, ViewedAt DESC` (post views over time)
- INDEX: `UserId, ViewedAt DESC` (user view history)
- INDEX: `ViewedAt` (for time-based aggregations)

### Alternative: Aggregated Views

For performance, may use **aggregated view counts** instead of individual records:

```sql
CREATE TABLE PostViewsAggregated (
    PostId UUID NOT NULL,
    ViewDate DATE NOT NULL,
    ViewCount INTEGER NOT NULL DEFAULT 0,
    UniqueViewers INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (PostId, ViewDate)
);
```

### Business Rules

1. View tracking may be sampled (not 100% of views)
2. Views from same user within time window deduplicated
3. View count cached on Post entity
4. Views used for recommendation algorithms
5. Anonymous views tracked but not associated with user

---

## Engagement Score Calculation

The **EngagementScore** on Post entity is calculated from engagement metrics:

```
EngagementScore = (
    Likes * 1.0 +
    Loves * 1.5 +
    Reposts * 2.0 +
    Quotes * 2.5 +
    Replies * 1.5 +
    Bookmarks * 0.5 +
    Views * 0.01
) / age_decay_factor
```

Where `age_decay_factor` increases over time to favor recent content.

### Real-Time Score Updates

Engagement scores updated via:
1. **Immediate**: Redis increment on engagement action
2. **Batched**: Periodic sync to PostgreSQL (every 5 minutes)
3. **Recalculation**: Full recalc daily for decay factor

---

## Engagement Analytics

### User Engagement Metrics

Track per-user engagement stats:

```sql
CREATE TABLE UserEngagementStats (
    UserId UUID PRIMARY KEY,
    TotalLikesGiven INTEGER NOT NULL DEFAULT 0,
    TotalLikesReceived INTEGER NOT NULL DEFAULT 0,
    TotalRepostsGiven INTEGER NOT NULL DEFAULT 0,
    TotalRepostsReceived INTEGER NOT NULL DEFAULT 0,
    TotalRepliesGiven INTEGER NOT NULL DEFAULT 0,
    TotalRepliesReceived INTEGER NOT NULL DEFAULT 0,
    EngagementRate FLOAT NOT NULL DEFAULT 0,      -- % of views that result in engagement
    LastEngagementAt TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL
);
```

### Post Engagement Metrics

Cached on Post entity:
- `LikeCount`
- `RepostCount`
- `QuoteCount`
- `ReplyCount`
- `BookmarkCount` (optional, may stay private)
- `ViewCount`
- `EngagementScore`

### Trending Algorithm

Posts ranked for trending based on:
1. Recent engagement velocity (engagement/hour)
2. Engagement diversity (likes + reposts + replies, not just one type)
3. Network reach (followers of engagers)
4. Time decay (recent posts favored)

```sql
SELECT p.*, 
       (p.EngagementScore * 1000 / EXTRACT(EPOCH FROM (NOW() - p.CreatedAt)) ^ 1.5) AS TrendingScore
FROM Posts p
WHERE p.CreatedAt > NOW() - INTERVAL '24 hours'
  AND p.IsDeleted = false
ORDER BY TrendingScore DESC
LIMIT 100;
```

---

## Event-Driven Engagement

All engagement actions emit events for real-time processing:

### Event Types

```csharp
public class PostLikedEvent : SimulationEvent
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public ReactionType ReactionType { get; set; }
}

public class PostRepostedEvent : SimulationEvent
{
    public Guid UserId { get; set; }
    public Guid OriginalPostId { get; set; }
    public Guid RepostPostId { get; set; }
}

public class PostQuotedEvent : SimulationEvent
{
    public Guid UserId { get; set; }
    public Guid QuotedPostId { get; set; }
    public Guid QuotePostId { get; set; }
    public string QuoteContent { get; set; }
}

public class PostBookmarkedEvent : SimulationEvent
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid? FolderId { get; set; }
}

public class PostViewedEvent : SimulationEvent
{
    public Guid? UserId { get; set; }  // Nullable for anonymous
    public Guid PostId { get; set; }
    public float ViewDurationSeconds { get; set; }
}
```

### Event Processing Pipeline

1. **User Action** → API receives engagement request
2. **Validation** → Check business rules (can user engage?)
3. **Write to DB** → Insert engagement record
4. **Update Counts** → Increment cached counts (Redis + DB)
5. **Emit Event** → Publish to Redis pub/sub
6. **Notification** → Send to post author (if applicable)
7. **Graph Update** → Update Neo4j relationships (async)
8. **Analytics** → Update trending scores, recommendation features

---

## Graph Analysis of Engagement

### Engagement Network

Build engagement graph for content recommendation:

```cypher
// Users who engage with similar content
MATCH (u1:User)-[:LIKES]->(p:Post)<-[:LIKES]-(u2:User)
WHERE u1.id = $userId AND u1 <> u2
WITH u2, count(p) AS commonLikes
ORDER BY commonLikes DESC
LIMIT 50
RETURN u2, commonLikes
```

### Viral Content Tracking

```cypher
// Track repost cascade
MATCH path = (original:Post)<-[:REPOST_OF*1..5]-(repost:Post)
WHERE original.id = $postId
RETURN path, length(path) AS cascade_depth
ORDER BY cascade_depth DESC
```

### Engagement Influence

```cypher
// Calculate influence based on engagement from followers
MATCH (u:User {id: $userId})-[:POSTS]->(p:Post)<-[:LIKES]-(engager:User)
OPTIONAL MATCH (engager)<-[:FOLLOWS]-(followerOfEngager:User)
WITH u, p, engager, count(DISTINCT followerOfEngager) AS engagerReach
RETURN p.id, 
       count(DISTINCT engager) AS directEngagers,
       sum(engagerReach) AS totalReach
ORDER BY totalReach DESC
```

---

## Next Steps

See [content-hierarchies.md](./content-hierarchies.md) for thread and conversation structures.
