namespace Graphql.Mcp.DTO;

public class SchemaArchitectureAnalysisResult
{
    public List<string> ArchitecturalPatterns { get; set; } = [];
    public int BestPracticesCompliance { get; set; }
    public List<string> PotentialImprovements { get; set; } = [];
    public List<string> PerformanceConsiderations { get; set; } = [];
}