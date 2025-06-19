namespace Graphql.Mcp.DTO;

public class ErrorAnalysisResult
{
    public string ErrorType { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public List<string> PossibleCauses { get; set; } = [];
    public List<string> SuggestedFixes { get; set; } = [];
    public string Severity { get; set; } = "";
}