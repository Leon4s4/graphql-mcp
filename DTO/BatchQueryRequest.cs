namespace Graphql.Mcp.DTO;

public class BatchQueryRequest
{
    public string? Id { get; set; }
    public string Query { get; set; } = "";
    public Dictionary<string, object> Variables { get; set; } = new();
}