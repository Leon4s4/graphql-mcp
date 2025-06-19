namespace Graphql.Mcp.DTO;

/// <summary>
/// Schema architecture analysis results
/// </summary>
public class SchemaArchitectureResult
{
    public string ArchitecturePattern { get; set; } = "";
    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
    public int ComplexityScore { get; set; }
}