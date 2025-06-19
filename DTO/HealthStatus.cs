namespace Graphql.Mcp.DTO;

public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastChecked { get; set; }
    public List<string> Issues { get; set; } = [];
}