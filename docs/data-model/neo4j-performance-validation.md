# Neo4j Performance Validation & Graph Algorithms

This document outlines the performance validation strategy and graph algorithm implementation for SocialSim's Neo4j database.

## Performance Testing Strategy

### Test Data Volumes

Performance tests should be run against realistic data volumes:

| Scale | Users | Follows | Posts | Likes |
|-------|-------|---------|-------|-------|
| Small | 1K | 10K | 10K | 50K |
| Medium | 100K | 1M | 1M | 5M |
| Large | 1M | 10M | 10M | 50M |
| XLarge | 10M | 100M | 100M | 500M |

### Critical Query Performance Targets

| Query Type | Target Latency | Max Latency |
|------------|----------------|-------------|
| Get user followers (p99) | < 50ms | < 200ms |
| Get user following (p99) | < 50ms | < 200ms |
| Find mutual follows | < 100ms | < 500ms |
| 2nd degree connections | < 200ms | < 1s |
| Get post likers | < 50ms | < 200ms |
| Thread participants | < 100ms | < 500ms |
| Shortest path (1-6 hops) | < 500ms | < 2s |
| PageRank (100K users) | < 5s | < 15s |
| Community detection | < 10s | < 30s |

## Query Performance Validation

### 1. Follower/Following Queries

**Test Query: Get User's Followers**

```cypher
PROFILE
MATCH (follower:User)-[f:FOLLOWS]->(user:User {id: $userId})
WHERE f.activeTo IS NULL
RETURN follower
ORDER BY follower.followerCount DESC
LIMIT 100;
```

**Performance Checklist:**
- ✅ Uses `user_id_idx` index for user lookup
- ✅ Uses `follows_created_idx` for relationship scan
- ✅ No Cartesian products in execution plan
- ✅ DB hits < 1000 for 100 results
- ✅ Execution time < 50ms (p50), < 200ms (p99)

**Optimization Notes:**
- If slow, ensure index on `User.id`
- Consider denormalizing follower list for high-profile users
- Use pagination to limit result set

**Test Query: Mutual Follows**

```cypher
PROFILE
MATCH (a:User {id: $userId})-[:FOLLOWS]->(b:User)-[:FOLLOWS]->(a)
WHERE NOT (a)-[:BLOCKS]-(b)
RETURN b
ORDER BY b.displayName
LIMIT 100;
```

**Performance Checklist:**
- ✅ Bidirectional pattern efficiently resolved
- ✅ BLOCKS check doesn't cause full scan
- ✅ Execution time < 100ms (p50), < 500ms (p99)

### 2. Second-Degree Connection Queries

```cypher
PROFILE
MATCH (a:User {id: $userId})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(b:User)
WHERE NOT (a)-[:FOLLOWS]->(b)
  AND a <> b
  AND NOT (a)-[:BLOCKS]->(b)
  AND NOT (b)-[:BLOCKS]->(a)
RETURN DISTINCT b
ORDER BY b.followerCount DESC
LIMIT 50;
```

**Performance Checklist:**
- ✅ Two-hop traversal completes efficiently
- ✅ DISTINCT doesn't cause memory issues
- ✅ Execution time < 200ms (p50), < 1s (p99)

**Optimization:**
```cypher
// Optimized version with relationship filtering
MATCH (a:User {id: $userId})-[f1:FOLLOWS]->(:User)-[f2:FOLLOWS]->(b:User)
WHERE f1.activeTo IS NULL 
  AND f2.activeTo IS NULL
  AND NOT (a)-[:FOLLOWS]->(b)
  AND a <> b
  AND NOT EXISTS((a)-[:BLOCKS]-(b))
WITH b, count(*) AS commonConnections
RETURN b
ORDER BY commonConnections DESC, b.followerCount DESC
LIMIT 50;
```

### 3. Engagement Queries

**Test Query: Post Cascade Tracking**

```cypher
PROFILE
MATCH path = (original:Post {id: $postId})<-[:REPOSTS*1..5]-(user:User)
RETURN path, length(path) AS depth
ORDER BY depth DESC
LIMIT 100;
```

**Performance Checklist:**
- ✅ Variable-length path doesn't timeout
- ✅ Depth limit prevents infinite recursion
- ✅ Execution time < 500ms for 5 hops
- ✅ Consider caching for viral posts

### 4. Thread Queries

```cypher
PROFILE
MATCH (post:Post)-[:BELONGS_TO_THREAD]->(thread:Thread {id: $threadId})
MATCH (author:User)-[:POSTS]->(post)
RETURN post, author
ORDER BY post.createdAt ASC
LIMIT 100;
```

**Performance Checklist:**
- ✅ Thread lookup uses index
- ✅ Join to author efficient
- ✅ Execution time < 100ms

## Graph Algorithm Implementation

### 1. PageRank (Influence Scoring)

**Setup:**

```cypher
// Create graph projection for PageRank
CALL gds.graph.project(
  'influence-graph',
  'User',
  {
    FOLLOWS: {
      orientation: 'NATURAL',
      properties: ['strength']
    }
  },
  {
    nodeProperties: ['followerCount', 'influenceScore']
  }
);
```

**Run PageRank:**

```cypher
// Calculate PageRank with relationship weighting
CALL gds.pageRank.stream('influence-graph', {
  maxIterations: 20,
  dampingFactor: 0.85,
  relationshipWeightProperty: 'strength'
})
YIELD nodeId, score
WITH gds.util.asNode(nodeId) AS user, score
RETURN 
  user.id AS userId,
  user.handle AS handle,
  user.followerCount AS followers,
  score AS pageRankScore
ORDER BY score DESC
LIMIT 100;
```

**Write results back:**

```cypher
CALL gds.pageRank.write('influence-graph', {
  writeProperty: 'pageRankScore',
  maxIterations: 20,
  dampingFactor: 0.85,
  relationshipWeightProperty: 'strength'
});
```

**Performance Target:**
- 100K users: < 5 seconds
- 1M users: < 30 seconds
- 10M users: < 5 minutes (consider sampling)

**Validation:**
- Top influencers should align with high follower counts
- Score distribution should follow power law
- Verify results against known influencer rankings

### 2. Community Detection (Louvain Algorithm)

**Setup:**

```cypher
CALL gds.graph.project(
  'community-graph',
  'User',
  {
    FOLLOWS: {
      orientation: 'UNDIRECTED',
      aggregation: 'SINGLE'
    }
  }
);
```

**Run Louvain:**

```cypher
CALL gds.louvain.stream('community-graph', {
  maxLevels: 10,
  maxIterations: 10,
  tolerance: 0.0001,
  includeIntermediateCommunities: false
})
YIELD nodeId, communityId, intermediateCommunityIds
WITH gds.util.asNode(nodeId) AS user, communityId
RETURN 
  communityId,
  count(user) AS memberCount,
  collect(user.handle)[0..10] AS sampleMembers
ORDER BY memberCount DESC
LIMIT 20;
```

**Write communities:**

```cypher
CALL gds.louvain.write('community-graph', {
  writeProperty: 'communityId',
  maxLevels: 10
})
YIELD communityCount, modularity, ranLevels;
```

**Performance Target:**
- 100K users: < 10 seconds
- 1M users: < 60 seconds

**Validation:**
- Modularity score should be > 0.3 (good community structure)
- Communities should have meaningful semantic groupings
- Verify against known social clusters

### 3. Betweenness Centrality (Bridge Identification)

**Setup:**

```cypher
CALL gds.graph.project(
  'centrality-graph',
  'User',
  'FOLLOWS'
);
```

**Run Betweenness Centrality:**

```cypher
CALL gds.betweenness.stream('centrality-graph', {
  samplingSize: 1000,  // Sample for performance on large graphs
  samplingSeed: 42
})
YIELD nodeId, score
WITH gds.util.asNode(nodeId) AS user, score
WHERE score > 0
RETURN 
  user.id AS userId,
  user.handle AS handle,
  user.followerCount AS followers,
  score AS betweennessScore
ORDER BY score DESC
LIMIT 50;
```

**Performance Target:**
- 100K users: < 30 seconds (with sampling)
- 1M users: sampling required

**Validation:**
- High betweenness users should bridge different communities
- Verify bridge users connect otherwise disconnected clusters

### 4. Triangle Count & Clustering Coefficient

**Run Triangle Count:**

```cypher
CALL gds.triangleCount.stream('community-graph')
YIELD nodeId, triangleCount
WITH gds.util.asNode(nodeId) AS user, triangleCount
WHERE triangleCount > 0
RETURN 
  user.handle,
  triangleCount,
  user.followerCount
ORDER BY triangleCount DESC
LIMIT 50;
```

**Local Clustering Coefficient:**

```cypher
CALL gds.localClusteringCoefficient.stream('community-graph')
YIELD nodeId, localClusteringCoefficient
WITH gds.util.asNode(nodeId) AS user, localClusteringCoefficient
WHERE localClusteringCoefficient > 0
RETURN 
  user.handle,
  localClusteringCoefficient,
  user.followerCount
ORDER BY localClusteringCoefficient DESC
LIMIT 50;
```

**Validation:**
- High clustering = tight-knit communities
- Compare clustering across different user segments

### 5. Weakly Connected Components

**Identify Isolated Subgraphs:**

```cypher
CALL gds.wcc.stream('community-graph')
YIELD nodeId, componentId
WITH componentId, collect(gds.util.asNode(nodeId)) AS members
WHERE size(members) > 1
RETURN 
  componentId,
  size(members) AS componentSize,
  [m in members | m.handle][0..5] AS sampleMembers
ORDER BY componentSize DESC;
```

**Validation:**
- Expect one large connected component (main network)
- Small components indicate isolated clusters

### 6. Shortest Path & Path Finding

**Single Shortest Path:**

```cypher
MATCH (start:User {id: $userId1}),
      (end:User {id: $userId2}),
      path = shortestPath((start)-[:FOLLOWS*..10]-(end))
RETURN 
  path,
  length(path) AS distance,
  [node in nodes(path) | node.handle] AS pathHandles;
```

**All Shortest Paths:**

```cypher
MATCH (start:User {id: $userId1}),
      (end:User {id: $userId2}),
      paths = allShortestPaths((start)-[:FOLLOWS*..6]-(end))
RETURN 
  count(paths) AS pathCount,
  length(head(collect(paths))) AS distance;
```

**Performance Target:**
- Distance ≤ 6: < 500ms
- Distance > 6: May timeout (limit search depth)

## Performance Testing Procedure

### 1. Generate Test Data

```python
# Python script to generate realistic test data
import uuid
import random
from neo4j import GraphDatabase

def generate_test_network(driver, num_users=10000, avg_follows_per_user=100):
    """Generate realistic social network test data"""
    
    # Create users
    with driver.session() as session:
        user_ids = []
        for i in range(num_users):
            user_id = str(uuid.uuid4())
            session.run("""
                CREATE (u:User {
                    id: $id,
                    handle: $handle,
                    displayName: $name,
                    followerCount: 0,
                    followingCount: 0,
                    influenceScore: 0.5,
                    isVerified: $verified,
                    isSimulated: true,
                    createdAt: datetime(),
                    lastActiveAt: datetime()
                })
            """, id=user_id, 
                 handle=f"@user{i}.sim.test",
                 name=f"Test User {i}",
                 verified=(i < num_users * 0.01))
            user_ids.append(user_id)
        
        # Create follows with preferential attachment (power law)
        for follower_id in user_ids:
            num_follows = min(int(random.expovariate(1.0 / avg_follows_per_user)), 1000)
            
            # More likely to follow popular users (preferential attachment)
            following_ids = random.choices(
                user_ids, 
                k=num_follows,
                weights=[1 + random.random() ** 2 for _ in user_ids]
            )
            
            for following_id in following_ids:
                if follower_id != following_id:
                    session.run("""
                        MATCH (follower:User {id: $follower_id})
                        MATCH (following:User {id: $following_id})
                        MERGE (follower)-[:FOLLOWS {
                            createdAt: datetime(),
                            activeFrom: datetime(),
                            activeTo: NULL,
                            strength: $strength,
                            notificationsEnabled: true
                        }]->(following)
                    """, follower_id=follower_id, 
                         following_id=following_id,
                         strength=random.uniform(0.3, 1.0))
        
        # Update cached counts
        session.run("""
            MATCH (u:User)
            OPTIONAL MATCH (u)<-[:FOLLOWS]-(follower)
            OPTIONAL MATCH (u)-[:FOLLOWS]->(following)
            WITH u, count(DISTINCT follower) AS fc, count(DISTINCT following) AS fgc
            SET u.followerCount = fc, u.followingCount = fgc
        """)

# Usage
import os
driver = GraphDatabase.driver("bolt://localhost:7687", auth=("neo4j", os.getenv("NEO4J_PASSWORD")))
generate_test_network(driver, num_users=100000, avg_follows_per_user=100)
driver.close()
```

### 2. Run Performance Benchmark Suite

```cypher
// Benchmark query suite
// Run with EXPLAIN and PROFILE to analyze

// 1. Follower list (high-profile user)
PROFILE
MATCH (u:User)
WITH u ORDER BY u.followerCount DESC LIMIT 1
MATCH (follower:User)-[:FOLLOWS]->(u)
RETURN count(follower) AS followerCount;

// 2. Following list
PROFILE  
MATCH (u:User)
WITH u ORDER BY u.followerCount DESC LIMIT 1
MATCH (u)-[:FOLLOWS]->(following:User)
RETURN count(following) AS followingCount;

// 3. Mutual follows
PROFILE
MATCH (u:User)
WITH u ORDER BY u.followerCount DESC LIMIT 10
MATCH (u)-[:FOLLOWS]->(f:User)-[:FOLLOWS]->(u)
RETURN u.handle, count(f) AS mutualFollows;

// 4. Second-degree connections
PROFILE
MATCH (u:User {handle: '@user0.sim.test'})
MATCH (u)-[:FOLLOWS]->(:User)-[:FOLLOWS]->(rec:User)
WHERE NOT (u)-[:FOLLOWS]->(rec) AND u <> rec
RETURN count(DISTINCT rec) AS recommendations;

// 5. Shortest path
PROFILE
MATCH (a:User), (b:User)
WHERE a.handle = '@user0.sim.test' AND b.handle = '@user100.sim.test'
MATCH path = shortestPath((a)-[:FOLLOWS*..6]-(b))
RETURN length(path) AS distance;
```

### 3. Validate Results

```cypher
// Data quality checks

// 1. Check for orphaned nodes
MATCH (u:User)
WHERE NOT (u)-[:FOLLOWS]-() AND NOT ()-[:FOLLOWS]->(u)
RETURN count(u) AS orphanedUsers;

// 2. Verify follower count cache
MATCH (u:User)
OPTIONAL MATCH (follower)-[:FOLLOWS]->(u)
WITH u, count(follower) AS actualCount
WHERE u.followerCount <> actualCount
RETURN count(u) AS inconsistentCounts;

// 3. Check for self-follows
MATCH (u:User)-[:FOLLOWS]->(u)
RETURN count(u) AS selfFollows;  // Should be 0

// 4. Verify relationship properties
MATCH ()-[f:FOLLOWS]->()
WHERE f.createdAt IS NULL OR f.strength IS NULL
RETURN count(f) AS invalidRelationships;
```

## Graph Algorithm Validation Checklist

- [ ] PageRank produces sensible influence scores
- [ ] Top PageRank users correlate with high follower counts
- [ ] Community detection finds meaningful clusters
- [ ] Modularity score > 0.3 indicates good community structure
- [ ] Betweenness centrality identifies bridge users
- [ ] Triangle count reflects network density
- [ ] Shortest path queries complete in < 500ms for distance ≤ 6
- [ ] WCC identifies main network component
- [ ] All graph projections can be created without errors
- [ ] Algorithm results are reproducible with same seed

## Optimization Recommendations

### When Queries Are Slow

1. **Check indexes**: Ensure all ID and handle lookups use indexes
2. **Limit depth**: Cap variable-length patterns at 6 hops maximum
3. **Use LIMIT**: Always limit results, especially for recommendations
4. **Filter early**: Put WHERE clauses before expensive operations
5. **Denormalize**: Cache expensive aggregations (follower counts, etc.)
6. **Sample**: For very large graphs, use sampling in algorithms
7. **Batch**: Use UNWIND for bulk operations
8. **Profile**: Always PROFILE slow queries to identify bottlenecks

### When Algorithms Are Slow

1. **Use sampling**: For betweenness, use `samplingSize` parameter
2. **Incremental updates**: Update PageRank incrementally instead of full recalculation
3. **Caching**: Store algorithm results and refresh periodically
4. **Distributed**: For 10M+ nodes, consider Neo4j Fabric for distributed processing

## Next Steps

After validation:
1. Update ROADMAP.md to mark graph validation tasks complete
2. Document any performance bottlenecks found
3. Create optimization plan for slow queries
4. Prepare for PostgreSQL schema validation
