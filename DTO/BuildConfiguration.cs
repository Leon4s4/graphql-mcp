namespace Graphql.Mcp.DTO;

public class BuildConfiguration
{
    public string? ProjectFile { get; set; }
    public string? ConfigFile { get; set; }
    public List<string> Packages { get; set; } = [];
    public List<string> Dependencies { get; set; } = [];
}