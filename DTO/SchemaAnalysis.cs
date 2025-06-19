namespace Graphql.Mcp.DTO;

/// <summary>
/// Represents the results of a schema structure analysis
/// </summary>
public class SchemaAnalysis
{
    public int TotalTypes { get; set; }
    public int QueryFields { get; set; }
    public int MutationFields { get; set; }
    public int SubscriptionFields { get; set; }
    public int CustomScalars { get; set; }
    public int Directives { get; set; }
    public string Complexity { get; set; } = "Moderate";
    public List<string> Insights { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
}