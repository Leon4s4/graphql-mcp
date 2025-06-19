namespace Graphql.Mcp.DTO;

public class SchemaInfo
{
    public TypeReference? QueryType { get; set; }
    public TypeReference? MutationType { get; set; }
    public TypeReference? SubscriptionType { get; set; }
    public DateTime LastModified { get; set; }
    public string Version { get; set; } = "";
}