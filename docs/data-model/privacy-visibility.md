# Privacy & Visibility Rules

This document defines content visibility levels, access control, and privacy mechanisms.

## Design Principles

1. **Privacy by Default**: Users control who sees their content
2. **Granular Control**: Fine-grained visibility settings
3. **Consistent Enforcement**: Visibility checked at query time and write time
4. **Performance**: Visibility checks must be fast (indexing, caching)
5. **Composable Rules**: Multiple privacy rules can apply simultaneously

---

## 1. Visibility Levels

The Post `Visibility` field controls who can see the content.

### Enum: `ContentVisibility`

| Value | Description | Who Can See |
|-------|-------------|-------------|
| `Public` | Publicly visible | Everyone, including non-followers and non-authenticated users |
| `FollowersOnly` | Followers only | Users who follow the author |
| `Private` | Private/Direct | Only mentioned users and author |
| `Mentions` | Mentioned only | Only users explicitly mentioned in content |
| `CircleOnly` | Close friends/circle | Only users in author's designated circle |

### Default Visibility

User can set default visibility in account settings:

```sql
CREATE TABLE UserSettings (
    UserId UUID PRIMARY KEY,
    DefaultPostVisibility ContentVisibility NOT NULL DEFAULT 'Public',
    AllowFollowRequests BOOLEAN NOT NULL DEFAULT true,
    AllowMentions BOOLEAN NOT NULL DEFAULT true,
    AllowQuotes BOOLEAN NOT NULL DEFAULT true,
    AllowReposts BOOLEAN NOT NULL DEFAULT true,
    ...
);
```

---

## 2. Account Privacy Settings

User accounts can be public or private.

### User Fields for Privacy

| Field | Type | Description |
|-------|------|-------------|
| `IsPrivate` | BOOLEAN | Private account (requires follow approval) |
| `AllowDirectMessages` | ENUM | All, FollowersOnly, MutualFollows, None |
| `AllowMentions` | ENUM | All, FollowersOnly, None |
| `AllowTagging` | ENUM | All, FollowersOnly, None |
| `ShowFollowers` | BOOLEAN | Public follower list |
| `ShowFollowing` | BOOLEAN | Public following list |
| `ShowActivity` | BOOLEAN | Show "liked" and "reposted" in followers' feeds |

### Private Account Rules

When `IsPrivate = true`:

1. Followers must be approved (Follow.Status = 'Pending' until approved)
2. Non-followers cannot see posts (except Public posts, if allowed)
3. Non-followers cannot see follower/following lists
4. Profile visible but limited (no post count, no recent activity)

---

## 3. Content Visibility Matrix

| Viewer Type | Public | FollowersOnly | Private | Mentions | CircleOnly |
|-------------|--------|---------------|---------|----------|------------|
| **Author** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Follower** | ✅ | ✅ | ❌ | If mentioned | If in circle |
| **Mutual Follow** | ✅ | ✅ | ❌ | If mentioned | If in circle |
| **Non-Follower** | ✅ | ❌ | ❌ | If mentioned | ❌ |
| **Blocked User** | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Anonymous** | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## 4. Visibility Query Enforcement

### SQL Visibility Filter

Apply visibility filter to all post queries:

```sql
-- PostgreSQL function to check visibility
CREATE FUNCTION can_view_post(
    p_post_id UUID,
    p_viewer_id UUID  -- NULL for anonymous
) RETURNS BOOLEAN AS $$
DECLARE
    v_post RECORD;
    v_is_follower BOOLEAN;
    v_is_mentioned BOOLEAN;
    v_is_blocked BOOLEAN;
    v_is_in_circle BOOLEAN;
BEGIN
    -- Get post details
    SELECT p.AuthorId, p.Visibility, u.IsPrivate
    INTO v_post
    FROM Posts p
    JOIN Users u ON p.AuthorId = u.Id
    WHERE p.Id = p_post_id;
    
    -- Author can always see own posts
    IF v_post.AuthorId = p_viewer_id THEN
        RETURN true;
    END IF;
    
    -- Check if viewer is blocked
    IF EXISTS (
        SELECT 1 FROM Blocks 
        WHERE BlockerId = v_post.AuthorId AND BlockedId = p_viewer_id
    ) THEN
        RETURN false;
    END IF;
    
    -- Public posts visible to all (unless author is private)
    IF v_post.Visibility = 'Public' AND NOT v_post.IsPrivate THEN
        RETURN true;
    END IF;
    
    -- Authenticated checks
    IF p_viewer_id IS NULL THEN
        RETURN false;  -- Non-public posts require authentication
    END IF;
    
    -- FollowersOnly: Check if viewer follows author
    IF v_post.Visibility = 'FollowersOnly' THEN
        SELECT EXISTS (
            SELECT 1 FROM Follows 
            WHERE FollowerId = p_viewer_id 
              AND FollowingId = v_post.AuthorId
              AND Status = 'Active'
        ) INTO v_is_follower;
        
        RETURN v_is_follower;
    END IF;
    
    -- Mentions: Check if viewer is mentioned
    IF v_post.Visibility = 'Mentions' OR v_post.Visibility = 'Private' THEN
        SELECT EXISTS (
            SELECT 1 FROM Mentions 
            WHERE PostId = p_post_id AND MentionedId = p_viewer_id
        ) INTO v_is_mentioned;
        
        RETURN v_is_mentioned;
    END IF;
    
    -- CircleOnly: Check if viewer in author's circle
    IF v_post.Visibility = 'CircleOnly' THEN
        SELECT EXISTS (
            SELECT 1 FROM CircleMembers 
            WHERE CircleOwnerId = v_post.AuthorId AND MemberId = p_viewer_id
        ) INTO v_is_in_circle;
        
        RETURN v_is_in_circle;
    END IF;
    
    -- Default deny
    RETURN false;
END;
$$ LANGUAGE plpgsql;
```

### Usage in Queries

```sql
-- Get visible posts for user
SELECT p.*
FROM Posts p
WHERE can_view_post(p.Id, $viewerId)
  AND p.IsDeleted = false
ORDER BY p.CreatedAt DESC
LIMIT 50;
```

### Performance Optimization

For performance, use **denormalized visibility checks** in hot paths:

1. Pre-filter by visibility level
2. Join with Follows table when needed
3. Cache follower relationships in Redis
4. Use materialized views for complex visibility

```sql
-- Optimized query: Get public posts (no visibility check needed)
SELECT p.* FROM Posts p
JOIN Users u ON p.AuthorId = u.Id
WHERE p.Visibility = 'Public'
  AND u.IsPrivate = false
  AND p.IsDeleted = false
ORDER BY p.CreatedAt DESC
LIMIT 50;

-- Optimized query: Get posts from followed users
SELECT p.* FROM Posts p
JOIN Follows f ON p.AuthorId = f.FollowingId
WHERE f.FollowerId = $userId
  AND f.Status = 'Active'
  AND p.Visibility IN ('Public', 'FollowersOnly')
  AND p.IsDeleted = false
  AND NOT EXISTS (
      SELECT 1 FROM Blocks b 
      WHERE b.BlockerId = p.AuthorId AND b.BlockedId = $userId
  )
ORDER BY p.CreatedAt DESC
LIMIT 50;
```

---

## 5. Close Friends / Circles

Users can create "circles" for granular content sharing.

### PostgreSQL Table: `Circles`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Circle identifier |
| `OwnerId` | UUID | FK→User.Id, NOT NULL | Circle creator |
| `Name` | VARCHAR(128) | NOT NULL | Circle name (e.g., "Close Friends") |
| `Description` | TEXT | NULLABLE | Optional description |
| `Color` | VARCHAR(7) | NULLABLE | UI color |
| `MemberCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached member count |
| `CreatedAt` | TIMESTAMP | NOT NULL | Creation timestamp |
| `UpdatedAt` | TIMESTAMP | NOT NULL | Last update |

### PostgreSQL Table: `CircleMembers`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `CircleId` | UUID | FK→Circle.Id, NOT NULL | Circle |
| `MemberId` | UUID | FK→User.Id, NOT NULL | Member in circle |
| `AddedAt` | TIMESTAMP | NOT NULL | When added to circle |

### Indexes

- PRIMARY KEY (Circles): `Id`
- INDEX: `OwnerId, CreatedAt DESC`
- PRIMARY KEY (CircleMembers): `(CircleId, MemberId)`
- INDEX: `MemberId` (for reverse lookup)

### Business Rules

1. Only circle owner can add/remove members
2. Circle members not notified of membership
3. Posts with `Visibility = CircleOnly` specify CircleId (or default circle)
4. User can have multiple circles

---

## 6. Blocking & Muting Effects

### Blocking Effects on Visibility

When User A blocks User B:

| Content | Visibility to B |
|---------|-----------------|
| A's posts | Hidden (all visibility levels) |
| A's profile | Limited (bio, avatar only) |
| A's replies to others | Hidden |
| A's likes/reposts | Hidden from B's feed |
| Mentions of B by A | Not shown to B |
| B's mentions of A | Prevented (cannot mention) |

### Muting Effects on Visibility

When User A mutes User B:

| Content | Visibility to A |
|---------|-----------------|
| B's posts | Hidden from A's feed (based on MuteType) |
| B's replies | Hidden or shown based on MuteType |
| B's reposts | Hidden or shown based on MuteType |
| Mentions from B | Still delivered (mute ≠ block) |
| B's profile | Still accessible |

---

## 7. Reply Visibility Inheritance

Replies inherit visibility constraints from parent post.

### Reply Visibility Rules

| Parent Visibility | Reply Visibility | Who Can See Reply |
|-------------------|------------------|-------------------|
| Public | Public, FollowersOnly | Based on reply visibility |
| FollowersOnly | FollowersOnly, Private | Followers of reply author (if also following parent author) |
| Private | Private | Only mentioned users |
| Mentions | Mentions, Private | Only mentioned users |

### Validation Logic

```csharp
public async Task<bool> ValidateReplyVisibility(
    Guid parentPostId, 
    ContentVisibility replyVisibility)
{
    var parentPost = await _postRepo.GetByIdAsync(parentPostId);
    
    // Reply cannot be more public than parent
    var visibilityHierarchy = new[] {
        ContentVisibility.Private,
        ContentVisibility.Mentions,
        ContentVisibility.CircleOnly,
        ContentVisibility.FollowersOnly,
        ContentVisibility.Public
    };
    
    var parentIndex = Array.IndexOf(visibilityHierarchy, parentPost.Visibility);
    var replyIndex = Array.IndexOf(visibilityHierarchy, replyVisibility);
    
    if (replyIndex > parentIndex)
    {
        throw new InvalidOperationException(
            "Reply visibility cannot be more public than parent post");
    }
    
    return true;
}
```

---

## 8. Quote & Repost Visibility

### Quote Visibility

Quotes can reference posts with any visibility, but:

1. Quote visibility independent of quoted post
2. Quoted post embedded based on viewer's access
3. If viewer cannot see quoted post, shows "[Post not available]"

### Repost Visibility

Reposts inherit original post's visibility:

1. Repost visibility = Original visibility
2. Cannot make private post public via repost
3. Repost author's followers see it (if they can see original)

---

## 9. Search & Discovery Privacy

### Search Visibility

| Content Type | Public Account | Private Account |
|--------------|----------------|-----------------|
| Posts | Public posts searchable | No posts in search |
| Profile | Searchable | Limited profile shown |
| Username/Handle | Searchable | Searchable |
| Follower/Following Lists | Visible | Hidden |

### Discovery Settings

```sql
CREATE TABLE UserDiscoverySettings (
    UserId UUID PRIMARY KEY,
    AllowProfileInSearch BOOLEAN NOT NULL DEFAULT true,
    AllowPostsInSearch BOOLEAN NOT NULL DEFAULT true,
    AllowInRecommendations BOOLEAN NOT NULL DEFAULT true,
    AllowInPeopleYouMayKnow BOOLEAN NOT NULL DEFAULT true,
    AllowInTrending BOOLEAN NOT NULL DEFAULT true,
    IndexForSearch BOOLEAN NOT NULL DEFAULT true,
    UpdatedAt TIMESTAMP NOT NULL
);
```

---

## 10. Age-Gated & Sensitive Content

### Content Warnings & Filters

```sql
ALTER TABLE Posts ADD COLUMN ContentWarnings VARCHAR(64)[];
ALTER TABLE Posts ADD COLUMN IsSensitive BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE Posts ADD COLUMN AgeRestriction INTEGER;  -- 18, 21, etc.
```

### Content Warning Types

```csharp
public enum ContentWarning
{
    NSFW,           // Not safe for work
    Violence,       // Violent content
    Nudity,         // Nudity/adult content
    Graphic,        // Graphic imagery
    Spoiler,        // Spoiler warning
    Flashing,       // Flashing lights (seizure warning)
    Political,      // Political content
    Medical         // Medical/surgical content
}
```

### User Content Filters

```sql
CREATE TABLE UserContentFilters (
    UserId UUID PRIMARY KEY,
    HideNSFW BOOLEAN NOT NULL DEFAULT false,
    HideViolence BOOLEAN NOT NULL DEFAULT false,
    HideSpoilers BOOLEAN NOT NULL DEFAULT false,
    BlurSensitiveMedia BOOLEAN NOT NULL DEFAULT true,
    RequireContentWarnings BOOLEAN NOT NULL DEFAULT false,
    UpdatedAt TIMESTAMP NOT NULL
);
```

### Visibility Check with Content Warnings

```sql
-- Apply content filters to query
SELECT p.*
FROM Posts p
WHERE can_view_post(p.Id, $userId)
  AND (
      -- User has not filtered this content type
      NOT EXISTS (
          SELECT 1 FROM UserContentFilters ucf
          WHERE ucf.UserId = $userId
            AND (
                (ucf.HideNSFW = true AND 'NSFW' = ANY(p.ContentWarnings))
                OR (ucf.HideViolence = true AND 'Violence' = ANY(p.ContentWarnings))
            )
      )
      -- Or no filters set
      OR NOT EXISTS (SELECT 1 FROM UserContentFilters WHERE UserId = $userId)
  )
ORDER BY p.CreatedAt DESC;
```

---

## 11. Geographic & Language Privacy

### Location-Based Visibility

```sql
ALTER TABLE Posts ADD COLUMN LocationLat DECIMAL(10, 8);
ALTER TABLE Posts ADD COLUMN LocationLng DECIMAL(11, 8);
ALTER TABLE Posts ADD COLUMN LocationName VARCHAR(256);
ALTER TABLE Posts ADD COLUMN AllowLocationView BOOLEAN NOT NULL DEFAULT true;
```

Users can:
- Hide location on all posts
- Show location to followers only
- Show approximate location (city vs exact coords)

### Language-Based Filtering

```sql
CREATE TABLE UserLanguagePreferences (
    UserId UUID NOT NULL,
    LanguageCode VARCHAR(8) NOT NULL,
    Priority INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (UserId, LanguageCode)
);
```

Posts in non-preferred languages can be filtered or deprioritized.

---

## 12. Visibility Events & Audit

All visibility changes logged for security and debugging.

### PostgreSQL Table: `VisibilityAuditLog`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Log entry ID |
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post affected |
| `ChangedById` | UUID | FK→User.Id, NOT NULL | User who changed |
| `OldVisibility` | ENUM | NOT NULL | Previous visibility |
| `NewVisibility` | ENUM | NOT NULL | New visibility |
| `Reason` | TEXT | NULLABLE | Change reason |
| `CreatedAt` | TIMESTAMP | NOT NULL | Change timestamp |

---

## 13. Access Control Summary

### Permission Matrix

| Action | Owner | Follower | Non-Follower | Blocked | Anonymous |
|--------|-------|----------|--------------|---------|-----------|
| **View Public Post** | ✅ | ✅ | ✅ | ❌ | ✅ |
| **View Followers-Only Post** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **View Private Post** | ✅ | If mentioned | If mentioned | ❌ | ❌ |
| **Like Post** | ✅ | Depends on visibility | Depends on visibility | ❌ | ❌ |
| **Reply to Post** | ✅ | Depends on settings | Depends on settings | ❌ | ❌ |
| **Quote Post** | ✅ | Depends on settings | Depends on settings | ❌ | ❌ |
| **Repost** | ✅ | Depends on settings | Depends on settings | ❌ | ❌ |
| **View Profile** | ✅ | ✅ | Limited if private | Very limited | Public only |

### Privacy-First Defaults

Recommended defaults for new users:
- `DefaultPostVisibility = Public`
- `IsPrivate = false`
- `AllowMentions = All`
- `AllowDirectMessages = FollowersOnly`
- `ShowActivity = true`
- `BlurSensitiveMedia = true`

---

## Performance Considerations

### Caching Strategy

1. **User Privacy Settings**: Cache in Redis for 1 hour
2. **Follow Relationships**: Cache for 15 minutes
3. **Block Lists**: Cache indefinitely with invalidation
4. **Circle Membership**: Cache for 30 minutes

### Indexes for Visibility

```sql
-- Essential indexes for visibility checks
CREATE INDEX idx_posts_visibility ON Posts(Visibility);
CREATE INDEX idx_posts_author_visibility ON Posts(AuthorId, Visibility, CreatedAt DESC);
CREATE INDEX idx_follows_active ON Follows(FollowerId, FollowingId) WHERE Status = 'Active';
CREATE INDEX idx_blocks_blocker_blocked ON Blocks(BlockerId, BlockedId);
CREATE INDEX idx_mentions_mentioned ON Mentions(MentionedId, CreatedAt DESC);
```

---

## Next Steps

See [ERD.md](./ERD.md) for the complete entity relationship diagram incorporating all privacy and visibility rules.
