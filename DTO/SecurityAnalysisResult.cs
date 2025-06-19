namespace Graphql.Mcp.DTO;

/// <summary>
/// Security analysis results
/// </summary>
public class SecurityAnalysisResult
{
    public List<SecurityVulnerability> Vulnerabilities { get; set; } = [];
    public int SecurityScore { get; set; }
    public List<string> Recommendations { get; set; } = [];
    public bool IsCompliant { get; set; }
    public List<string> ComplianceStandards { get; set; } = [];
}