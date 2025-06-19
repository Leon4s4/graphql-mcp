namespace Graphql.Mcp.DTO;

/// <summary>
/// Enhanced field information with comprehensive metadata
/// </summary>
public class FieldInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ArgumentInfo> Args { get; set; } = [];
    public TypeReference Type { get; set; } = new();
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }

    // Comprehensive metadata
    public List<string> ExampleValues { get; set; } = [];
    public string? UsageHint { get; set; }
    public PerformanceProfile? PerformanceProfile { get; set; }
    public List<string> ValidationRules { get; set; } = [];
    public SecurityInfo? Security { get; set; }
}