namespace Graphql.Mcp.DTO;

public class ComplianceCheck
{
    public bool IsCompliant { get; set; }
    public int Score { get; set; }
    public List<string> Issues { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
}