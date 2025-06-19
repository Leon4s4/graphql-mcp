namespace Graphql.Mcp.DTO;

/// <summary>
/// Schema context for query results
/// </summary>
public class SchemaContext
{
    public List<GraphQlTypeInfo> ReferencedTypes { get; set; } = [];
    public List<string> AvailableFields { get; set; } = [];
    public List<string> RequiredArguments { get; set; } = [];
    public Dictionary<string, List<string>> EnumValues { get; set; } = new();
    public List<string> RelatedOperations { get; set; } = [];
}