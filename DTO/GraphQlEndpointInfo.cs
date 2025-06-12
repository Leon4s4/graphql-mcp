namespace Graphql.Mcp.DTO;

/// <summary>
/// Information about a GraphQL endpoint
/// </summary>
public class GraphQlEndpointInfo
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool AllowMutations { get; set; }
    public string ToolPrefix { get; set; } = "";
    public string? SchemaContent { get; set; } = "";
}