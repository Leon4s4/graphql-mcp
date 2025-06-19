namespace Graphql.Mcp.DTO;

public class EnumValueInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }
}