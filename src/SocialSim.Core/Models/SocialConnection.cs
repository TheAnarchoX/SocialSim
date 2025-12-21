namespace SocialSim.Core.Models;

/// <summary>
/// Represents a social connection/relationship between agents
/// Stored in Neo4J graph database
/// </summary>
public class SocialConnection
{
    public Guid Id { get; set; }
    
    public Guid SourceAgentId { get; set; }
    
    public Guid TargetAgentId { get; set; }
    
    public ConnectionType Type { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Strength of the connection (0.0 to 1.0)
    /// </summary>
    public double Strength { get; set; } = 0.5;
}

public enum ConnectionType
{
    Follow,      // One-way follow
    Friend,      // Mutual connection
    Block,       // Blocked relationship
    Mute         // Muted but still following
}
