# PostgreSQL Performance Validation & Query Optimization

This document outlines the indexing strategy, query patterns, and performance validation for SocialSim's PostgreSQL database.

## Index Strategy Review

### Current Index Coverage

Based on the schema in `postgresql-schema.sql`, we have the following indexes:

#### Users Table
```sql
CREATE INDEX idx_users_handle ON users(handle);              -- Unique lookups
CREATE INDEX idx_users_email ON users(email);                -- Authentication
CREATE INDEX idx_users_is_active ON users(is_active);        -- Filtering
CREATE INDEX idx_users_created_at ON users(created_at DESC); -- Ordering
CREATE INDEX idx_users_follower_count ON users(follower_count DESC); -- Top users
CREATE INDEX idx_users_last_active ON users(last_active_at DESC);    -- Activity tracking
```

**Coverage Analysis:** ✅ Good
- Covers primary lookup patterns (handle, email)
- Supports sorting by popularity and activity
- Missing: Composite index for common filters

**Recommended Additions:**
```sql
-- Composite index for active user queries with sorting
CREATE INDEX idx_users_active_followers ON users(is_active, follower_count DESC) 
WHERE is_active = TRUE AND deleted_at IS NULL;

-- Index for simulated agents
CREATE INDEX idx_users_simulated ON users(is_simulated, created_at DESC);

-- Partial index for verified users
CREATE INDEX idx_users_verified ON users(follower_count DESC) 
WHERE is_verified = TRUE AND is_active = TRUE;
```

#### Posts Table
```sql
CREATE INDEX idx_posts_author ON posts(author_id, created_at DESC);
CREATE INDEX idx_posts_thread ON posts(thread_id, created_at ASC);
CREATE INDEX idx_posts_created_at ON posts(created_at DESC);
CREATE INDEX idx_posts_engagement ON posts(engagement_score DESC, created_at DESC);
CREATE INDEX idx_posts_visibility ON posts(visibility);
CREATE INDEX idx_posts_reply_to ON posts(reply_to_post_id);
CREATE INDEX idx_posts_quoted ON posts(quoted_post_id);
CREATE INDEX idx_posts_content_fts ON posts USING GIN (to_tsvector('english', content));
```

**Coverage Analysis:** ✅ Excellent
- Covers timeline queries (author + time)
- Supports thread reconstruction
- Full-text search enabled
- Engagement-based discovery

**Recommended Additions:**
```sql
-- Composite index for feed generation (visibility + time)
CREATE INDEX idx_posts_public_feed ON posts(visibility, created_at DESC)
WHERE is_deleted = FALSE AND visibility = 'Public';

-- Index for reply chains
CREATE INDEX idx_posts_reply_chain ON posts(reply_to_post_id, created_at ASC)
WHERE is_reply = TRUE;

-- Index for quote tracking
CREATE INDEX idx_posts_quote_cascade ON posts(quoted_post_id, created_at DESC)
WHERE quoted_post_id IS NOT NULL;

-- Covering index for post list queries
CREATE INDEX idx_posts_feed_covering ON posts(
  visibility, created_at DESC, author_id, like_count, repost_count
) INCLUDE (content) WHERE is_deleted = FALSE;
```

#### Follows Table
```sql
CREATE INDEX idx_follows_follower ON follows(follower_id, created_at DESC);
CREATE INDEX idx_follows_following ON follows(following_id, created_at DESC);
CREATE INDEX idx_follows_status ON follows(status);
CREATE INDEX idx_follows_active ON follows(follower_id, following_id) 
WHERE status = 'Active';
```

**Coverage Analysis:** ✅ Good
- Covers relationship queries in both directions
- Partial index for active follows is excellent

**Recommended Additions:**
```sql
-- Index for mutual follow queries (reverse direction for efficient reciprocal lookups)
CREATE INDEX idx_follows_mutual ON follows(following_id, follower_id)
WHERE status = 'Active';
```

#### Likes Table
```sql
CREATE INDEX idx_likes_user ON likes(user_id, created_at DESC);
CREATE INDEX idx_likes_post ON likes(post_id, created_at DESC);
```

**Coverage Analysis:** ✅ Good

**Recommended Additions:**
```sql
-- Covering index for like lists
CREATE INDEX idx_likes_post_covering ON likes(post_id, user_id, created_at DESC, reaction_type);

-- Index for user's liked posts with post data
CREATE INDEX idx_likes_user_activity ON likes(user_id, created_at DESC)
INCLUDE (post_id, reaction_type);
```

#### Threads Table
```sql
CREATE INDEX idx_threads_last_activity ON threads(last_activity_at DESC);
CREATE INDEX idx_threads_post_count ON threads(post_count DESC);
```

**Coverage Analysis:** ✅ Good

**Recommended Additions:**
```sql
-- Composite for active thread discovery
CREATE INDEX idx_threads_active ON threads(last_activity_at DESC, post_count DESC)
WHERE is_locked = FALSE;
```

### Index Maintenance Strategy

```sql
-- Monitor index usage
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan AS index_scans,
    idx_tup_read AS tuples_read,
    idx_tup_fetch AS tuples_fetched,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY idx_scan ASC;

-- Find unused indexes (candidates for removal)
SELECT 
    schemaname || '.' || tablename AS table,
    indexname AS index,
    pg_size_pretty(pg_relation_size(indexrelid)) AS size,
    idx_scan AS scans
FROM pg_stat_user_indexes
WHERE schemaname = 'public' AND idx_scan = 0
ORDER BY pg_relation_size(indexrelid) DESC;

-- Index bloat check
SELECT 
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) AS size,
    idx_scan,
    round(100 * (pg_relation_size(indexrelid)::numeric / 
        NULLIF(pg_relation_size(tablename::regclass), 0)), 2) AS index_to_table_ratio
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY pg_relation_size(indexrelid) DESC;
```

## Sample Query Validation

### 1. User Timeline Query

**Query: Get user's posts**
```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT 
    p.id,
    p.content,
    p.created_at,
    p.like_count,
    p.repost_count,
    p.reply_count,
    u.handle,
    u.display_name,
    u.avatar_url
FROM posts p
JOIN users u ON p.author_id = u.id
WHERE p.author_id = 'user-uuid-here'
  AND p.is_deleted = FALSE
ORDER BY p.created_at DESC
LIMIT 50;
```

**Expected Plan:**
- Index Scan on `idx_posts_author`
- Nested Loop join to users (should be fast with PK lookup)
- Execution time: < 10ms

**Validation:**
- [ ] Uses index on posts(author_id, created_at DESC)
- [ ] No sequential scans
- [ ] Buffers hit (not read from disk)
- [ ] Execution time < 10ms for cached data

### 2. Home Feed Query

**Query: Get timeline from followed users**
```sql
EXPLAIN (ANALYZE, BUFFERS)
WITH followed_users AS (
    SELECT following_id
    FROM follows
    WHERE follower_id = 'user-uuid-here'
      AND status = 'Active'
)
SELECT 
    p.id,
    p.content,
    p.created_at,
    p.like_count,
    p.repost_count,
    p.reply_count,
    p.visibility,
    u.id AS author_id,
    u.handle,
    u.display_name,
    u.avatar_url,
    EXISTS(
        SELECT 1 FROM likes l 
        WHERE l.post_id = p.id AND l.user_id = 'user-uuid-here'
    ) AS is_liked_by_user
FROM posts p
JOIN users u ON p.author_id = u.id
WHERE p.author_id IN (SELECT following_id FROM followed_users)
  AND p.is_deleted = FALSE
  AND p.visibility IN ('Public', 'FollowersOnly')
ORDER BY p.created_at DESC
LIMIT 50;
```

**Expected Plan:**
- CTE scan for followed users (index on follows)
- Nested Loop or Hash Join to posts
- Execution time: < 50ms for 500 followed users

**Performance Targets:**
- Following 100 users: < 30ms
- Following 500 users: < 50ms
- Following 1000+ users: < 100ms (may need optimization)

**Optimization for Large Following Lists:**
```sql
-- Alternative: Union strategy for better performance
(
    SELECT p.*, u.handle, u.display_name, u.avatar_url
    FROM posts p
    JOIN users u ON p.author_id = u.id
    WHERE p.author_id = ANY(ARRAY(
        SELECT following_id FROM follows 
        WHERE follower_id = 'user-uuid' AND status = 'Active'
    ))
    AND p.is_deleted = FALSE
    AND p.created_at > NOW() - INTERVAL '7 days'
    ORDER BY p.created_at DESC
    LIMIT 50
)
UNION ALL
(
    -- Older posts if needed
    SELECT p.*, u.handle, u.display_name, u.avatar_url
    FROM posts p
    JOIN users u ON p.author_id = u.id
    WHERE p.author_id = ANY(ARRAY(
        SELECT following_id FROM follows 
        WHERE follower_id = 'user-uuid' AND status = 'Active'
    ))
    AND p.is_deleted = FALSE
    AND p.created_at <= NOW() - INTERVAL '7 days'
    ORDER BY p.created_at DESC
    LIMIT 10
)
ORDER BY created_at DESC
LIMIT 50;
```

### 3. Thread Reconstruction Query

**Query: Get entire thread**
```sql
EXPLAIN (ANALYZE, BUFFERS)
WITH RECURSIVE thread_posts AS (
    -- Root post
    SELECT 
        p.*,
        u.handle,
        u.display_name,
        u.avatar_url,
        1 AS depth,
        ARRAY[p.id] AS path
    FROM posts p
    JOIN users u ON p.author_id = u.id
    WHERE p.id = 'root-post-uuid'
    
    UNION ALL
    
    -- Recursive: replies to posts in thread
    SELECT 
        p.*,
        u.handle,
        u.display_name,
        u.avatar_url,
        tp.depth + 1,
        tp.path || p.id
    FROM posts p
    JOIN users u ON p.author_id = u.id
    JOIN thread_posts tp ON p.reply_to_post_id = tp.id
    WHERE p.is_deleted = FALSE
      AND NOT p.id = ANY(tp.path)  -- Prevent cycles
      AND tp.depth < 20  -- Limit depth
)
SELECT * FROM thread_posts
ORDER BY path;
```

**Expected Plan:**
- Recursive CTE with proper termination
- Index usage on reply_to_post_id
- Execution time: < 100ms for 100-post thread

**Validation:**
- [ ] Recursion terminates properly
- [ ] Uses index on posts(reply_to_post_id)
- [ ] No Cartesian products
- [ ] Handles cycles safely

### 4. Engagement Query (Who Liked a Post)

**Query: Get post likers**
```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT 
    u.id,
    u.handle,
    u.display_name,
    u.avatar_url,
    u.is_verified,
    u.follower_count,
    l.reaction_type,
    l.created_at AS liked_at,
    EXISTS(
        SELECT 1 FROM follows f 
        WHERE f.follower_id = 'current-user-uuid' 
          AND f.following_id = u.id 
          AND f.status = 'Active'
    ) AS is_followed_by_current_user
FROM likes l
JOIN users u ON l.user_id = u.id
WHERE l.post_id = 'post-uuid-here'
ORDER BY l.created_at DESC
LIMIT 100;
```

**Expected Plan:**
- Index Scan on likes(post_id, created_at DESC)
- Nested Loop to users
- Execution time: < 20ms

### 5. Search Query (Full-Text)

**Query: Search posts**
```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT 
    p.id,
    p.content,
    p.created_at,
    p.like_count,
    p.engagement_score,
    u.handle,
    u.display_name,
    ts_rank(to_tsvector('english', p.content), plainto_tsquery('english', 'search term')) AS rank
FROM posts p
JOIN users u ON p.author_id = u.id
WHERE to_tsvector('english', p.content) @@ plainto_tsquery('english', 'search term')
  AND p.is_deleted = FALSE
  AND p.visibility = 'Public'
ORDER BY rank DESC, p.created_at DESC
LIMIT 50;
```

**Expected Plan:**
- Bitmap Index Scan on GIN index
- Execution time: < 100ms

**Optimization:**
```sql
-- Materialized tsvector column for better performance
ALTER TABLE posts ADD COLUMN content_tsv tsvector;

CREATE INDEX idx_posts_content_tsv ON posts USING GIN (content_tsv);

-- Trigger to maintain it
CREATE OR REPLACE FUNCTION posts_content_tsv_trigger() 
RETURNS trigger AS $$
BEGIN
    NEW.content_tsv := to_tsvector('english', COALESCE(NEW.content, ''));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER posts_content_tsv_update 
BEFORE INSERT OR UPDATE ON posts
FOR EACH ROW EXECUTE FUNCTION posts_content_tsv_trigger();

-- Updated query
SELECT p.id, p.content, p.created_at, u.handle, u.display_name,
       ts_rank(p.content_tsv, query) AS rank
FROM posts p
JOIN users u ON p.author_id = u.id,
     plainto_tsquery('english', 'search term') query
WHERE p.content_tsv @@ query
  AND p.is_deleted = FALSE
  AND p.visibility = 'Public'
ORDER BY rank DESC, p.created_at DESC
LIMIT 50;
```

### 6. Trending Posts Query

**Query: Get trending posts (last 24 hours)**
```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT 
    p.id,
    p.content,
    p.created_at,
    p.like_count,
    p.repost_count,
    p.reply_count,
    p.engagement_score,
    u.handle,
    u.display_name,
    -- Calculate trending score
    (p.like_count * 1.0 + p.repost_count * 2.0 + p.reply_count * 1.5) / 
    (EXTRACT(EPOCH FROM (NOW() - p.created_at)) / 3600.0 + 2.0) ^ 1.5 AS trending_score
FROM posts p
JOIN users u ON p.author_id = u.id
WHERE p.created_at > NOW() - INTERVAL '24 hours'
  AND p.is_deleted = FALSE
  AND p.visibility = 'Public'
ORDER BY trending_score DESC
LIMIT 50;
```

**Expected Plan:**
- Index Scan on posts(created_at DESC) with filter
- Execution time: < 100ms

**Optimization with Materialized View:**
```sql
CREATE MATERIALIZED VIEW trending_posts AS
SELECT 
    p.id,
    p.content,
    p.created_at,
    p.like_count,
    p.repost_count,
    p.reply_count,
    p.author_id,
    (p.like_count * 1.0 + p.repost_count * 2.0 + p.reply_count * 1.5) / 
    (EXTRACT(EPOCH FROM (NOW() - p.created_at)) / 3600.0 + 2.0) ^ 1.5 AS trending_score
FROM posts p
WHERE p.created_at > NOW() - INTERVAL '24 hours'
  AND p.is_deleted = FALSE
  AND p.visibility = 'Public'
ORDER BY trending_score DESC;

CREATE INDEX idx_trending_score ON trending_posts(trending_score DESC);

-- Refresh every 5 minutes (schedule with pg_cron or app)
REFRESH MATERIALIZED VIEW CONCURRENTLY trending_posts;
```

### 7. User Discovery Query

**Query: Find users to follow**
```sql
EXPLAIN (ANALYZE, BUFFERS)
WITH user_context AS (
    -- Users current user already follows
    SELECT following_id AS user_id
    FROM follows
    WHERE follower_id = 'current-user-uuid' 
      AND status = 'Active'
),
candidate_users AS (
    -- Popular users not yet followed
    SELECT DISTINCT u.id, u.handle, u.display_name, u.avatar_url, 
           u.follower_count, u.post_count, u.bio
    FROM users u
    WHERE u.is_active = TRUE
      AND u.deleted_at IS NULL
      AND u.id NOT IN (SELECT user_id FROM user_context)
      AND u.id != 'current-user-uuid'
      AND u.follower_count > 100  -- Minimum threshold
    ORDER BY u.follower_count DESC
    LIMIT 100
)
SELECT * FROM candidate_users
ORDER BY follower_count DESC
LIMIT 20;
```

**Advanced Version (Collaborative Filtering):**
```sql
-- Find users followed by people you follow
WITH user_follows AS (
    SELECT following_id 
    FROM follows 
    WHERE follower_id = 'current-user-uuid' AND status = 'Active'
),
second_degree AS (
    SELECT 
        f.following_id AS recommended_user_id,
        COUNT(*) AS common_follows,
        MAX(u.follower_count) AS follower_count
    FROM follows f
    JOIN users u ON f.following_id = u.id
    WHERE f.follower_id IN (SELECT following_id FROM user_follows)
      AND f.following_id NOT IN (SELECT following_id FROM user_follows)
      AND f.following_id != 'current-user-uuid'
      AND f.status = 'Active'
      AND u.is_active = TRUE
    GROUP BY f.following_id
    HAVING COUNT(*) >= 2  -- At least 2 common connections
)
SELECT 
    u.id,
    u.handle,
    u.display_name,
    u.avatar_url,
    u.bio,
    u.follower_count,
    sd.common_follows
FROM second_degree sd
JOIN users u ON sd.recommended_user_id = u.id
ORDER BY sd.common_follows DESC, u.follower_count DESC
LIMIT 20;
```

## Database Constraint Testing

### Test Cases for Constraints

```sql
-- Test 1: Prevent self-follow
BEGIN;
INSERT INTO follows (follower_id, following_id, status)
VALUES ('user-uuid', 'user-uuid', 'Active');
-- Should fail with: chk_no_self_follow
ROLLBACK;

-- Test 2: Prevent duplicate follows
BEGIN;
INSERT INTO follows (follower_id, following_id, status)
VALUES ('user1-uuid', 'user2-uuid', 'Active');
INSERT INTO follows (follower_id, following_id, status)
VALUES ('user1-uuid', 'user2-uuid', 'Active');
-- Should fail with: uq_follow
ROLLBACK;

-- Test 3: Prevent negative counts
BEGIN;
UPDATE users SET follower_count = -5 WHERE id = 'user-uuid';
-- Should fail with: chk_follower_count
ROLLBACK;

-- Test 4: Cascade delete (user deletion removes posts)
BEGIN;
SELECT COUNT(*) FROM posts WHERE author_id = 'user-uuid';
DELETE FROM users WHERE id = 'user-uuid';
SELECT COUNT(*) FROM posts WHERE author_id = 'user-uuid';
-- Should be 0 after delete
ROLLBACK;

-- Test 5: Referential integrity (can't like non-existent post)
BEGIN;
INSERT INTO likes (user_id, post_id)
VALUES ('user-uuid', 'non-existent-post-uuid');
-- Should fail with FK violation
ROLLBACK;

-- Test 6: Reply consistency check
BEGIN;
INSERT INTO posts (author_id, content, is_reply, reply_to_post_id)
VALUES ('user-uuid', 'Test', TRUE, NULL);
-- Should fail with: chk_reply_consistency
ROLLBACK;

-- Test 7: Report target check (must report user OR post, not both)
BEGIN;
INSERT INTO reports (reporter_id, reported_user_id, reported_post_id, reason)
VALUES ('user1-uuid', 'user2-uuid', 'post-uuid', 'Spam');
-- Should fail with: chk_report_target
ROLLBACK;
```

### Trigger Testing

```sql
-- Test updated_at trigger
BEGIN;
SELECT updated_at FROM users WHERE id = 'user-uuid';
-- Wait 1 second
SELECT pg_sleep(1);
UPDATE users SET bio = 'New bio' WHERE id = 'user-uuid';
SELECT updated_at FROM users WHERE id = 'user-uuid';
-- updated_at should be > initial value
ROLLBACK;

-- Test content_tsv trigger (if implemented)
BEGIN;
INSERT INTO posts (author_id, content)
VALUES ('user-uuid', 'Hello world testing search');
SELECT content_tsv FROM posts WHERE content = 'Hello world testing search';
-- content_tsv should contain 'hello', 'world', 'test', 'search'
ROLLBACK;
```

## Performance Benchmarking

### Benchmark Test Data

```sql
-- Generate test data for benchmarking
-- 1M users, 10M posts, 50M follows

DO $$
DECLARE
    i INTEGER;
BEGIN
    -- Insert users
    FOR i IN 1..1000000 LOOP
        INSERT INTO users (
            decentralized_id, handle, display_name, 
            is_simulated, follower_count, following_count
        )
        VALUES (
            'did:plc:sim' || i,
            '@user' || i || '.sim.test',
            'Test User ' || i,
            TRUE,
            (random() * 10000)::INTEGER,
            (random() * 1000)::INTEGER
        );
        
        IF i % 10000 = 0 THEN
            RAISE NOTICE 'Inserted % users', i;
        END IF;
    END LOOP;
END $$;
```

### Query Performance Targets

| Query Type | Target (p50) | Target (p99) | Max Acceptable |
|------------|--------------|--------------|----------------|
| User profile lookup | < 5ms | < 10ms | 50ms |
| User timeline (50 posts) | < 10ms | < 30ms | 100ms |
| Home feed (500 follows) | < 50ms | < 100ms | 500ms |
| Thread reconstruction | < 50ms | < 150ms | 500ms |
| Post likers list | < 20ms | < 50ms | 200ms |
| Full-text search | < 100ms | < 300ms | 1s |
| Trending posts | < 50ms | < 100ms | 300ms |
| User recommendations | < 200ms | < 500ms | 2s |

## Validation Checklist

### Index Validation
- [x] All primary keys have indexes
- [x] Foreign keys have indexes
- [x] Commonly filtered columns have indexes
- [x] Commonly sorted columns have indexes
- [ ] Composite indexes for common query patterns (needs additions)
- [ ] Partial indexes for filtered queries (needs additions)
- [ ] GIN indexes for full-text and JSONB (implemented for posts)
- [ ] BRIN indexes for large time-series tables (if needed)

### Query Validation
- [ ] All critical queries use indexes (not seq scans)
- [ ] Join queries use appropriate join types
- [ ] No Cartesian products in query plans
- [ ] CTEs don't cause optimization barriers
- [ ] Recursive queries terminate properly
- [ ] All queries meet performance targets

### Constraint Validation
- [ ] All FK constraints tested
- [ ] All CHECK constraints tested
- [ ] All UNIQUE constraints tested
- [ ] Cascade deletes work correctly
- [ ] Triggers fire correctly
- [ ] No orphaned records after operations

### Data Integrity
- [ ] No NULL values where NOT NULL specified
- [ ] All counts (follower_count, etc.) are consistent
- [ ] All timestamps are populated
- [ ] Soft deletes (deleted_at) work correctly
- [ ] JSONB fields have valid JSON

## Next Steps

1. Implement recommended additional indexes
2. Run benchmark suite with realistic data volumes
3. Profile slow queries and optimize
4. Test all constraints and triggers
5. Document any schema changes needed
6. Update ROADMAP.md to mark tasks complete
