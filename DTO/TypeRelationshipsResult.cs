namespace Graphql.Mcp.DTO;

/// <summary>
/// Type relationships information
/// </summary>
public class TypeRelationshipsResult
{
    public List<string> DirectRelationships { get; set; } = [];
    public List<string> IndirectRelationships { get; set; } = [];
    public int MaxDepth { get; set; }
    public Dictionary<string, List<string>> RelationshipMap { get; set; } = new();
}