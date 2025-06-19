namespace Graphql.Mcp.DTO;

/// <summary>
/// Utility operation results
/// </summary>
public class UtilityOperationResult
{
    public string Result { get; set; } = "";
    public List<string> Options { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
    public Dictionary<string, object> Metrics { get; set; } = new();
}