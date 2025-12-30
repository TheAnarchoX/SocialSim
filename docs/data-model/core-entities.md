# Core Entities Specification

This document defines the core entities for the SocialSim social network.

## 1. User (SocialAgent)

The User entity represents both real users and simulated agents in the system.

### Attributes

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `DecentralizedId` | VARCHAR(256) | UNIQUE, NOT NULL | AT Protocol DID (e.g., did:plc:abc123) |
| `Handle` | VARCHAR(128) | UNIQUE, NOT NULL | Human-readable handle (@username.domain.com) |
| `DisplayName` | VARCHAR(256) | NOT NULL | Display name shown in UI |
| `Bio` | TEXT | NULLABLE | User biography/description |
| `AvatarUrl` | VARCHAR(512) | NULLABLE | URL to avatar image |
| `BannerUrl` | VARCHAR(512) | NULLABLE | URL to banner/header image |
| `Location` | VARCHAR(256) | NULLABLE | User's location (free text) |
| `Website` | VARCHAR(512) | NULLABLE | User's website URL |
| `Email` | VARCHAR(256) | UNIQUE, NULLABLE | Email address (for authentication) |
| `PasswordHash` | VARCHAR(256) | NULLABLE | Hashed password (bcrypt) |
| `ProtocolType` | ENUM | NOT NULL | Protocol type: ATProtocol, ActivityPub, Native |
| `IsSimulated` | BOOLEAN | NOT NULL, DEFAULT FALSE | Whether this is a simulated agent |
| `IsVerified` | BOOLEAN | NOT NULL, DEFAULT FALSE | Verification status |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT TRUE | Account active status |
| `IsSuspended` | BOOLEAN | NOT NULL, DEFAULT FALSE | Account suspension status |
| `SuspendedAt` | TIMESTAMP | NULLABLE | When account was suspended |
| `SuspensionReason` | TEXT | NULLABLE | Reason for suspension |
| `FollowerCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached follower count |
| `FollowingCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached following count |
| `PostCount` | INTEGER | NOT NULL, DEFAULT 0 | Total posts created |
| `CreatedAt` | TIMESTAMP | NOT NULL | Account creation timestamp |
| `UpdatedAt` | TIMESTAMP | NOT NULL | Last update timestamp |
| `LastActiveAt` | TIMESTAMP | NULLABLE | Last activity timestamp |
| `DeletedAt` | TIMESTAMP | NULLABLE | Soft delete timestamp |

### Simulation-Specific Attributes (JSON field or separate table)

```json
{
  "agentBehavior": {
    "postingFrequency": 5.2,           // posts per day
    "engagementRate": 0.15,            // probability of engaging with content
    "influenceScore": 0.73,            // 0-1 scale
    "personality": {
      "extraversion": 0.65,            // 0-1 scale
      "agreeableness": 0.82,
      "conscientiousness": 0.55,
      "emotionalStability": 0.71,
      "openness": 0.88
    },
    "contentPreferences": {
      "topics": ["technology", "science", "politics"],
      "mediaPreference": "text",       // text, image, video, mixed
      "averagePostLength": 280,
      "sentimentBias": 0.2             // -1 (negative) to 1 (positive)
    },
    "socialBehavior": {
      "followBackProbability": 0.35,
      "replyProbability": 0.12,
      "shareProbability": 0.08,
      "quoteProbability": 0.05,
      "influenceSusceptibility": 0.45
    },
    "activityPattern": {
      "peakHours": [9, 12, 18, 21],    // hours of day (0-23)
      "weekdayActivity": 0.7,
      "weekendActivity": 0.5,
      "timezone": "UTC"
    }
  }
}
```

### Indexes

- PRIMARY KEY: `Id`
- UNIQUE: `DecentralizedId`, `Handle`, `Email`
- INDEX: `IsActive`, `IsSimulated`, `CreatedAt`
- INDEX: `FollowerCount DESC` (for top users queries)
- INDEX: `LastActiveAt DESC` (for active users queries)

### Business Rules

1. Handle must match pattern: `@[a-zA-Z0-9._-]+\.[a-zA-Z0-9._-]+`
2. DID must be valid AT Protocol DID format
3. Email required for non-simulated users
4. Suspended users cannot create content or interact
5. Deleted users retain data but are not discoverable

---

## 2. Post

The Post entity represents user-generated content.

### Attributes

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `AuthorId` | UUID | FK→User.Id, NOT NULL | Post author |
| `RecordKey` | VARCHAR(128) | NULLABLE | AT Protocol record key (rkey) |
| `ContentId` | VARCHAR(256) | NULLABLE | AT Protocol content ID (CID) |
| `Content` | TEXT | NOT NULL | Post text content (max 300 chars for microblog) |
| `ContentType` | ENUM | NOT NULL | Type: Text, TextWithMedia, Quote, Repost |
| `Language` | VARCHAR(8) | NOT NULL, DEFAULT 'en' | Content language (ISO 639-1) |
| `Visibility` | ENUM | NOT NULL, DEFAULT 'Public' | Public, FollowersOnly, Private, Mentions |
| `IsReply` | BOOLEAN | NOT NULL, DEFAULT FALSE | Whether this is a reply |
| `ReplyToPostId` | UUID | FK→Post.Id, NULLABLE | Parent post if reply |
| `ReplyToUserId` | UUID | FK→User.Id, NULLABLE | User being replied to |
| `ThreadId` | UUID | FK→Thread.Id, NULLABLE | Conversation thread |
| `RootPostId` | UUID | FK→Post.Id, NULLABLE | Root post of thread |
| `QuotedPostId` | UUID | FK→Post.Id, NULLABLE | Post being quoted |
| `RepostOfPostId` | UUID | FK→Post.Id, NULLABLE | Original post if repost |
| `LikeCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached like count |
| `RepostCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached repost count |
| `QuoteCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached quote count |
| `ReplyCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached reply count |
| `ViewCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached view count |
| `EngagementScore` | FLOAT | NOT NULL, DEFAULT 0 | Calculated engagement metric |
| `IsFlagged` | BOOLEAN | NOT NULL, DEFAULT FALSE | Flagged for moderation |
| `IsDeleted` | BOOLEAN | NOT NULL, DEFAULT FALSE | Soft delete flag |
| `CreatedAt` | TIMESTAMP | NOT NULL | Creation timestamp |
| `UpdatedAt` | TIMESTAMP | NOT NULL | Last update timestamp |
| `DeletedAt` | TIMESTAMP | NULLABLE | Soft delete timestamp |

### Metadata (JSON field)

```json
{
  "facets": [                          // Rich text facets (AT Protocol)
    {
      "index": { "byteStart": 10, "byteEnd": 23 },
      "features": [
        { "$type": "app.bsky.richtext.facet#mention", "did": "did:plc:xyz" }
      ]
    }
  ],
  "tags": ["#technology", "#ai"],      // Hashtags
  "mentions": ["did:plc:abc", "did:plc:def"],
  "links": ["https://example.com"],
  "contentWarnings": [],                // Content warnings
  "simulationMetadata": {
    "generatedBy": "agent-123",
    "scenario": "viral-event-001",
    "topicModel": "tech-trends",
    "sentiment": 0.65
  }
}
```

### Indexes

- PRIMARY KEY: `Id`
- FOREIGN KEY: `AuthorId`, `ReplyToPostId`, `ThreadId`, `RootPostId`, `QuotedPostId`
- INDEX: `AuthorId, CreatedAt DESC` (user timeline)
- INDEX: `ThreadId, CreatedAt ASC` (thread view)
- INDEX: `CreatedAt DESC` (global feed)
- INDEX: `EngagementScore DESC, CreatedAt DESC` (trending)
- FULLTEXT INDEX: `Content` (search)

### Business Rules

1. Content length limits based on ContentType
2. Replies must have valid ReplyToPostId
3. Quotes must have valid QuotedPostId
4. Visibility cannot be more restrictive than parent post
5. Deleted posts hide content but retain metadata for thread integrity

---

## 3. Comment

The Comment entity represents replies to posts (alternative to treating replies as Posts).

> **Note**: In many social networks, replies ARE posts. We may choose to use the Post entity with `IsReply=true` instead of a separate Comment entity. This design includes both options.

### Attributes (if using separate Comment entity)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post being commented on |
| `AuthorId` | UUID | FK→User.Id, NOT NULL | Comment author |
| `ParentCommentId` | UUID | FK→Comment.Id, NULLABLE | Parent comment (for nested replies) |
| `Content` | TEXT | NOT NULL | Comment text |
| `LikeCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached like count |
| `ReplyCount` | INTEGER | NOT NULL, DEFAULT 0 | Cached reply count |
| `Depth` | INTEGER | NOT NULL, DEFAULT 0 | Nesting depth (0 = top-level) |
| `IsDeleted` | BOOLEAN | NOT NULL, DEFAULT FALSE | Soft delete flag |
| `CreatedAt` | TIMESTAMP | NOT NULL | Creation timestamp |
| `UpdatedAt` | TIMESTAMP | NOT NULL | Last update timestamp |
| `DeletedAt` | TIMESTAMP | NULLABLE | Soft delete timestamp |

### Indexes

- PRIMARY KEY: `Id`
- FOREIGN KEY: `PostId`, `AuthorId`, `ParentCommentId`
- INDEX: `PostId, CreatedAt ASC` (comments on post)
- INDEX: `AuthorId, CreatedAt DESC` (user's comments)

---

## 4. Media

The Media entity represents attached images, videos, and other media.

### Attributes

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PK, NOT NULL | Unique identifier |
| `UploadedById` | UUID | FK→User.Id, NOT NULL | User who uploaded |
| `BlobId` | VARCHAR(256) | NOT NULL | AT Protocol blob CID or storage key |
| `MediaType` | ENUM | NOT NULL | Image, Video, Audio, Document |
| `MimeType` | VARCHAR(128) | NOT NULL | e.g., image/jpeg, video/mp4 |
| `FileName` | VARCHAR(512) | NOT NULL | Original filename |
| `FileSizeBytes` | BIGINT | NOT NULL | File size in bytes |
| `Width` | INTEGER | NULLABLE | Image/video width in pixels |
| `Height` | INTEGER | NULLABLE | Image/video height in pixels |
| `DurationSeconds` | FLOAT | NULLABLE | Audio/video duration |
| `StorageUrl` | VARCHAR(1024) | NOT NULL | URL to blob storage |
| `ThumbnailUrl` | VARCHAR(1024) | NULLABLE | URL to thumbnail |
| `AltText` | TEXT | NULLABLE | Accessibility alt text |
| `IsProcessed` | BOOLEAN | NOT NULL, DEFAULT FALSE | Processing complete flag |
| `ProcessingError` | TEXT | NULLABLE | Error message if processing failed |
| `CreatedAt` | TIMESTAMP | NOT NULL | Upload timestamp |
| `DeletedAt` | TIMESTAMP | NULLABLE | Soft delete timestamp |

### Metadata (JSON field)

```json
{
  "exif": {                             // EXIF data for images
    "make": "Canon",
    "model": "EOS R5",
    "dateTaken": "2025-01-15T14:30:00Z"
  },
  "blurhash": "LKO2?U%2Tw=w]~RBVZRi};RPxuwH", // Blurhash for placeholder
  "colors": ["#FF5733", "#33FF57"],     // Dominant colors
  "nsfw": {                             // Content classification
    "score": 0.02,
    "isNSFW": false
  },
  "faces": 2,                           // Number of faces detected
  "labels": ["technology", "computer", "workspace"] // AI-generated labels
}
```

### Indexes

- PRIMARY KEY: `Id`
- FOREIGN KEY: `UploadedById`
- INDEX: `UploadedById, CreatedAt DESC`
- INDEX: `BlobId` (for deduplication)

### Business Rules

1. File size limits based on MediaType
2. MIME type validation
3. Image/video processing required before availability
4. NSFW content flagging
5. Orphaned media cleanup after retention period

---

## 5. PostMedia (Junction Table)

Links posts to their attached media.

### Attributes

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `PostId` | UUID | FK→Post.Id, NOT NULL | Post with media |
| `MediaId` | UUID | FK→Media.Id, NOT NULL | Attached media |
| `DisplayOrder` | INTEGER | NOT NULL, DEFAULT 0 | Order of media in post |

### Indexes

- PRIMARY KEY: `(PostId, MediaId)`
- INDEX: `PostId, DisplayOrder ASC`

---

## Entity Relationships

```
User (1) ──── (N) Post [AuthorId]
User (1) ──── (N) Media [UploadedById]
Post (1) ──── (N) Post [ReplyToPostId] (self-referential)
Post (1) ──── (N) Post [QuotedPostId] (self-referential)
Post (N) ──── (N) Media [through PostMedia]
```

## Next Steps

See [relationships.md](./relationships.md) for social relationship entities (Follow, Block, Mute, Report).
