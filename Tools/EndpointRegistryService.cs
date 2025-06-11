using System.Text.Json;

namespace Tools;

/// <summary>
/// Singleton service to manage GraphQL endpoint registrations and dynamic tools
/// This ensures data persists across MCP tool calls
/// </summary>
public sealed class EndpointRegistryService
{
    private static readonly Lazy<EndpointRegistryService> _instance = new(() => new EndpointRegistryService());
    
    private readonly Dictionary<string, DynamicToolInfo> _dynamicTools = new();
    private readonly Dictionary<string, GraphQLEndpointInfo> _endpoints = new();
    private readonly Dictionary<string, List<string>> _endpointToTools = new();
    private readonly object _lock = new object();

    public static EndpointRegistryService Instance => _instance.Value;

    private EndpointRegistryService() { }

    #region Endpoint Management

    public void RegisterEndpoint(string endpointName, GraphQLEndpointInfo endpointInfo)
    {
        lock (_lock)
        {
            // If endpoint already exists, remove existing tools first
            if (_endpoints.ContainsKey(endpointName))
            {
                RemoveToolsForEndpointInternal(endpointName);
            }

            _endpoints[endpointName] = endpointInfo;
        }
    }

    public GraphQLEndpointInfo? GetEndpointInfo(string endpointName)
    {
        lock (_lock)
        {
            return _endpoints.TryGetValue(endpointName, out var endpointInfo) ? endpointInfo : null;
        }
    }

    public bool IsEndpointRegistered(string endpointName)
    {
        lock (_lock)
        {
            return _endpoints.ContainsKey(endpointName);
        }
    }

    public IEnumerable<string> GetRegisteredEndpointNames()
    {
        lock (_lock)
        {
            return _endpoints.Keys.ToList();
        }
    }

    public Dictionary<string, GraphQLEndpointInfo> GetAllEndpoints()
    {
        lock (_lock)
        {
            return new Dictionary<string, GraphQLEndpointInfo>(_endpoints);
        }
    }

    public bool RemoveEndpoint(string endpointName, out int toolsRemoved)
    {
        lock (_lock)
        {
            if (!_endpoints.TryGetValue(endpointName, out var endpointInfo))
            {
                toolsRemoved = 0;
                return false;
            }

            toolsRemoved = 0;

            // Use lookup map for efficient tool removal
            if (_endpointToTools.TryGetValue(endpointName, out var toolNames))
            {
                foreach (var toolName in toolNames)
                {
                    if (_dynamicTools.Remove(toolName))
                    {
                        toolsRemoved++;
                    }
                }
                _endpointToTools.Remove(endpointName);
            }

            // Remove endpoint
            _endpoints.Remove(endpointName);

            return true;
        }
    }

    #endregion

    #region Dynamic Tool Management

    public void RegisterDynamicTool(string toolName, DynamicToolInfo toolInfo)
    {
        lock (_lock)
        {
            _dynamicTools[toolName] = toolInfo;

            // Ensure the endpoint has an entry in the lookup map
            if (!_endpointToTools.ContainsKey(toolInfo.EndpointName))
            {
                _endpointToTools[toolInfo.EndpointName] = new List<string>();
            }

            _endpointToTools[toolInfo.EndpointName].Add(toolName);
        }
    }

    public DynamicToolInfo? GetDynamicTool(string toolName)
    {
        lock (_lock)
        {
            return _dynamicTools.TryGetValue(toolName, out var toolInfo) ? toolInfo : null;
        }
    }

    public Dictionary<string, DynamicToolInfo> GetAllDynamicTools()
    {
        lock (_lock)
        {
            return new Dictionary<string, DynamicToolInfo>(_dynamicTools);
        }
    }

    public int GetToolCountForEndpoint(string endpointName)
    {
        lock (_lock)
        {
            return _endpointToTools.TryGetValue(endpointName, out var toolNames) ? toolNames.Count : 0;
        }
    }

    public int RemoveToolsForEndpoint(string endpointName)
    {
        lock (_lock)
        {
            return RemoveToolsForEndpointInternal(endpointName);
        }
    }

    private int RemoveToolsForEndpointInternal(string endpointName)
    {
        var toolsRemoved = 0;

        // Use lookup map for efficient tool removal
        if (_endpointToTools.TryGetValue(endpointName, out var toolNames))
        {
            foreach (var toolName in toolNames)
            {
                if (_dynamicTools.Remove(toolName))
                {
                    toolsRemoved++;
                }
            }
            // Clear the tool list but keep the endpoint entry for future tool additions
            toolNames.Clear();
        }

        return toolsRemoved;
    }

    #endregion

    #region Statistics

    public int TotalEndpoints => _endpoints.Count;
    public int TotalDynamicTools => _dynamicTools.Count;

    #endregion
}

/// <summary>
/// Information about a GraphQL endpoint
/// </summary>
public class GraphQLEndpointInfo
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool AllowMutations { get; set; }
    public string ToolPrefix { get; set; } = "";
}

/// <summary>
/// Information about a dynamically generated tool
/// </summary>
public class DynamicToolInfo
{
    public string ToolName { get; set; } = "";
    public string EndpointName { get; set; } = "";
    public string OperationType { get; set; } = "";
    public string OperationName { get; set; } = "";
    public string Operation { get; set; } = "";
    public string Description { get; set; } = "";
    public JsonElement Field { get; set; }
}
