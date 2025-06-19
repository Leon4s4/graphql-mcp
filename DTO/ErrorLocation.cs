namespace Graphql.Mcp.DTO;

/// <summary>
/// Error location in GraphQL query
/// </summary>
public class ErrorLocation
{
    public int Line { get; set; }
    public int Column { get; set; }
}