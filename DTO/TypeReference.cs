namespace Graphql.Mcp.DTO;

public class TypeReference
{
    public TypeKind? Kind { get; set; }
    public string? Name { get; set; }
    public TypeReference? OfType { get; set; }
}