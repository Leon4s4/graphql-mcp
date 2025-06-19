namespace Graphql.Mcp.DTO;

/// <summary>
/// Comprehensive response for smart response operations
/// </summary>
public class ComprehensiveResponse
{
    public bool Success { get; set; }
    public string ResponseId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }
    public ResponseMetadata? Metadata { get; set; }
    public AnalyticsInfo? Analytics { get; set; }
}