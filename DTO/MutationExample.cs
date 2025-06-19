namespace Graphql.Mcp.DTO;

/// <summary>
/// Mutation example with smart defaults
/// </summary>
public class MutationExample
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Mutation { get; set; } = "";
    public Dictionary<string, object> Variables { get; set; } = new();
    public object? ExpectedResult { get; set; }
    public List<string> Tags { get; set; } = [];
    public int ComplexityScore { get; set; }
    public List<string> SideEffects { get; set; } = [];
    public List<string> RequiredPermissions { get; set; } = [];
    public bool IsIdempotent { get; set; }
}