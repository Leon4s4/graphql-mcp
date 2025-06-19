namespace Graphql.Mcp.DTO;

/// <summary>
/// Code generation results
/// </summary>
public class CodeGenerationResult
{
    public string GeneratedCode { get; set; } = "";
    public List<string> Files { get; set; } = [];
    public string Target { get; set; } = "";
    public List<string> Dependencies { get; set; } = [];
    public string Documentation { get; set; } = "";
    public List<string> BestPractices { get; set; } = [];
}