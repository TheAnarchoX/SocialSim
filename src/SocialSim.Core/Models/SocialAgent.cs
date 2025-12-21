namespace SocialSim.Core.Models;

/// <summary>
/// Represents a user/agent in the social simulation
/// Supports both traditional social network and AT Protocol (Bluesky) concepts
/// </summary>
public class SocialAgent
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Username for traditional platforms
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Decentralized Identifier (DID) for AT Protocol support
    /// Example: did:plc:z72i7hdynmk6r22z27h6tvur
    /// </summary>
    public string? DecentralizedId { get; set; }
    
    /// <summary>
    /// Handle for AT Protocol (e.g., user.bsky.social)
    /// </summary>
    public string? Handle { get; set; }
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string Bio { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Behavioral traits that drive simulation decisions
    /// </summary>
    public AgentBehavior Behavior { get; set; } = new();
    
    /// <summary>
    /// Network metrics
    /// </summary>
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int PostCount { get; set; }
    
    /// <summary>
    /// Protocol type: Traditional, ATProtocol, or Hybrid
    /// </summary>
    public ProtocolType ProtocolType { get; set; } = ProtocolType.Traditional;
}

public enum ProtocolType
{
    Traditional,   // Standard social network
    ATProtocol,    // Bluesky/AT Protocol
    Hybrid         // Supports both
}

public class AgentBehavior
{
    /// <summary>
    /// How often the agent posts (0.0 to 1.0)
    /// </summary>
    public double PostingFrequency { get; set; } = 0.5;
    
    /// <summary>
    /// How likely to engage with content (0.0 to 1.0)
    /// </summary>
    public double EngagementRate { get; set; } = 0.5;
    
    /// <summary>
    /// How influential the agent is (0.0 to 1.0)
    /// </summary>
    public double InfluenceScore { get; set; } = 0.5;
    
    /// <summary>
    /// Topics of interest for content generation
    /// </summary>
    public List<string> Interests { get; set; } = new();
}
