using System.Text.Json;

namespace Graphql.Mcp.DTO;

/// <summary>
/// Information about a dynamically generated tool
/// </summary>
public class DynamicToolInfo
{
    public string ToolName { get; set; } = "";
    public string EndpointName { get; set; } = "";
    public string OperationType { get; set; } = "";
    public string OperationName { get; set; } = "";
    public string Operation { get; set; } = "";
    public string Description { get; set; } = "";
    public JsonElement Field { get; set; }
}