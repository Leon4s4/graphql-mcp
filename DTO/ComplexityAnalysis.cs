namespace Graphql.Mcp.DTO;

/// <summary>
/// Represents the results of a complexity analysis for GraphQL fields
/// </summary>
public class ComplexityAnalysis
{
    /// <summary>
    /// The total complexity score for all analyzed fields
    /// </summary>
    public int TotalComplexity { get; set; }

    /// <summary>
    /// The average complexity per field
    /// </summary>
    public double AverageComplexity { get; set; }

    /// <summary>
    /// The name of the most complex field
    /// </summary>
    public string MostComplexField { get; set; } = "";

    /// <summary>
    /// List of fields that have significantly higher complexity than average
    /// </summary>
    public List<string> HighComplexityFields { get; set; } = [];
}