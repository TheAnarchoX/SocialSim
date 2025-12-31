-- =====================================================
-- SocialSim PostgreSQL Database Schema
-- =====================================================
-- This schema defines the relational database structure
-- for SocialSim's social network simulation platform.
--
-- Features:
-- - Full AT Protocol support (DID, handles, CIDs)
-- - Privacy & visibility controls
-- - Engagement tracking
-- - Thread/conversation support
-- - Polyglot persistence (pairs with Neo4j)
-- =====================================================

-- =====================================================
-- EXTENSIONS
-- =====================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =====================================================
-- ENUMS
-- =====================================================

CREATE TYPE protocol_type AS ENUM ('ATProtocol', 'ActivityPub', 'Native');
CREATE TYPE content_visibility AS ENUM ('Public', 'FollowersOnly', 'Private', 'Mentions', 'CircleOnly');
CREATE TYPE content_type AS ENUM ('Text', 'TextWithMedia', 'Quote', 'Repost');
CREATE TYPE follow_status AS ENUM ('Active', 'Pending', 'Rejected');
CREATE TYPE report_reason AS ENUM ('Spam', 'Harassment', 'Violence', 'NSFW', 'Misinformation', 'Impersonation', 'SelfHarm', 'Other');
CREATE TYPE report_status AS ENUM ('Pending', 'UnderReview', 'Resolved', 'Dismissed');
CREATE TYPE media_type AS ENUM ('Image', 'Video', 'Audio', 'Document');
CREATE TYPE reaction_type AS ENUM ('Like', 'Love', 'Laugh', 'Wow', 'Sad', 'Angry');
CREATE TYPE mute_type AS ENUM ('All', 'Replies', 'Reposts');
CREATE TYPE notification_level AS ENUM ('All', 'Mentions', 'None');
CREATE TYPE dm_setting AS ENUM ('All', 'FollowersOnly', 'MutualFollows', 'None');
CREATE TYPE mention_setting AS ENUM ('All', 'FollowersOnly', 'None');

-- =====================================================
-- CORE ENTITIES
-- =====================================================

-- Users (SocialAgent)
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    decentralized_id VARCHAR(256) UNIQUE NOT NULL,
    handle VARCHAR(128) UNIQUE NOT NULL,
    display_name VARCHAR(256) NOT NULL,
    bio TEXT,
    avatar_url VARCHAR(512),
    banner_url VARCHAR(512),
    location VARCHAR(256),
    website VARCHAR(512),
    email VARCHAR(256) UNIQUE,
    password_hash VARCHAR(256),
    protocol_type protocol_type NOT NULL DEFAULT 'Native',
    is_simulated BOOLEAN NOT NULL DEFAULT FALSE,
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_private BOOLEAN NOT NULL DEFAULT FALSE,
    is_suspended BOOLEAN NOT NULL DEFAULT FALSE,
    suspended_at TIMESTAMP,
    suspension_reason TEXT,
    follower_count INTEGER NOT NULL DEFAULT 0,
    following_count INTEGER NOT NULL DEFAULT 0,
    post_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_active_at TIMESTAMP,
    deleted_at TIMESTAMP,
    
    -- Simulation-specific data (JSONB for flexibility)
    agent_behavior JSONB,
    
    CONSTRAINT chk_follower_count CHECK (follower_count >= 0),
    CONSTRAINT chk_following_count CHECK (following_count >= 0),
    CONSTRAINT chk_post_count CHECK (post_count >= 0)
);

-- User Settings
CREATE TABLE user_settings (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    default_post_visibility content_visibility NOT NULL DEFAULT 'Public',
    allow_follow_requests BOOLEAN NOT NULL DEFAULT TRUE,
    allow_mentions mention_setting NOT NULL DEFAULT 'All',
    allow_direct_messages dm_setting NOT NULL DEFAULT 'FollowersOnly',
    allow_quotes BOOLEAN NOT NULL DEFAULT TRUE,
    allow_reposts BOOLEAN NOT NULL DEFAULT TRUE,
    show_followers BOOLEAN NOT NULL DEFAULT TRUE,
    show_following BOOLEAN NOT NULL DEFAULT TRUE,
    show_activity BOOLEAN NOT NULL DEFAULT TRUE,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- User Content Filters
CREATE TABLE user_content_filters (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    hide_nsfw BOOLEAN NOT NULL DEFAULT FALSE,
    hide_violence BOOLEAN NOT NULL DEFAULT FALSE,
    hide_spoilers BOOLEAN NOT NULL DEFAULT FALSE,
    blur_sensitive_media BOOLEAN NOT NULL DEFAULT TRUE,
    require_content_warnings BOOLEAN NOT NULL DEFAULT FALSE,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Posts
CREATE TABLE posts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    record_key VARCHAR(128),
    content_id VARCHAR(256),
    content TEXT NOT NULL,
    content_type content_type NOT NULL DEFAULT 'Text',
    language VARCHAR(8) NOT NULL DEFAULT 'en',
    visibility content_visibility NOT NULL DEFAULT 'Public',
    is_reply BOOLEAN NOT NULL DEFAULT FALSE,
    reply_to_post_id UUID REFERENCES posts(id) ON DELETE SET NULL,
    reply_to_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    thread_id UUID,
    root_post_id UUID REFERENCES posts(id) ON DELETE SET NULL,
    quoted_post_id UUID REFERENCES posts(id) ON DELETE SET NULL,
    repost_of_post_id UUID REFERENCES posts(id) ON DELETE SET NULL,
    like_count INTEGER NOT NULL DEFAULT 0,
    repost_count INTEGER NOT NULL DEFAULT 0,
    quote_count INTEGER NOT NULL DEFAULT 0,
    reply_count INTEGER NOT NULL DEFAULT 0,
    view_count INTEGER NOT NULL DEFAULT 0,
    engagement_score FLOAT NOT NULL DEFAULT 0,
    is_sensitive BOOLEAN NOT NULL DEFAULT FALSE,
    content_warnings VARCHAR(64)[],
    is_flagged BOOLEAN NOT NULL DEFAULT FALSE,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMP,
    
    -- Metadata (facets, tags, etc.)
    metadata JSONB,
    
    CONSTRAINT chk_reply_consistency CHECK (
        (is_reply = FALSE AND reply_to_post_id IS NULL) OR
        (is_reply = TRUE AND reply_to_post_id IS NOT NULL)
    )
);

-- Media
CREATE TABLE media (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    uploaded_by_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    blob_id VARCHAR(256) NOT NULL,
    media_type media_type NOT NULL,
    mime_type VARCHAR(128) NOT NULL,
    file_name VARCHAR(512) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    width INTEGER,
    height INTEGER,
    duration_seconds FLOAT,
    storage_url VARCHAR(1024) NOT NULL,
    thumbnail_url VARCHAR(1024),
    alt_text TEXT,
    is_processed BOOLEAN NOT NULL DEFAULT FALSE,
    processing_error TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMP,
    
    -- Metadata (EXIF, blurhash, etc.)
    metadata JSONB
);

-- Post-Media junction table
CREATE TABLE post_media (
    post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    media_id UUID NOT NULL REFERENCES media(id) ON DELETE CASCADE,
    display_order INTEGER NOT NULL DEFAULT 0,
    
    PRIMARY KEY (post_id, media_id)
);

-- =====================================================
-- THREADS & CONVERSATIONS
-- =====================================================

-- Threads
CREATE TABLE threads (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    root_post_id UUID UNIQUE NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    title VARCHAR(256),
    participant_count INTEGER NOT NULL DEFAULT 1,
    post_count INTEGER NOT NULL DEFAULT 1,
    view_count INTEGER NOT NULL DEFAULT 0,
    last_activity_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_locked BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Add FK from posts to threads
ALTER TABLE posts ADD CONSTRAINT fk_posts_thread 
    FOREIGN KEY (thread_id) REFERENCES threads(id) ON DELETE SET NULL;

-- Thread Participants
CREATE TABLE thread_participants (
    thread_id UUID NOT NULL REFERENCES threads(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    post_count INTEGER NOT NULL DEFAULT 1,
    first_post_at TIMESTAMP NOT NULL,
    last_post_at TIMESTAMP NOT NULL,
    
    PRIMARY KEY (thread_id, user_id)
);

-- Thread Subscriptions
CREATE TABLE thread_subscriptions (
    thread_id UUID NOT NULL REFERENCES threads(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    notification_level notification_level NOT NULL DEFAULT 'All',
    is_muted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    PRIMARY KEY (thread_id, user_id)
);

-- =====================================================
-- SOCIAL RELATIONSHIPS
-- =====================================================

-- Follows
CREATE TABLE follows (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    follower_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    following_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status follow_status NOT NULL DEFAULT 'Active',
    is_muted BOOLEAN NOT NULL DEFAULT FALSE,
    notifications_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_follow UNIQUE (follower_id, following_id),
    CONSTRAINT chk_no_self_follow CHECK (follower_id != following_id)
);

-- Blocks
CREATE TABLE blocks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    blocker_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    blocked_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    reason report_reason,
    notes TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_block UNIQUE (blocker_id, blocked_id),
    CONSTRAINT chk_no_self_block CHECK (blocker_id != blocked_id)
);

-- Mutes
CREATE TABLE mutes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    muter_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    muted_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    mute_type mute_type NOT NULL DEFAULT 'All',
    expires_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_mute UNIQUE (muter_id, muted_id),
    CONSTRAINT chk_no_self_mute CHECK (muter_id != muted_id)
);

-- Reports
CREATE TABLE reports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    reporter_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    reported_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    reported_post_id UUID REFERENCES posts(id) ON DELETE SET NULL,
    report_type VARCHAR(32) NOT NULL,
    reason report_reason NOT NULL,
    description TEXT,
    status report_status NOT NULL DEFAULT 'Pending',
    resolution TEXT,
    resolved_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    resolved_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_report_target CHECK (
        (reported_user_id IS NOT NULL AND reported_post_id IS NULL) OR
        (reported_user_id IS NULL AND reported_post_id IS NOT NULL)
    )
);

-- =====================================================
-- ENGAGEMENT
-- =====================================================

-- Likes
CREATE TABLE likes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    reaction_type reaction_type NOT NULL DEFAULT 'Like',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_like UNIQUE (user_id, post_id)
);

-- Reposts
CREATE TABLE reposts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    original_post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    repost_post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_repost UNIQUE (user_id, original_post_id)
);

-- Quotes (similar to reposts but with commentary)
CREATE TABLE quotes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    quoted_post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    quote_post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Mentions
CREATE TABLE mentions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    mentioner_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    mentioned_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    start_index INTEGER NOT NULL,
    end_index INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Bookmarks
CREATE TABLE bookmarks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    folder_id UUID,
    notes TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_bookmark UNIQUE (user_id, post_id)
);

-- Bookmark Folders
CREATE TABLE bookmark_folders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(128) NOT NULL,
    description TEXT,
    color VARCHAR(7),
    icon VARCHAR(32),
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

ALTER TABLE bookmarks ADD CONSTRAINT fk_bookmarks_folder
    FOREIGN KEY (folder_id) REFERENCES bookmark_folders(id) ON DELETE SET NULL;

-- =====================================================
-- PRIVACY & CIRCLES
-- =====================================================

-- Circles (Close Friends)
CREATE TABLE circles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    owner_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(128) NOT NULL,
    description TEXT,
    color VARCHAR(7),
    member_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Circle Members
CREATE TABLE circle_members (
    circle_id UUID NOT NULL REFERENCES circles(id) ON DELETE CASCADE,
    member_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    added_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    PRIMARY KEY (circle_id, member_id)
);

-- =====================================================
-- HISTORY & AUDIT
-- =====================================================

-- Relationship History (for temporal queries)
CREATE TABLE relationship_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    target_user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    relationship_type VARCHAR(32) NOT NULL, -- 'FOLLOW', 'UNFOLLOW', 'BLOCK', 'UNBLOCK', 'MUTE', 'UNMUTE'
    action VARCHAR(16) NOT NULL, -- 'CREATED', 'DELETED', 'UPDATED'
    metadata JSONB,
    occurred_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_relationship_history_user ON relationship_history(user_id, occurred_at DESC);
CREATE INDEX idx_relationship_history_target ON relationship_history(target_user_id, occurred_at DESC);
CREATE INDEX idx_relationship_history_type ON relationship_history(relationship_type, occurred_at DESC);

-- User Metrics Snapshots (for tracking follower count over time)
CREATE TABLE user_metrics_snapshots (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    follower_count INTEGER NOT NULL,
    following_count INTEGER NOT NULL,
    post_count INTEGER NOT NULL,
    engagement_rate FLOAT,
    influence_score FLOAT,
    snapshot_date DATE NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_user_snapshot UNIQUE (user_id, snapshot_date)
);

CREATE INDEX idx_snapshots_user_date ON user_metrics_snapshots(user_id, snapshot_date DESC);
CREATE INDEX idx_snapshots_date ON user_metrics_snapshots(snapshot_date DESC);

-- =====================================================
-- INDEXES
-- =====================================================

-- Users
CREATE INDEX idx_users_handle ON users(handle);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_is_active ON users(is_active);
CREATE INDEX idx_users_created_at ON users(created_at DESC);
CREATE INDEX idx_users_follower_count ON users(follower_count DESC);
CREATE INDEX idx_users_last_active ON users(last_active_at DESC);

-- Additional user indexes for common query patterns
CREATE INDEX idx_users_active_followers ON users(is_active, follower_count DESC) 
WHERE is_active = TRUE AND deleted_at IS NULL;
CREATE INDEX idx_users_simulated ON users(is_simulated, created_at DESC);
CREATE INDEX idx_users_verified ON users(follower_count DESC) 
WHERE is_verified = TRUE AND is_active = TRUE;

-- Posts
CREATE INDEX idx_posts_author ON posts(author_id, created_at DESC);
CREATE INDEX idx_posts_thread ON posts(thread_id, created_at ASC);
CREATE INDEX idx_posts_created_at ON posts(created_at DESC);
CREATE INDEX idx_posts_engagement ON posts(engagement_score DESC, created_at DESC);
CREATE INDEX idx_posts_visibility ON posts(visibility);
CREATE INDEX idx_posts_reply_to ON posts(reply_to_post_id);
CREATE INDEX idx_posts_quoted ON posts(quoted_post_id);

-- Full-text search on posts
CREATE INDEX idx_posts_content_fts ON posts USING GIN (to_tsvector('english', content));

-- Additional post indexes for feed generation and discovery
CREATE INDEX idx_posts_public_feed ON posts(visibility, created_at DESC)
WHERE is_deleted = FALSE AND visibility = 'Public';
CREATE INDEX idx_posts_reply_chain ON posts(reply_to_post_id, created_at ASC)
WHERE is_reply = TRUE;
CREATE INDEX idx_posts_quote_cascade ON posts(quoted_post_id, created_at DESC)
WHERE quoted_post_id IS NOT NULL;

-- Follows
CREATE INDEX idx_follows_follower ON follows(follower_id, created_at DESC);
CREATE INDEX idx_follows_following ON follows(following_id, created_at DESC);
CREATE INDEX idx_follows_status ON follows(status);
CREATE INDEX idx_follows_active ON follows(follower_id, following_id) WHERE status = 'Active';

-- Additional follow indexes for relationship queries
CREATE INDEX idx_follows_active_follower ON follows(follower_id, following_id, created_at DESC)
WHERE status = 'Active';
CREATE INDEX idx_follows_mutual ON follows(follower_id, following_id, status)
WHERE status = 'Active';

-- Blocks
CREATE INDEX idx_blocks_blocker ON blocks(blocker_id, created_at DESC);
CREATE INDEX idx_blocks_blocked ON blocks(blocked_id);

-- Mutes
CREATE INDEX idx_mutes_muter ON mutes(muter_id, created_at DESC);
CREATE INDEX idx_mutes_expires ON mutes(expires_at) WHERE expires_at IS NOT NULL;

-- Reports
CREATE INDEX idx_reports_reporter ON reports(reporter_id, created_at DESC);
CREATE INDEX idx_reports_reported_user ON reports(reported_user_id, created_at DESC);
CREATE INDEX idx_reports_reported_post ON reports(reported_post_id, created_at DESC);
CREATE INDEX idx_reports_status ON reports(status, created_at ASC);

-- Likes
CREATE INDEX idx_likes_user ON likes(user_id, created_at DESC);
CREATE INDEX idx_likes_post ON likes(post_id, created_at DESC);

-- Mentions
CREATE INDEX idx_mentions_mentioned ON mentions(mentioned_id, created_at DESC);
CREATE INDEX idx_mentions_post ON mentions(post_id);

-- Bookmarks
CREATE INDEX idx_bookmarks_user ON bookmarks(user_id, created_at DESC);
CREATE INDEX idx_bookmarks_folder ON bookmarks(folder_id, created_at DESC);

-- Threads
CREATE INDEX idx_threads_last_activity ON threads(last_activity_at DESC);
CREATE INDEX idx_threads_post_count ON threads(post_count DESC);
CREATE INDEX idx_threads_active ON threads(last_activity_at DESC, post_count DESC)
WHERE is_locked = FALSE;

-- =====================================================
-- TRIGGERS & FUNCTIONS
-- =====================================================

-- Update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_posts_updated_at BEFORE UPDATE ON posts
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_threads_updated_at BEFORE UPDATE ON threads
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =====================================================
-- VIEWS
-- =====================================================

-- Active users view
CREATE VIEW active_users AS
SELECT id, handle, display_name, follower_count, following_count, post_count
FROM users
WHERE is_active = TRUE AND deleted_at IS NULL;

-- Public posts view
CREATE VIEW public_posts AS
SELECT p.*, u.handle AS author_handle, u.display_name AS author_name
FROM posts p
JOIN users u ON p.author_id = u.id
WHERE p.visibility = 'Public' AND p.is_deleted = FALSE AND u.is_active = TRUE;

-- =====================================================
-- COMMENTS
-- =====================================================

COMMENT ON TABLE users IS 'User accounts and simulated agents';
COMMENT ON TABLE posts IS 'User-generated content';
COMMENT ON TABLE follows IS 'Follower relationships between users';
COMMENT ON TABLE likes IS 'User reactions to posts';
COMMENT ON TABLE threads IS 'Conversation threads';

-- =====================================================
-- END OF SCHEMA
-- =====================================================
