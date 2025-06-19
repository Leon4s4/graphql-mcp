namespace Graphql.Mcp.DTO;

/// <summary>
/// Comprehensive endpoint metadata
/// </summary>
public class EndpointMetadata
{
    public string Url { get; set; } = "";
    public List<string> SupportedProtocols { get; set; } = [];
    public AuthenticationInfo? Authentication { get; set; }
    public RateLimitInfo? RateLimit { get; set; }
    public List<string> SupportedFeatures { get; set; } = [];
    public HealthStatus Health { get; set; } = new();
    public VersionInfo Version { get; set; } = new();
    public List<string> DeprecationWarnings { get; set; } = [];
}