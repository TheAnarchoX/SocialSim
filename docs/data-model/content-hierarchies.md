# Content Hierarchies

This document defines how posts are organized into threads, conversations, and quote chains.

## Design Goals

1. **Conversational Context**: Group related posts for coherent reading
2. **Efficient Queries**: Fast retrieval of threads without excessive joins
3. **Flexible Structure**: Support linear threads, branching discussions, and quote chains
4. **Scalability**: Handle viral threads with thousands of replies
5. **Navigation**: Easy traversal of conversation structure

---

## 1. Thread Model

A **Thread** represents a conversation initiated by a root post with replies.

### PostgreSQL Table: `Threads`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique thread identifier |
| `RootPostId` | UUID | FK→Post.Id, NOT NULL | First post in thread |
| `Title` | VARCHAR(256) | NULLABLE | Optional thread title |
| `ParticipantCount` | INTEGER | NOT NULL, DEFAULT 1 | Unique users in thread |
| `PostCount` | INTEGER | NOT NULL, DEFAULT 1 | Total posts in thread |
| `ViewCount` | INTEGER | NOT NULL, DEFAULT 0 | Thread views |
| `LastActivityAt` | TIMESTAMP | NOT NULL | Last post/reply timestamp |
| `IsLocked` | BOOLEAN | NOT NULL, DEFAULT FALSE | No new replies allowed |
| `CreatedAt` | TIMESTAMP | NOT NULL | Thread creation timestamp |
| `UpdatedAt` | TIMESTAMP | NOT NULL | Last update timestamp |

### Indexes

**PostgreSQL:**
- PRIMARY KEY: `Id`
- UNIQUE: `RootPostId` (one thread per root post)
- INDEX: `LastActivityAt DESC` (active threads)
- INDEX: `PostCount DESC` (popular threads)

### Business Rules

1. Thread created automatically when root post created
2. Thread `Id` same as root post `Id` (or separate UUID)
3. Thread locked by moderators or automatically (e.g., after 6 months)
4. Thread metrics updated on every reply
5. Thread participants list can be cached/materialized

---

## 2. Post Thread Relationships

Posts reference their thread context via several fields.

### Post Fields for Threading

From the `Post` entity (see core-entities.md):

| Field | Type | Description |
|-------|------|-------------|
| `IsReply` | BOOLEAN | Whether this post is a reply |
| `ReplyToPostId` | UUID | Immediate parent post (nullable) |
| `ReplyToUserId` | UUID | User being replied to (nullable) |
| `ThreadId` | UUID | Conversation thread this belongs to (nullable) |
| `RootPostId` | UUID | Root post of thread (nullable) |

### Thread Structure Types

#### Linear Thread (Twitter/X style)

```
Root Post (Thread.RootPostId)
  └─ Reply 1 (ReplyToPostId = Root)
       └─ Reply 2 (ReplyToPostId = Reply 1)
            └─ Reply 3 (ReplyToPostId = Reply 2)
```

All posts have same `ThreadId` and `RootPostId`.

#### Branching Thread (Reddit style)

```
Root Post
  ├─ Reply 1A
  │    ├─ Reply 1A-i
  │    └─ Reply 1A-ii
  ├─ Reply 1B
  └─ Reply 1C
       └─ Reply 1C-i
```

Each post knows its parent via `ReplyToPostId`, enabling tree traversal.

---

## 3. Reply Chain Paths

To efficiently query reply chains without recursive queries, use materialized paths.

### PostgreSQL Table: `ReplyPaths` (Optional Optimization)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post in thread |
| `ThreadId` | UUID | FK→Thread.Id, NOT NULL | Thread containing post |
| `Path` | VARCHAR(2048) | NOT NULL | Materialized path (e.g., "root.reply1.reply2") |
| `Depth` | INTEGER | NOT NULL | Reply depth (0 = root) |
| `AncestorPostIds` | UUID[] | NOT NULL | Array of ancestor post IDs |

### Indexes

- PRIMARY KEY: `PostId`
- INDEX: `ThreadId, Depth ASC, CreatedAt ASC` (thread view)
- INDEX: `Path` (for subtree queries using LIKE)

### Materialized Path Example

```
Root:          Path = "01HQRS0001"                    Depth = 0
  Reply 1:     Path = "01HQRS0001.01HQRS0002"         Depth = 1
    Reply 2:   Path = "01HQRS0001.01HQRS0002.01HQRS0003"   Depth = 2
  Reply 3:     Path = "01HQRS0001.01HQRS0004"         Depth = 1
```

To get all replies under "Reply 1":
```sql
SELECT * FROM Posts p
JOIN ReplyPaths rp ON p.Id = rp.PostId
WHERE rp.Path LIKE '01HQRS0001.01HQRS0002%'
ORDER BY rp.Path ASC;
```

---

## 4. Conversation Participants

Track users who have participated in a thread.

### PostgreSQL Table: `ThreadParticipants`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `ThreadId` | UUID | FK→Thread.Id, NOT NULL | Thread |
| `UserId` | UUID | FK→User.Id, NOT NULL | Participant |
| `PostCount` | INTEGER | NOT NULL, DEFAULT 1 | Posts by user in thread |
| `FirstPostAt` | TIMESTAMP | NOT NULL | First participation |
| `LastPostAt` | TIMESTAMP | NOT NULL | Most recent participation |

### Indexes

- PRIMARY KEY: `(ThreadId, UserId)`
- INDEX: `ThreadId, PostCount DESC` (top participants)
- INDEX: `UserId, LastPostAt DESC` (user's active threads)

### Business Rules

1. Record created when user first posts in thread
2. Updated on each subsequent post
3. Used for "X and Y are discussing" UI elements
4. Can be used for notification settings (mute thread)

---

## 5. Quote Chains

Quote posts create a different type of hierarchy from reply threads.

### Quote Chain Structure

```
Original Post (A)
  ├─ Quote 1 (B quotes A)
  │    └─ Quote 2 (C quotes B, which includes A)
  ├─ Quote 3 (D quotes A)
  └─ Quote 4 (E quotes A)
```

### PostgreSQL Table: `QuoteChains` (Optional)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `QuotePostId` | UUID | FK→Post.Id, NOT NULL | The quote post |
| `OriginalPostId` | UUID | FK→Post.Id, NOT NULL | The quoted post |
| `ChainDepth` | INTEGER | NOT NULL, DEFAULT 1 | Quote nesting level |
| `ChainPath` | UUID[] | NOT NULL | Array of post IDs in chain |
| `CreatedAt` | TIMESTAMP | NOT NULL | Quote timestamp |

### Indexes

- PRIMARY KEY: `QuotePostId`
- INDEX: `OriginalPostId, CreatedAt DESC` (quotes of a post)
- INDEX: `ChainDepth` (for limiting deep quote chains)

### Quote Chain Queries

**Find all quotes of a post:**
```sql
SELECT p.* FROM Posts p
WHERE p.QuotedPostId = $postId
ORDER BY p.CreatedAt DESC;
```

**Find nested quote chains:**
```cypher
// Neo4j: Track quote propagation
MATCH path = (original:Post {id: $postId})<-[:QUOTES*1..5]-(quote:Post)
RETURN path, length(path) AS quoteDepth
ORDER BY quoteDepth DESC
```

**Prevent infinite quote loops:**
```csharp
// Business logic: Cannot quote a post that quotes you
if (await IsCircularQuote(quotedPostId, currentUserId))
{
    throw new InvalidOperationException("Circular quote detected");
}
```

---

## 6. Conversation Trees

Visualizing conversation structure requires tree traversal.

### Recursive CTE for Thread Tree

```sql
-- Get entire thread tree
WITH RECURSIVE ThreadTree AS (
    -- Base case: root post
    SELECT 
        Id, 
        AuthorId, 
        Content, 
        ReplyToPostId, 
        0 AS Depth,
        ARRAY[Id] AS Path,
        CAST(CreatedAt AS VARCHAR) AS SortPath
    FROM Posts 
    WHERE Id = $rootPostId
    
    UNION ALL
    
    -- Recursive case: replies
    SELECT 
        p.Id,
        p.AuthorId,
        p.Content,
        p.ReplyToPostId,
        tt.Depth + 1,
        tt.Path || p.Id,
        tt.SortPath || '.' || CAST(p.CreatedAt AS VARCHAR)
    FROM Posts p
    INNER JOIN ThreadTree tt ON p.ReplyToPostId = tt.Id
    WHERE tt.Depth < 10  -- Prevent infinite recursion
)
SELECT * FROM ThreadTree
ORDER BY SortPath;
```

### Thread Rendering Strategies

#### Strategy 1: Flat Timeline (Twitter)
All replies in chronological order, each showing parent context.

```
Post A
├─ Post B (reply to A)
├─ Post C (reply to B)  [Shows: "Replying to @userB"]
└─ Post D (reply to A)
```

#### Strategy 2: Nested Tree (Reddit)
Full tree structure with indentation.

```
Post A
├─ Post B
│  ├─ Post C
│  └─ Post D
└─ Post E
   └─ Post F
```

#### Strategy 3: Highlighted Chain (Default)
Show main chain, collapse side branches.

```
Post A
├─ Post B ⭐ (most engagement)
│  └─ Post C ⭐
│     └─ Post D ⭐
└─ [+15 other replies] (collapsed)
```

---

## 7. Thread Metrics & Ranking

### Thread Popularity Score

```sql
SELECT 
    t.Id,
    t.RootPostId,
    t.PostCount,
    t.ParticipantCount,
    t.ViewCount,
    t.LastActivityAt,
    (
        t.PostCount * 1.0 +
        t.ParticipantCount * 2.0 +
        t.ViewCount * 0.01 +
        EXTRACT(EPOCH FROM (NOW() - t.LastActivityAt)) * -0.0001
    ) AS PopularityScore
FROM Threads t
WHERE t.LastActivityAt > NOW() - INTERVAL '7 days'
ORDER BY PopularityScore DESC;
```

### Active Thread Detection

Threads with recent activity (bump mechanism):

```sql
SELECT t.*, COUNT(p.Id) AS RecentPosts
FROM Threads t
LEFT JOIN Posts p ON p.ThreadId = t.Id 
    AND p.CreatedAt > NOW() - INTERVAL '1 hour'
WHERE t.LastActivityAt > NOW() - INTERVAL '24 hours'
GROUP BY t.Id
HAVING COUNT(p.Id) > 5
ORDER BY t.LastActivityAt DESC;
```

---

## 8. Thread Moderation

### Thread Lock/Archive

```sql
UPDATE Threads
SET IsLocked = true,
    UpdatedAt = NOW()
WHERE Id = $threadId;
```

Locked threads:
- No new replies allowed
- Existing posts remain visible
- Can still be quoted/shared
- Moderator action required to unlock

### Thread Depth Limits

Prevent excessively deep nesting:

```csharp
public const int MaxReplyDepth = 10;

if (await GetReplyDepth(parentPostId) >= MaxReplyDepth)
{
    throw new InvalidOperationException("Maximum reply depth exceeded");
}
```

---

## 9. Notifications for Thread Activity

### Thread Subscription

Users auto-subscribe to threads when:
1. They create the root post
2. They reply to the thread
3. They are mentioned in the thread

### PostgreSQL Table: `ThreadSubscriptions`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `ThreadId` | UUID | FK→Thread.Id, NOT NULL | Thread being followed |
| `UserId` | UUID | FK→User.Id, NOT NULL | Subscribed user |
| `NotificationLevel` | ENUM | NOT NULL, DEFAULT 'All' | All, Mentions, None |
| `IsMuted` | BOOLEAN | NOT NULL, DEFAULT FALSE | Mute thread notifications |
| `CreatedAt` | TIMESTAMP | NOT NULL | Subscription timestamp |

### Indexes

- PRIMARY KEY: `(ThreadId, UserId)`
- INDEX: `UserId, CreatedAt DESC` (user's subscribed threads)

### Notification Levels

- **All**: Notify on every new post
- **Mentions**: Notify only when mentioned
- **None**: No notifications (but still tracking participation)

---

## 10. Graph Representation of Conversations

### Neo4j Thread Structure

```cypher
// Posts in thread
(:Post {id: "root"})-[:STARTS_THREAD]->(:Thread {id: "thread-123"})
(:Post {id: "reply1"})-[:BELONGS_TO_THREAD]->(:Thread {id: "thread-123"})
(:Post {id: "reply1"})-[:REPLIES_TO]->(:Post {id: "root"})
(:Post {id: "reply2"})-[:REPLIES_TO]->(:Post {id: "reply1"})

// Conversation participants
(:User {id: "user1"})-[:PARTICIPATES_IN {postCount: 3}]->(:Thread {id: "thread-123"})
```

### Conversation Graph Queries

**Find most active threads for user:**
```cypher
MATCH (u:User {id: $userId})-[p:PARTICIPATES_IN]->(t:Thread)
RETURN t, p.postCount
ORDER BY p.postCount DESC, t.lastActivityAt DESC
LIMIT 20
```

**Find related conversations:**
```cypher
// Threads with overlapping participants
MATCH (t1:Thread {id: $threadId})<-[:PARTICIPATES_IN]-(u:User)-[:PARTICIPATES_IN]->(t2:Thread)
WHERE t1 <> t2
WITH t2, count(DISTINCT u) AS commonParticipants
ORDER BY commonParticipants DESC
LIMIT 10
RETURN t2, commonParticipants
```

---

## Performance Considerations

### Caching Strategy

1. **Thread metadata**: Cache in Redis (participant count, last activity)
2. **Recent posts**: Cache last 50 posts of active threads
3. **Thread tree**: Materialize paths for hot threads
4. **Participant lists**: Cache for threads with < 100 participants

### Pagination Strategy

For long threads:

```sql
-- Page through thread chronologically
SELECT p.* FROM Posts p
WHERE p.ThreadId = $threadId
  AND p.CreatedAt > $cursor  -- Cursor-based pagination
ORDER BY p.CreatedAt ASC
LIMIT 50;
```

Or by reply depth:

```sql
-- Page through thread by tree structure
SELECT p.*, rp.Depth, rp.Path
FROM Posts p
JOIN ReplyPaths rp ON p.Id = rp.PostId
WHERE rp.ThreadId = $threadId
  AND rp.Path > $cursor  -- Path-based pagination
ORDER BY rp.Path ASC
LIMIT 50;
```

---

## Summary

| Structure | Storage | Query Pattern | Use Case |
|-----------|---------|---------------|----------|
| **Thread** | PostgreSQL + Neo4j | By ThreadId, LastActivityAt | Group conversations |
| **Reply Chain** | Post.ReplyToPostId | Recursive CTE, Materialized Path | Nested discussions |
| **Quote Chain** | Post.QuotedPostId | Quote graph traversal | Quote propagation |
| **Participants** | ThreadParticipants | By ThreadId | "X is discussing" UI |
| **Subscriptions** | ThreadSubscriptions | By UserId | Thread notifications |

## Next Steps

See [privacy-visibility.md](./privacy-visibility.md) for access control and content visibility rules.
