namespace Graphql.Mcp.DTO;

/// <summary>
/// Development guide information
/// </summary>
public class DevelopmentGuide
{
    public List<string> Steps { get; set; } = [];
    public List<string> BestPractices { get; set; } = [];
    public List<string> Examples { get; set; } = [];
    public Dictionary<string, string> Resources { get; set; } = new();
}