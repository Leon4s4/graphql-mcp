namespace Graphql.Mcp.DTO;

/// <summary>
/// Error information from GraphQL response
/// </summary>
public class ErrorInfo
{
    public bool IsGraphQlError { get; set; }
    public List<GraphQlError> Errors { get; set; } = [];
}