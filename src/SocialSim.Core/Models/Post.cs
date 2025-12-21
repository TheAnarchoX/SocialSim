namespace SocialSim.Core.Models;

/// <summary>
/// Represents a post/content in the social network
/// Supports both traditional posts and AT Protocol records
/// </summary>
public class Post
{
    public Guid Id { get; set; }
    
    public Guid AuthorId { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// AT Protocol record key (rkey) for the post
    /// Used in AT Protocol for content addressing
    /// </summary>
    public string? RecordKey { get; set; }
    
    /// <summary>
    /// AT Protocol CID (Content Identifier)
    /// </summary>
    public string? ContentId { get; set; }
    
    /// <summary>
    /// Engagement metrics
    /// </summary>
    public int LikeCount { get; set; }
    public int RepostCount { get; set; }
    public int ReplyCount { get; set; }
    
    /// <summary>
    /// References for replies and reposts
    /// </summary>
    public Guid? ReplyToPostId { get; set; }
    public Guid? RepostOfPostId { get; set; }
    
    public List<string> Tags { get; set; } = new();
}
