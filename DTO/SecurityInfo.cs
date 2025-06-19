namespace Graphql.Mcp.DTO;

public class SecurityInfo
{
    public List<string> RequiredRoles { get; set; } = [];
    public bool IsSensitive { get; set; }
    public List<string> SecurityNotes { get; set; } = [];
}