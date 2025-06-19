namespace Graphql.Mcp.DTO;

public class VersionInfo
{
    public string Version { get; set; } = "";
    public string GraphQlVersion { get; set; } = "";
    public DateTime ReleaseDate { get; set; }
    public List<string> Features { get; set; } = [];
}