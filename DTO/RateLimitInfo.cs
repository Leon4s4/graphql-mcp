namespace Graphql.Mcp.DTO;

public class RateLimitInfo
{
    public int RequestsPerMinute { get; set; }
    public int RequestsPerHour { get; set; }
    public int CurrentUsage { get; set; }
    public DateTime ResetTime { get; set; }
}