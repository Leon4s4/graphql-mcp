namespace Graphql.Mcp.DTO;

/// <summary>
/// Represents a field usage in a GraphQL query
/// </summary>
public class FieldUsage
{
    /// <summary>
    /// The name of the field
    /// </summary>
    public string FieldName { get; set; } = "";
    
    /// <summary>
    /// The parent type that contains this field
    /// </summary>
    public string ParentType { get; set; } = "";
    
    /// <summary>
    /// The nesting depth of this field in the query
    /// </summary>
    public int Depth { get; set; }
}
