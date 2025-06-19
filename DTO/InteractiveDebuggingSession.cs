namespace Graphql.Mcp.DTO;

public class InteractiveDebuggingSession
{
    public string SessionId { get; set; } = "";
    public string Focus { get; set; } = "";
    public List<string> AvailableCommands { get; set; } = [];
    public Dictionary<string, string> DebugInfo { get; set; } = new();
}