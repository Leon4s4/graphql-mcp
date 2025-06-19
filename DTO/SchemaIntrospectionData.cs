namespace Graphql.Mcp.DTO;

/// <summary>
/// Enhanced schema introspection data with smart defaults
/// </summary>
public class SchemaIntrospectionData
{
    public SchemaInfo SchemaInfo { get; set; } = new();
    public List<GraphQlTypeInfo> Types { get; set; } = [];
    public List<DirectiveInfo> Directives { get; set; } = [];
    public SchemaMetadata Metadata { get; set; } = new();
    public TypeRelationships TypeRelationships { get; set; } = new();
    public List<string> AvailableOperations { get; set; } = [];
}