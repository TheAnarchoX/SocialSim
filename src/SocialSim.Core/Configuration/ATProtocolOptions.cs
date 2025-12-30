namespace SocialSim.Core.Configuration;

/// <summary>
/// Configuration options for AT Protocol implementation
/// Supports custom domain handles (e.g., @username.theanarchox.net)
/// </summary>
public class ATProtocolOptions
{
    /// <summary>
    /// Custom domain for handles (e.g., "theanarchox.net")
    /// </summary>
    public string CustomDomain { get; set; } = "localhost";
    
    /// <summary>
    /// Format string for generating handles
    /// Use {username} as placeholder (e.g., "{username}.theanarchox.net")
    /// </summary>
    public string HandleFormat { get; set; } = "{username}.localhost";
    
    /// <summary>
    /// Enable DNS TXT record verification for handles
    /// </summary>
    public bool EnableDNSVerification { get; set; } = false;
    
    /// <summary>
    /// Enable HTTPS well-known endpoint for DID resolution
    /// </summary>
    public bool EnableWellKnownEndpoint { get; set; } = true;
    
    /// <summary>
    /// PDS (Personal Data Server) endpoint
    /// </summary>
    public string PDSEndpoint { get; set; } = "http://localhost:3000";
    
    /// <summary>
    /// Base DID prefix for generating agent DIDs
    /// </summary>
    public string BaseDIDPrefix { get; set; } = "did:plc:socialsim";
    
    /// <summary>
    /// Prefix for agent usernames (e.g., "agent" -> "agent001")
    /// </summary>
    public string AgentHandlePrefix { get; set; } = "agent";
    
    /// <summary>
    /// Generate a handle for a username
    /// </summary>
    public string GenerateHandle(string username)
    {
        return HandleFormat.Replace("{username}", username);
    }
    
    /// <summary>
    /// Generate a DID for an agent
    /// </summary>
    public string GenerateDID(string identifier)
    {
        return $"{BaseDIDPrefix}-{identifier}";
    }
    
    /// <summary>
    /// Parse username from a handle
    /// </summary>
    public string? ParseUsername(string handle)
    {
        if (string.IsNullOrWhiteSpace(handle))
            return null;
            
        // Remove domain suffix
        var suffix = $".{CustomDomain}";
        if (handle.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return handle[..^suffix.Length];
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the DNS TXT record name for a handle
    /// </summary>
    public string GetDNSTxtRecordName(string handle)
    {
        return $"_atproto.{handle}";
    }
    
    /// <summary>
    /// Get the well-known endpoint path for a handle
    /// </summary>
    public string GetWellKnownPath(string handle)
    {
        return $"https://{handle}/.well-known/atproto-did";
    }
}
