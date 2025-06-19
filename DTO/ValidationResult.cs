namespace Graphql.Mcp.DTO;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public List<string> Suggestions { get; set; } = [];
}