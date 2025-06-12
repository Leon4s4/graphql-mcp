namespace Graphql.Mcp.DTO;

/// <summary>
/// Represents usage statistics for a field in a GraphQL schema
/// </summary>
public class FieldUsageStats
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
    
    /// <summary>
    /// Number of times this field was used in queries
    /// </summary>
    public int UsageCount { get; set; }
}
