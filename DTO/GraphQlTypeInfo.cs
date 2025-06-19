namespace Graphql.Mcp.DTO;

/// <summary>
/// Enhanced type information with usage examples and metadata
/// </summary>
public class GraphQlTypeInfo
{
    public TypeKind Kind { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<FieldInfo> Fields { get; set; } = [];
    public List<InputFieldInfo> InputFields { get; set; } = [];
    public List<TypeReference> Interfaces { get; set; } = [];
    public List<EnumValueInfo> EnumValues { get; set; } = [];

    // Smart default extensions
    public List<string> ExampleUsages { get; set; } = [];
    public List<QueryExample> RelatedQueries { get; set; } = [];
    public Dictionary<string, object> Extensions { get; set; } = new();
    public UsageStatistics? UsageStats { get; set; }
    public ComplexityMetrics? Complexity { get; set; }
}