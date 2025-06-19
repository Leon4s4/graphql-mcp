namespace Graphql.Mcp.DTO;

/// <summary>
/// GraphQL error details
/// </summary>
public class GraphQlError
{
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public string Path { get; set; } = "";
    public List<ErrorLocation> Locations { get; set; } = [];
}