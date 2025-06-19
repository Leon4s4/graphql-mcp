namespace Graphql.Mcp.DTO;

public class UtilityMetrics
{
    public int InputSize { get; set; }
    public int OutputSize { get; set; }
    public double CompressionRatio { get; set; }
    public string ProcessingEfficiency { get; set; } = "";
}