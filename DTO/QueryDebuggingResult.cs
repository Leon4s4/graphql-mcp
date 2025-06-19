namespace Graphql.Mcp.DTO;

public class QueryDebuggingResult
{
    public bool IsValid { get; set; }
    public List<string> SyntaxErrors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public List<string> Suggestions { get; set; } = [];
    public QueryComplexityInfo? Complexity { get; set; }
    public int Depth { get; set; }
    public List<string> Fields { get; set; } = [];
}