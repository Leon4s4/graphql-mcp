namespace Graphql.Mcp.DTO;

/// <summary>
/// Security analysis for queries
/// </summary>
public class SecurityAnalysis
{
    public List<string> SecurityWarnings { get; set; } = [];
    public List<string> RequiredPermissions { get; set; } = [];
    public bool HasSensitiveData { get; set; }
    public List<string> SecurityRecommendations { get; set; } = [];
}