namespace Graphql.Mcp.DTO;

public class ProjectStructure
{
    public string RootFolder { get; set; } = "";
    public List<string> Folders { get; set; } = [];
    public List<string> Files { get; set; } = [];
}