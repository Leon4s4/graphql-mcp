namespace Graphql.Mcp.DTO;

public class ExecutionError
{
    public string Message { get; set; } = "";
    public List<object> Path { get; set; } = [];
    public Dictionary<string, object> Extensions { get; set; } = new();
    public List<string> Suggestions { get; set; } = [];
}