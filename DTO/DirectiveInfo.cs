namespace Graphql.Mcp.DTO;

public class DirectiveInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Locations { get; set; } = [];
    public List<ArgumentInfo> Args { get; set; } = [];
}