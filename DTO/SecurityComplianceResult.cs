namespace Graphql.Mcp.DTO;

public class SecurityComplianceResult
{
    public ComplianceCheck OwaspCompliance { get; set; } = new();
    public ComplianceCheck GraphQlBestPractices { get; set; } = new();
    public ComplianceCheck? IndustryStandards { get; set; }
    public List<string> Recommendations { get; set; } = [];
}