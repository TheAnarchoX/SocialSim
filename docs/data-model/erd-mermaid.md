# Visual ERD - Mermaid Diagrams

This document contains interactive ERD diagrams using Mermaid.js syntax that can be rendered in GitHub, VS Code, and other Markdown viewers.

## Core Entities Diagram

### Users and Posts

```mermaid
erDiagram
    Users ||--o{ Posts : creates
    Users ||--o{ Media : uploads
    Posts }o--o{ Media : contains
    Posts ||--o{ Posts : replies_to
    Posts ||--o{ Posts : quotes
    
    Users {
        uuid Id PK
        string DecentralizedId UK "AT Protocol DID"
        string Handle UK
        string DisplayName
        text Bio
        string Email UK
        boolean IsPrivate
        boolean IsVerified
        boolean IsSimulated
        int FollowerCount
        int FollowingCount
        int PostCount
        timestamp CreatedAt
        timestamp UpdatedAt
    }
    
    Posts {
        uuid Id PK
        uuid AuthorId FK
        string RecordKey "AT Protocol rkey"
        string ContentId "AT Protocol CID"
        text Content
        enum ContentType
        enum Visibility
        boolean IsReply
        uuid ReplyToPostId FK
        uuid ThreadId FK
        uuid QuotedPostId FK
        int LikeCount
        int RepostCount
        int ReplyCount
        float EngagementScore
        timestamp CreatedAt
    }
    
    Media {
        uuid Id PK
        uuid UploadedById FK
        string BlobId
        enum MediaType
        string MimeType
        bigint FileSizeBytes
        int Width
        int Height
        string StorageUrl
        text AltText
        timestamp CreatedAt
    }
```

---

## Social Relationships Diagram

```mermaid
erDiagram
    Users ||--o{ Follows : follower
    Users ||--o{ Follows : following
    Users ||--o{ Blocks : blocker
    Users ||--o{ Blocks : blocked
    Users ||--o{ Mutes : muter
    Users ||--o{ Mutes : muted
    
    Follows {
        uuid Id PK
        uuid FollowerId FK
        uuid FollowingId FK
        enum Status "Active, Pending, Rejected"
        boolean IsMuted
        boolean NotificationsEnabled
        timestamp CreatedAt
    }
    
    Blocks {
        uuid Id PK
        uuid BlockerId FK
        uuid BlockedId FK
        enum Reason
        text Notes
        timestamp CreatedAt
    }
    
    Mutes {
        uuid Id PK
        uuid MuterId FK
        uuid MutedId FK
        enum MuteType "All, Replies, Reposts"
        timestamp ExpiresAt
        timestamp CreatedAt
    }
```

---

## Engagement System Diagram

```mermaid
erDiagram
    Users ||--o{ Likes : gives
    Posts ||--o{ Likes : receives
    Users ||--o{ Reposts : creates
    Posts ||--o{ Reposts : receives
    Users ||--o{ Bookmarks : saves
    Posts ||--o{ Bookmarks : bookmarked
    Posts ||--o{ Mentions : contains
    Users ||--o{ Mentions : mentioned_in
    
    Likes {
        uuid Id PK
        uuid UserId FK
        uuid PostId FK
        enum ReactionType "Like, Love, Laugh"
        timestamp CreatedAt
    }
    
    Reposts {
        uuid Id PK
        uuid UserId FK
        uuid OriginalPostId FK
        uuid RepostPostId FK
        timestamp CreatedAt
    }
    
    Bookmarks {
        uuid Id PK
        uuid UserId FK
        uuid PostId FK
        uuid FolderId FK
        text Notes
        timestamp CreatedAt
    }
    
    Mentions {
        uuid Id PK
        uuid PostId FK
        uuid MentionerId FK
        uuid MentionedId FK
        int StartIndex
        int EndIndex
        timestamp CreatedAt
    }
```

---

## Thread System Diagram

```mermaid
erDiagram
    Threads ||--|| Posts : root_post
    Threads ||--o{ Posts : contains
    Threads ||--o{ ThreadParticipants : has
    Users ||--o{ ThreadParticipants : participates
    Threads ||--o{ ThreadSubscriptions : subscribed
    Users ||--o{ ThreadSubscriptions : subscribes
    
    Threads {
        uuid Id PK
        uuid RootPostId FK
        string Title
        int ParticipantCount
        int PostCount
        int ViewCount
        timestamp LastActivityAt
        boolean IsLocked
        timestamp CreatedAt
    }
    
    ThreadParticipants {
        uuid ThreadId PK_FK
        uuid UserId PK_FK
        int PostCount
        timestamp FirstPostAt
        timestamp LastPostAt
    }
    
    ThreadSubscriptions {
        uuid ThreadId PK_FK
        uuid UserId PK_FK
        enum NotificationLevel
        boolean IsMuted
        timestamp CreatedAt
    }
```

---

## Privacy & Circles Diagram

```mermaid
erDiagram
    Users ||--o{ Circles : owns
    Circles ||--o{ CircleMembers : contains
    Users ||--o{ CircleMembers : member_of
    Users ||--|| UserSettings : has_settings
    Users ||--|| UserContentFilters : has_filters
    
    Circles {
        uuid Id PK
        uuid OwnerId FK
        string Name
        text Description
        string Color
        int MemberCount
        timestamp CreatedAt
    }
    
    CircleMembers {
        uuid CircleId PK_FK
        uuid MemberId PK_FK
        timestamp AddedAt
    }
    
    UserSettings {
        uuid UserId PK_FK
        enum DefaultPostVisibility
        boolean AllowFollowRequests
        enum AllowMentions
        enum AllowDirectMessages
        boolean ShowFollowers
        boolean ShowFollowing
        timestamp UpdatedAt
    }
    
    UserContentFilters {
        uuid UserId PK_FK
        boolean HideNSFW
        boolean HideViolence
        boolean HideSpoilers
        boolean BlurSensitiveMedia
        timestamp UpdatedAt
    }
```

---

## Complete System Overview

```mermaid
erDiagram
    %% Core Entities
    Users ||--o{ Posts : "1:N (creates)"
    Users ||--o{ Media : "1:N (uploads)"
    Posts }o--o{ Media : "N:M (contains)"
    
    %% Social Graph
    Users ||--o{ Follows : "1:N (follower)"
    Users ||--o{ Follows : "1:N (following)"
    Users ||--o{ Blocks : "1:N"
    Users ||--o{ Mutes : "1:N"
    
    %% Engagement
    Users ||--o{ Likes : "1:N"
    Posts ||--o{ Likes : "1:N"
    Users ||--o{ Bookmarks : "1:N"
    Posts ||--o{ Bookmarks : "1:N"
    
    %% Threads
    Threads ||--|| Posts : "1:1 (root)"
    Threads ||--o{ Posts : "1:N (contains)"
    Threads }o--o{ Users : "N:M (participants)"
    
    %% Privacy
    Users ||--o{ Circles : "1:N (owns)"
    Circles }o--o{ Users : "N:M (members)"
    
    Users {
        uuid Id PK
        string Handle UK
        string DecentralizedId UK
        int FollowerCount
    }
    
    Posts {
        uuid Id PK
        uuid AuthorId FK
        enum Visibility
        int LikeCount
        int ReplyCount
    }
    
    Follows {
        uuid FollowerId FK
        uuid FollowingId FK
        enum Status
    }
    
    Likes {
        uuid UserId FK
        uuid PostId FK
        enum ReactionType
    }
    
    Threads {
        uuid Id PK
        uuid RootPostId FK
        int ParticipantCount
    }
```

---

## Neo4j Graph Visualization

### Social Network Graph

```mermaid
graph LR
    U1((User A<br/>@alice))
    U2((User B<br/>@bob))
    U3((User C<br/>@carol))
    U4((User D<br/>@dan))
    
    P1[Post 1<br/>Hello World]
    P2[Post 2<br/>Reply]
    P3[Post 3<br/>Quote]
    
    %% Follows
    U1 -->|FOLLOWS| U2
    U2 -->|FOLLOWS| U1
    U1 -->|FOLLOWS| U3
    U3 -->|FOLLOWS| U4
    U4 -->|FOLLOWS| U1
    
    %% Posts
    U1 -->|POSTS| P1
    U2 -->|POSTS| P2
    U3 -->|POSTS| P3
    
    %% Engagement
    U2 -.->|LIKES| P1
    U3 -.->|LIKES| P1
    U4 -.->|LIKES| P1
    
    %% Replies & Quotes
    P2 -->|REPLIES_TO| P1
    P3 -->|QUOTES| P1
    
    style U1 fill:#e1f5ff
    style U2 fill:#e1f5ff
    style U3 fill:#e1f5ff
    style U4 fill:#e1f5ff
    style P1 fill:#fff4e1
    style P2 fill:#fff4e1
    style P3 fill:#fff4e1
```

### Engagement Cascade

```mermaid
graph TB
    P1[Original Post<br/>by @alice]
    
    R1[Repost<br/>by @bob]
    R2[Repost<br/>by @carol]
    R3[Repost<br/>by @dan]
    
    R1_1[Repost<br/>by @eve]
    R1_2[Repost<br/>by @frank]
    
    R2_1[Repost<br/>by @grace]
    
    P1 --> R1
    P1 --> R2
    P1 --> R3
    
    R1 --> R1_1
    R1 --> R1_2
    
    R2 --> R2_1
    
    style P1 fill:#4CAF50
    style R1 fill:#FFC107
    style R2 fill:#FFC107
    style R3 fill:#FFC107
    style R1_1 fill:#FF9800
    style R1_2 fill:#FF9800
    style R2_1 fill:#FF9800
```

---

## Thread Structure Visualization

```mermaid
graph TD
    ROOT[Root Post<br/>Thread ID: 001]
    
    R1[Reply 1<br/>by @bob]
    R2[Reply 2<br/>by @carol]
    R3[Reply 3<br/>by @dan]
    
    R1_1[Reply 1.1<br/>by @eve]
    R1_2[Reply 1.2<br/>by @alice]
    
    R1_1_1[Reply 1.1.1<br/>by @frank]
    
    R2_1[Reply 2.1<br/>by @grace]
    
    ROOT --> R1
    ROOT --> R2
    ROOT --> R3
    
    R1 --> R1_1
    R1 --> R1_2
    
    R1_1 --> R1_1_1
    
    R2 --> R2_1
    
    style ROOT fill:#2196F3,color:#fff
    style R1 fill:#64B5F6
    style R2 fill:#64B5F6
    style R3 fill:#64B5F6
    style R1_1 fill:#90CAF9
    style R1_2 fill:#90CAF9
    style R1_1_1 fill:#BBDEFB
    style R2_1 fill:#90CAF9
```

---

## Data Flow Diagram

```mermaid
graph LR
    CLIENT[Client App]
    API[API Server]
    PG[(PostgreSQL)]
    NEO[(Neo4j)]
    REDIS[(Redis)]
    SIGNALR[SignalR Hub]
    
    CLIENT -->|HTTP Request| API
    API -->|Write| PG
    API -->|Write| NEO
    API -->|Cache/Events| REDIS
    API -->|Broadcast| SIGNALR
    SIGNALR -->|WebSocket| CLIENT
    
    API -->|Read Cache| REDIS
    REDIS -.->|Cache Miss| PG
    REDIS -.->|Cache Miss| NEO
    
    style CLIENT fill:#E8F5E9
    style API fill:#FFF9C4
    style PG fill:#E1F5FE
    style NEO fill:#FCE4EC
    style REDIS fill:#FFF3E0
    style SIGNALR fill:#F3E5F5
```

---

## Visibility & Access Control Flow

```mermaid
graph TD
    REQ[User Request:<br/>View Post]
    AUTH{Authenticated?}
    AUTHOR{Is Author?}
    BLOCKED{Blocked?}
    VIS{Post Visibility}
    FOLLOWER{Is Follower?}
    MENTIONED{Is Mentioned?}
    CIRCLE{In Circle?}
    
    ALLOW[✅ Allow Access]
    DENY[❌ Deny Access]
    
    REQ --> AUTH
    AUTH -->|No| VIS
    AUTH -->|Yes| AUTHOR
    
    AUTHOR -->|Yes| ALLOW
    AUTHOR -->|No| BLOCKED
    
    BLOCKED -->|Yes| DENY
    BLOCKED -->|No| VIS
    
    VIS -->|Public| ALLOW
    VIS -->|FollowersOnly| FOLLOWER
    VIS -->|Mentions| MENTIONED
    VIS -->|CircleOnly| CIRCLE
    VIS -->|Private| MENTIONED
    
    FOLLOWER -->|Yes| ALLOW
    FOLLOWER -->|No| DENY
    
    MENTIONED -->|Yes| ALLOW
    MENTIONED -->|No| DENY
    
    CIRCLE -->|Yes| ALLOW
    CIRCLE -->|No| DENY
    
    style ALLOW fill:#4CAF50,color:#fff
    style DENY fill:#F44336,color:#fff
    style REQ fill:#2196F3,color:#fff
```

---

## Rendering Instructions

### In VS Code

1. Install "Markdown Preview Mermaid Support" extension
2. Open this file in preview mode (Ctrl+Shift+V)
3. Diagrams will render automatically

### In GitHub

1. GitHub natively supports Mermaid diagrams
2. View this file on GitHub to see rendered diagrams

### In Documentation Sites

1. **MkDocs**: Use `mkdocs-mermaid2-plugin`
2. **Docusaurus**: Supports Mermaid out of the box
3. **GitBook**: Use Mermaid plugin

### Export to Image

Use [Mermaid Live Editor](https://mermaid.live) to export as PNG/SVG.

---

## Interactive Exploration

For interactive exploration of the data model:

1. **Neo4j Browser**: Load sample graph data and explore visually
2. **DBeaver**: Connect to PostgreSQL and auto-generate ERD
3. **dbdiagram.io**: Import schema and create collaborative diagrams

See [sample-data.md](./sample-data.md) for seed data to populate databases.
