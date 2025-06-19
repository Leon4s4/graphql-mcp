namespace Graphql.Mcp.DTO;

public class ArgumentInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public TypeReference Type { get; set; } = new();
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public List<string> ValidValues { get; set; } = [];
}