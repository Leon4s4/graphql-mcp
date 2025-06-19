namespace Graphql.Mcp.DTO;

/// <summary>
/// Result of calculating complexity metrics for a GraphQL schema
/// </summary>
public class SchemaComplexityResult
{
    /// <summary>
    /// Overall estimated complexity level (low, medium, high, very-high)
    /// </summary>
    public string EstimatedComplexity { get; set; } = "medium";

    /// <summary>
    /// Total number of types in the schema
    /// </summary>
    public int TypeCount { get; set; }

    /// <summary>
    /// Total number of fields across all types
    /// </summary>
    public int FieldCount { get; set; }

    /// <summary>
    /// Maximum query depth allowed/recommended
    /// </summary>
    public int MaxDepth { get; set; }

    /// <summary>
    /// Whether the schema contains circular references
    /// </summary>
    public bool CircularReferences { get; set; }

    /// <summary>
    /// List of complexity-based recommendations
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// Complexity score (0-100)
    /// </summary>
    public int ComplexityScore { get; set; }

    /// <summary>
    /// Number of object types
    /// </summary>
    public int ObjectTypeCount { get; set; }

    /// <summary>
    /// Number of input types
    /// </summary>
    public int InputTypeCount { get; set; }

    /// <summary>
    /// Number of enum types
    /// </summary>
    public int EnumTypeCount { get; set; }

    /// <summary>
    /// Number of interface types
    /// </summary>
    public int InterfaceTypeCount { get; set; }

    /// <summary>
    /// Number of union types
    /// </summary>
    public int UnionTypeCount { get; set; }

    /// <summary>
    /// Average fields per type
    /// </summary>
    public double AverageFieldsPerType { get; set; }

    /// <summary>
    /// Most complex type in the schema
    /// </summary>
    public string? MostComplexType { get; set; }

    /// <summary>
    /// Performance implications based on complexity
    /// </summary>
    public List<string> PerformanceImplications { get; set; } = new();
}