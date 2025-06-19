namespace Graphql.Mcp.DTO;

public class ExecutionResult
{
    public object? Data { get; set; }
    public List<ExecutionError>? Errors { get; set; }
}