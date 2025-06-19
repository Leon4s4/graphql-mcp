namespace Graphql.Mcp.DTO;

/// <summary>
/// Test generation results
/// </summary>
public class TestGenerationResult
{
    public List<string> TestFiles { get; set; } = [];
    public string Framework { get; set; } = "";
    public List<string> Dependencies { get; set; } = [];
    public string SetupInstructions { get; set; } = "";
    public List<string> MockData { get; set; } = [];
}