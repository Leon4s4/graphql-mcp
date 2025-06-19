namespace Graphql.Mcp.DTO;

/// <summary>
/// Result of comparing two GraphQL schemas
/// </summary>
public class SchemaComparisonResult
{
    /// <summary>
    /// Whether the schemas are compatible
    /// </summary>
    public bool Compatible { get; set; }

    /// <summary>
    /// List of non-breaking differences between schemas
    /// </summary>
    public List<string> Differences { get; set; } = new();

    /// <summary>
    /// List of breaking changes that would affect client compatibility
    /// </summary>
    public List<string> BreakingChanges { get; set; } = new();

    /// <summary>
    /// Types that were added in the second schema
    /// </summary>
    public List<string> AddedTypes { get; set; } = new();

    /// <summary>
    /// Types that were removed from the first schema
    /// </summary>
    public List<string> RemovedTypes { get; set; } = new();

    /// <summary>
    /// Fields that were added
    /// </summary>
    public List<string> AddedFields { get; set; } = new();

    /// <summary>
    /// Fields that were removed
    /// </summary>
    public List<string> RemovedFields { get; set; } = new();

    /// <summary>
    /// Arguments that were added
    /// </summary>
    public List<string> AddedArguments { get; set; } = new();

    /// <summary>
    /// Arguments that were removed
    /// </summary>
    public List<string> RemovedArguments { get; set; } = new();

    /// <summary>
    /// Directives that changed
    /// </summary>
    public List<string> DirectiveChanges { get; set; } = new();

    /// <summary>
    /// Overall compatibility rating
    /// </summary>
    public string CompatibilityRating { get; set; } = "Unknown";

    /// <summary>
    /// Recommended migration steps if there are breaking changes
    /// </summary>
    public List<string> MigrationSteps { get; set; } = new();
}