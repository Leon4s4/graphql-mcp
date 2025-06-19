namespace Graphql.Mcp.DTO;

/// <summary>
/// Result of analyzing schema complexity for an endpoint
/// </summary>
public class SchemaAnalysisResult
{
    /// <summary>
    /// The endpoint that was analyzed
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Detailed complexity metrics
    /// </summary>
    public SchemaComplexityResult Complexity { get; set; } = new();

    /// <summary>
    /// Optimization recommendations
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// When the analysis was performed
    /// </summary>
    public DateTime AnalyzedAt { get; set; }

    /// <summary>
    /// Analysis duration
    /// </summary>
    public TimeSpan AnalysisDuration { get; set; }

    /// <summary>
    /// Schema version or hash if available
    /// </summary>
    public string? SchemaVersion { get; set; }

    /// <summary>
    /// Overall health rating (A-F)
    /// </summary>
    public string HealthRating { get; set; } = "C";

    /// <summary>
    /// Critical issues found during analysis
    /// </summary>
    public List<string> CriticalIssues { get; set; } = new();

    /// <summary>
    /// Performance warnings
    /// </summary>
    public List<string> PerformanceWarnings { get; set; } = new();
}