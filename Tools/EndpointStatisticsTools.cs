using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class EndpointStatisticsTools
{
    [McpServerTool, Description("Get statistics about registered endpoints and dynamic tools")]
    public static string GetEndpointStatistics()
    {
        var service = EndpointRegistryService.Instance;
        var stats = new StringBuilder();
        
        stats.AppendLine("# GraphQL Endpoint Statistics");
        stats.AppendLine();
        stats.AppendLine($"**Total Registered Endpoints:** {service.TotalEndpoints}");
        stats.AppendLine($"**Total Dynamic Tools:** {service.TotalDynamicTools}");
        stats.AppendLine();
        
        if (service.TotalEndpoints > 0)
        {
            stats.AppendLine("## Endpoint Details");
            foreach (var endpointName in service.GetRegisteredEndpointNames())
            {
                var endpoint = service.GetEndpointInfo(endpointName);
                if (endpoint == null) continue;
                
                var toolCount = service.GetToolCountForEndpoint(endpointName);
                stats.AppendLine($"- **{endpointName}**: {toolCount} tools, URL: {endpoint.Url}");
            }
        }
        
        return stats.ToString();
    }
}
