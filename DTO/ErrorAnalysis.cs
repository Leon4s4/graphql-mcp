namespace Graphql.Mcp.DTO;

/// <summary>
/// Error analysis result
/// </summary>
public class ErrorAnalysis
{
    public string Explanation { get; set; } = "";
    public List<string> Solutions { get; set; } = [];
    public string Severity { get; set; } = "";
}