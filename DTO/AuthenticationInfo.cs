namespace Graphql.Mcp.DTO;

public class AuthenticationInfo
{
    public List<string> SupportedMethods { get; set; } = [];
    public bool Required { get; set; }
    public Dictionary<string, string> Configuration { get; set; } = new();
}