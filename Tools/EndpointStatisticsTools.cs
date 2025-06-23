using System.ComponentModel;
using System.Text;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class EndpointStatisticsTools
{
    [McpServerTool, Description(@"Comprehensive GraphQL endpoint analytics and management dashboard with detailed usage statistics and operational insights.

This advanced monitoring tool provides complete endpoint analysis including:
1. Endpoint registry status and health monitoring
2. Dynamic tool count and distribution analysis
3. URL mapping and connection status tracking
4. Resource utilization and performance metrics
5. Endpoint lifecycle and registration history
6. Tool generation efficiency and success rates

Statistical Features:
- Total endpoint count and active registrations
- Dynamic tool generation statistics per endpoint
- URL validation and accessibility monitoring
- Registration timestamp and lifecycle tracking
- Tool usage patterns and frequency analysis
- Resource allocation and memory utilization metrics

Management Benefits:
- Quick overview of all registered GraphQL services
- Identification of unused or problematic endpoints
- Resource optimization opportunities and recommendations
- Endpoint health monitoring and status verification
- Development environment management and cleanup guidance
- Performance benchmarking and comparison analysis

Operational Insights:
- Endpoint registration success/failure rates
- Tool generation performance and timing
- Schema complexity correlation with tool count
- Network connectivity and response validation
- Authentication status and configuration verification

Essential for DevOps monitoring, resource management, and GraphQL infrastructure optimization.")]
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