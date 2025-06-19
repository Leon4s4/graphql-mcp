namespace Graphql.Mcp.DTO;

/// <summary>
/// Result of comparing two GraphQL endpoints
/// </summary>
public class EndpointComparisonResult
{
    /// <summary>
    /// First endpoint name
    /// </summary>
    public string Endpoint1 { get; set; } = string.Empty;

    /// <summary>
    /// Second endpoint name
    /// </summary>
    public string Endpoint2 { get; set; } = string.Empty;

    /// <summary>
    /// Detailed schema comparison results
    /// </summary>
    public SchemaComparisonResult Comparison { get; set; } = new();

    /// <summary>
    /// Performance comparison between endpoints
    /// </summary>
    public PerformanceComparisonResult PerformanceComparison { get; set; } = new();

    /// <summary>
    /// When the comparison was performed
    /// </summary>
    public DateTime ComparedAt { get; set; }

    /// <summary>
    /// Comparison duration
    /// </summary>
    public TimeSpan ComparisonDuration { get; set; }

    /// <summary>
    /// Overall compatibility summary
    /// </summary>
    public string CompatibilitySummary { get; set; } = string.Empty;

    /// <summary>
    /// Migration effort estimate (hours)
    /// </summary>
    public int EstimatedMigrationHours { get; set; }

    /// <summary>
    /// Risk level for migration (Low, Medium, High, Critical)
    /// </summary>
    public string MigrationRiskLevel { get; set; } = "Medium";
}