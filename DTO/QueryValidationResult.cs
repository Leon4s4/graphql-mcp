namespace Graphql.Mcp.DTO;

/// <summary>
/// Query validation results
/// </summary>
public class QueryValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public string EstimatedExecutionTime { get; set; } = "";
}