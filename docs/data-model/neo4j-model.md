# Neo4j Graph Data Model

This document defines the graph database schema for SocialSim using Neo4j, including node types, relationship types, properties, and Cypher query examples.

## Overview

Neo4j stores the **social graph** - relationships between users and content that are optimized for traversal and network analysis. While PostgreSQL is the source of truth for entity data, Neo4j provides:

- Fast graph traversals (followers, influence paths)
- Network analysis (centrality, communities)
- Recommendation algorithms
- Social graph queries

## Node Types

### User Node

Represents a user/agent in the social network.

```cypher
CREATE (u:User {
  id: 'uuid-string',
  handle: '@username.domain.com',
  displayName: 'Display Name',
  followerCount: 1234,
  followingCount: 567,
  influenceScore: 0.75,
  isVerified: true,
  isSimulated: false,
  createdAt: datetime('2025-01-01T00:00:00Z'),
  lastActiveAt: datetime('2025-12-31T10:30:00Z')
})
```

**Properties:**
- `id` (String, required): UUID matching PostgreSQL Users.Id
- `handle` (String, required): User handle
- `displayName` (String): Display name
- `followerCount` (Integer): Cached follower count
- `followingCount` (Integer): Cached following count
- `influenceScore` (Float): Calculated influence (0-1)
- `isVerified` (Boolean): Verification status
- `isSimulated` (Boolean): Whether this is a simulated agent
- `createdAt` (DateTime): Account creation
- `lastActiveAt` (DateTime): Last activity

### Post Node

Represents a post/content item.

```cypher
CREATE (p:Post {
  id: 'uuid-string',
  authorId: 'author-uuid',
  content: 'Post text content...',
  visibility: 'Public',
  engagementScore: 123.45,
  likeCount: 42,
  repostCount: 15,
  replyCount: 8,
  createdAt: datetime('2025-12-31T10:30:00Z')
})
```

**Properties:**
- `id` (String, required): UUID matching PostgreSQL Posts.Id
- `authorId` (String, required): Author's user ID
- `content` (String): Post text
- `visibility` (String): Visibility level
- `engagementScore` (Float): Calculated engagement
- `likeCount` (Integer): Cached like count
- `repostCount` (Integer): Cached repost count
- `replyCount` (Integer): Cached reply count
- `createdAt` (DateTime): Post creation

### Thread Node

Represents a conversation thread.

```cypher
CREATE (t:Thread {
  id: 'uuid-string',
  rootPostId: 'root-post-uuid',
  participantCount: 5,
  postCount: 23,
  lastActivityAt: datetime('2025-12-31T10:30:00Z')
})
```

**Properties:**
- `id` (String, required): UUID matching PostgreSQL Threads.Id
- `rootPostId` (String): Root post UUID
- `participantCount` (Integer): Number of participants
- `postCount` (Integer): Number of posts
- `lastActivityAt` (DateTime): Last activity

## Relationship Types

### FOLLOWS

One user follows another.

```cypher
CREATE (follower:User)-[:FOLLOWS {
  createdAt: datetime('2025-01-15T10:30:00Z'),
  strength: 0.75,
  notificationsEnabled: true,
  isMuted: false
}]->(following:User)
```

**Properties:**
- `createdAt` (DateTime, required): When follow occurred
- `strength` (Float, 0-1): Relationship strength based on engagement
- `notificationsEnabled` (Boolean): Whether notifications enabled
- `isMuted` (Boolean): Whether muted from feed

**Use Cases:**
- Build follower/following lists
- Calculate network reach
- Generate recommendations
- Measure influence

### BLOCKS

One user blocks another.

```cypher
CREATE (blocker:User)-[:BLOCKS {
  createdAt: datetime('2025-01-15T10:30:00Z'),
  reason: 'harassment'
}]->(blocked:User)
```

**Properties:**
- `createdAt` (DateTime, required): When block occurred
- `reason` (String): Block reason

### MUTES

One user mutes another.

```cypher
CREATE (muter:User)-[:MUTES {
  createdAt: datetime('2025-01-15T10:30:00Z'),
  muteType: 'all',
  expiresAt: datetime('2025-01-22T10:30:00Z')
}]->(muted:User)
```

**Properties:**
- `createdAt` (DateTime, required): When mute occurred
- `muteType` (String): Type of mute (all, replies, reposts)
- `expiresAt` (DateTime, optional): Expiration time

### POSTS

User created a post.

```cypher
CREATE (author:User)-[:POSTS {
  createdAt: datetime('2025-12-31T10:30:00Z')
}]->(post:Post)
```

**Properties:**
- `createdAt` (DateTime, required): Post creation time

### LIKES

User likes a post.

```cypher
CREATE (user:User)-[:LIKES {
  createdAt: datetime('2025-12-31T10:30:00Z'),
  reactionType: 'like'
}]->(post:Post)
```

**Properties:**
- `createdAt` (DateTime, required): Like timestamp
- `reactionType` (String): Type of reaction (like, love, laugh, etc.)

### REPOSTS

User reposts a post.

```cypher
CREATE (user:User)-[:REPOSTS {
  createdAt: datetime('2025-12-31T10:30:00Z')
}]->(post:Post)
```

**Properties:**
- `createdAt` (DateTime, required): Repost timestamp

### QUOTES

Post quotes another post.

```cypher
CREATE (quotePost:Post)-[:QUOTES {
  createdAt: datetime('2025-12-31T10:30:00Z')
}]->(quotedPost:Post)
```

**Properties:**
- `createdAt` (DateTime, required): Quote timestamp

### REPLIES_TO

Post replies to another post.

```cypher
CREATE (reply:Post)-[:REPLIES_TO {
  createdAt: datetime('2025-12-31T10:30:00Z')
}]->(parent:Post)
```

**Properties:**
- `createdAt` (DateTime, required): Reply timestamp

### MENTIONS

Post mentions a user.

```cypher
CREATE (post:Post)-[:MENTIONS {
  createdAt: datetime('2025-12-31T10:30:00Z'),
  startIndex: 10,
  endIndex: 23
}]->(user:User)
```

**Properties:**
- `createdAt` (DateTime, required): Mention timestamp
- `startIndex` (Integer): Character start position
- `endIndex` (Integer): Character end position

### BELONGS_TO_THREAD

Post belongs to a thread.

```cypher
CREATE (post:Post)-[:BELONGS_TO_THREAD]->(thread:Thread)
```

### PARTICIPATES_IN

User participates in a thread.

```cypher
CREATE (user:User)-[:PARTICIPATES_IN {
  postCount: 5,
  firstPostAt: datetime('2025-12-31T08:00:00Z'),
  lastPostAt: datetime('2025-12-31T10:30:00Z')
}]->(thread:Thread)
```

**Properties:**
- `postCount` (Integer): Number of posts by user in thread
- `firstPostAt` (DateTime): First participation
- `lastPostAt` (DateTime): Most recent participation

## Indexes

Create indexes for performance:

```cypher
-- User indexes
CREATE INDEX user_id_idx FOR (u:User) ON (u.id);
CREATE INDEX user_handle_idx FOR (u:User) ON (u.handle);
CREATE INDEX user_influence_idx FOR (u:User) ON (u.influenceScore);

-- Post indexes
CREATE INDEX post_id_idx FOR (p:Post) ON (p.id);
CREATE INDEX post_author_idx FOR (p:Post) ON (p.authorId);
CREATE INDEX post_created_idx FOR (p:Post) ON (p.createdAt);
CREATE INDEX post_engagement_idx FOR (p:Post) ON (p.engagementScore);

-- Thread indexes
CREATE INDEX thread_id_idx FOR (t:Thread) ON (t.id);
CREATE INDEX thread_activity_idx FOR (t:Thread) ON (t.lastActivityAt);

-- Relationship indexes
CREATE INDEX follows_created_idx FOR ()-[r:FOLLOWS]-() ON (r.createdAt);
CREATE INDEX likes_created_idx FOR ()-[r:LIKES]-() ON (r.createdAt);
```

## Common Queries

### Social Graph Queries

#### Get User's Followers

```cypher
MATCH (follower:User)-[:FOLLOWS]->(user:User {id: $userId})
RETURN follower
ORDER BY follower.followerCount DESC
LIMIT 100;
```

#### Get User's Following

```cypher
MATCH (user:User {id: $userId})-[:FOLLOWS]->(following:User)
RETURN following
ORDER BY following.followerCount DESC
LIMIT 100;
```

#### Find Mutual Follows (Friends)

```cypher
MATCH (a:User {id: $userId})-[:FOLLOWS]->(b:User)-[:FOLLOWS]->(a)
RETURN b
ORDER BY b.displayName;
```

#### Get Follower Count

```cypher
MATCH (user:User {id: $userId})<-[:FOLLOWS]-(follower)
RETURN count(follower) AS followerCount;
```

#### Find Second-Degree Connections

```cypher
MATCH (a:User {id: $userId})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(b:User)
WHERE NOT (a)-[:FOLLOWS]->(b)
  AND a <> b
  AND NOT (a)-[:BLOCKS]->(b)
  AND NOT (b)-[:BLOCKS]->(a)
RETURN DISTINCT b
ORDER BY b.followerCount DESC
LIMIT 50;
```

### Engagement Queries

#### Get Post Likers

```cypher
MATCH (user:User)-[l:LIKES]->(post:Post {id: $postId})
RETURN user, l.reactionType, l.createdAt
ORDER BY l.createdAt DESC
LIMIT 100;
```

#### Get User's Liked Posts

```cypher
MATCH (user:User {id: $userId})-[l:LIKES]->(post:Post)
RETURN post, l.createdAt
ORDER BY l.createdAt DESC
LIMIT 50;
```

#### Track Repost Cascade

```cypher
MATCH path = (original:Post {id: $postId})<-[:REPOSTS*1..5]-(repost:User)
RETURN path, length(path) AS cascadeDepth
ORDER BY cascadeDepth DESC;
```

### Thread Queries

#### Get Thread Participants

```cypher
MATCH (user:User)-[p:PARTICIPATES_IN]->(thread:Thread {id: $threadId})
RETURN user, p.postCount
ORDER BY p.postCount DESC;
```

#### Get Thread Posts

```cypher
MATCH (post:Post)-[:BELONGS_TO_THREAD]->(thread:Thread {id: $threadId})
MATCH (author:User)-[:POSTS]->(post)
RETURN post, author
ORDER BY post.createdAt ASC;
```

### Network Analysis

#### Calculate PageRank

```cypher
CALL gds.pageRank.stream({
  nodeProjection: 'User',
  relationshipProjection: 'FOLLOWS'
})
YIELD nodeId, score
RETURN gds.util.asNode(nodeId).handle AS handle, score
ORDER BY score DESC
LIMIT 20;
```

#### Detect Communities (Louvain)

```cypher
CALL gds.louvain.stream({
  nodeProjection: 'User',
  relationshipProjection: 'FOLLOWS'
})
YIELD nodeId, communityId
RETURN communityId, count(nodeId) AS memberCount
ORDER BY memberCount DESC;
```

#### Calculate Betweenness Centrality

```cypher
CALL gds.betweenness.stream({
  nodeProjection: 'User',
  relationshipProjection: 'FOLLOWS'
})
YIELD nodeId, score
RETURN gds.util.asNode(nodeId).handle AS handle, score
ORDER BY score DESC
LIMIT 20;
```

#### Find Shortest Path Between Users

```cypher
MATCH (start:User {id: $userId1}),
      (end:User {id: $userId2}),
      path = shortestPath((start)-[:FOLLOWS*]-(end))
RETURN path, length(path) AS distance;
```

### Recommendation Queries

#### Recommend Users to Follow (Collaborative Filtering)

```cypher
MATCH (user:User {id: $userId})-[:FOLLOWS]->(followed:User)
MATCH (followed)<-[:FOLLOWS]-(other:User)-[:FOLLOWS]->(recommendation:User)
WHERE NOT (user)-[:FOLLOWS]->(recommendation)
  AND user <> recommendation
  AND NOT (user)-[:BLOCKS]->(recommendation)
WITH recommendation, count(DISTINCT other) AS commonFollowers
RETURN recommendation
ORDER BY commonFollowers DESC, recommendation.followerCount DESC
LIMIT 20;
```

#### Recommend Posts (From Network)

```cypher
MATCH (user:User {id: $userId})-[:FOLLOWS]->(followed:User)-[:POSTS]->(post:Post)
WHERE NOT (user)-[:LIKES]->(post)
  AND post.createdAt > datetime() - duration('P7D')
RETURN post, followed
ORDER BY post.engagementScore DESC
LIMIT 50;
```

## Synchronization with PostgreSQL

### Write Strategy

1. **PostgreSQL First**: All writes go to PostgreSQL
2. **Event-Driven Sync**: Events trigger Neo4j updates
3. **Eventual Consistency**: Neo4j may lag slightly

### Example: Create Follow

```csharp
// 1. Write to PostgreSQL
await _followRepo.CreateAsync(new Follow {
    FollowerId = userId,
    FollowingId = targetUserId,
    Status = FollowStatus.Active
});

// 2. Emit event
await _eventBus.PublishAsync(new UserFollowedEvent {
    FollowerId = userId,
    FollowingId = targetUserId
});

// 3. Event handler updates Neo4j
public async Task Handle(UserFollowedEvent evt)
{
    await _neo4jClient.Cypher
        .Match("(follower:User {id: $followerId})")
        .Match("(following:User {id: $followingId})")
        .Create("(follower)-[:FOLLOWS {createdAt: datetime(), strength: 0.5}]->(following)")
        .WithParams(new {
            followerId = evt.FollowerId,
            followingId = evt.FollowingId
        })
        .ExecuteWithoutResultsAsync();
}
```

### Read Strategy

- **PostgreSQL**: For user profiles, posts content, detailed data
- **Neo4j**: For graph traversals, recommendations, network analysis

## Performance Considerations

### Graph Projection

For intensive analysis, create in-memory graph projections:

```cypher
CALL gds.graph.project(
  'social-network',
  'User',
  'FOLLOWS'
);
```

### Relationship Weighting

Update follow relationship strength based on engagement:

```cypher
MATCH (a:User)-[f:FOLLOWS]->(b:User)
MATCH (a)-[l:LIKES]->(p:Post)<-[:POSTS]-(b)
WITH f, count(l) AS likeCount
SET f.strength = CASE
  WHEN likeCount > 100 THEN 1.0
  WHEN likeCount > 50 THEN 0.8
  WHEN likeCount > 20 THEN 0.6
  WHEN likeCount > 5 THEN 0.4
  ELSE 0.2
END;
```

## Next Steps

See [postgresql-schema.sql](./postgresql-schema.sql) for the relational database schema that complements this graph model.
