namespace Graphql.Mcp.DTO;

/// <summary>
/// Validation issue for GraphQL queries
/// </summary>
public class ValidationIssue
{
    public string Type { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Message { get; set; } = "";
    public int? Line { get; set; }
    public int? Column { get; set; }
    public string? Suggestion { get; set; }
    public string? Location { get; set; }
    public string? Fix { get; set; }
}