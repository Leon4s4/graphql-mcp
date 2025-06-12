using System.ComponentModel;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class EndpointManagementTools
{
    [McpServerTool, Description("List all registered GraphQL endpoints")]
    public static string GetAllEndpoints()
    {
        var endpoints = EndpointRegistryService.Instance.GetAllEndpoints();
        
        if (endpoints.Count == 0)
        {
            return "No GraphQL endpoints are currently registered. Use RegisterEndpoint to add an endpoint.";
        }
        
        var result = new System.Text.StringBuilder();
        result.AppendLine("# Registered GraphQL Endpoints\n");
        
        foreach (var endpoint in endpoints)
        {
            result.AppendLine($"## {endpoint.Key}");
            result.AppendLine($"**URL:** {endpoint.Value.Url}");
            
            if (endpoint.Value.Headers.Count > 0)
            {
                result.AppendLine($"**Headers:** {endpoint.Value.Headers.Count} custom header(s)");
            }
            
            result.AppendLine($"**Allows Mutations:** {(endpoint.Value.AllowMutations ? "Yes" : "No")}");
            
            if (!string.IsNullOrEmpty(endpoint.Value.ToolPrefix))
            {
                result.AppendLine($"**Tool Prefix:** {endpoint.Value.ToolPrefix}");
            }
            
            var toolCount = EndpointRegistryService.Instance.GetToolCountForEndpoint(endpoint.Key);
            result.AppendLine($"**Dynamic Tools:** {toolCount}");
            result.AppendLine();
        }
        
        return result.ToString();
    }
}
