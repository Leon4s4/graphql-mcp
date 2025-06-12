namespace Graphql.Mcp.DTO;

/// <summary>
/// Represents a field in a GraphQL schema
/// </summary>
public class SchemaField
{
    /// <summary>
    /// The name of the type containing this field
    /// </summary>
    public string TypeName { get; set; } = "";

    /// <summary>
    /// The name of the field
    /// </summary>
    public string FieldName { get; set; } = "";

    /// <summary>
    /// The GraphQL type of the field
    /// </summary>
    public string FieldType { get; set; } = "";
}