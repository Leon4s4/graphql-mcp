using System.Collections.Concurrent;
using Graphql.Mcp.DTO;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Singleton service to manage GraphQL endpoint registrations and dynamic tools
/// This ensures data persists across MCP tool calls
/// </summary>
public sealed class EndpointRegistryService
{
    private static readonly Lazy<EndpointRegistryService> EndpointRegistryServiceInstance = new(() => new EndpointRegistryService());
    
    private readonly ConcurrentDictionary<string, DynamicToolInfo> _dynamicTools = new();
    private readonly ConcurrentDictionary<string, GraphQlEndpointInfo> _endpoints = new();
    private readonly ConcurrentDictionary<string, List<string>> _endpointToTools = new();

    public static EndpointRegistryService Instance => EndpointRegistryServiceInstance.Value;

    private EndpointRegistryService() { }

    #region Endpoint Management

    public void RegisterEndpoint(string endpointName, GraphQlEndpointInfo endpointInfo)
    {
        if (_endpoints.ContainsKey(endpointName))
        {
            RemoveToolsForEndpointInternal(endpointName);
        }
        
        endpointInfo.SchemaContent = LoadSchemaContentFromFile();
        _endpoints[endpointName] = endpointInfo;
    }

    private static string? LoadSchemaContentFromFile()
    {
        var schemaPath = Environment.GetEnvironmentVariable("SCHEMA");
        if (!string.IsNullOrWhiteSpace(schemaPath) && File.Exists(schemaPath))
            return File.ReadAllText(schemaPath);
        
        return null;
    }

    public GraphQlEndpointInfo? GetEndpointInfo(string endpointName)
    {
        return _endpoints.GetValueOrDefault(endpointName);
    }

    public bool IsEndpointRegistered(string endpointName)
    {
        return _endpoints.ContainsKey(endpointName);
    }

    public IEnumerable<string> GetRegisteredEndpointNames()
    {
        return _endpoints.Keys.ToList();
    }

    public Dictionary<string, GraphQlEndpointInfo> GetAllEndpoints()
    {
        return new Dictionary<string, GraphQlEndpointInfo>(_endpoints);
    }

    public bool RemoveEndpoint(string endpointName, out int toolsRemoved)
    {
        if (!_endpoints.TryGetValue(endpointName, out var endpointInfo))
        {
            toolsRemoved = 0;
            return false;
        }

        toolsRemoved = 0;

        if (_endpointToTools.TryGetValue(endpointName, out var toolNames))
        {
            foreach (var toolName in toolNames)
            {
                if (_dynamicTools.TryRemove(toolName, out _))
                {
                    toolsRemoved++;
                }
            }
            _endpointToTools.TryRemove(endpointName, out _);
        }

        _endpoints.TryRemove(endpointName, out _);

        return true;
    }

    #endregion

    #region Dynamic Tool Management

    public void RegisterDynamicTool(string toolName, DynamicToolInfo toolInfo)
    {
        _dynamicTools[toolName] = toolInfo;

        _endpointToTools.AddOrUpdate(
            toolInfo.EndpointName,
            [toolName],
            (_, existingList) =>
            {
                lock (existingList)
                {
                    existingList.Add(toolName);
                    return existingList;
                }
            });
    }

    public DynamicToolInfo? GetDynamicTool(string toolName)
    {
        return _dynamicTools.GetValueOrDefault(toolName);
    }

    public IReadOnlyDictionary<string, DynamicToolInfo> GetAllDynamicTools() => _dynamicTools;

    public int GetToolCountForEndpoint(string endpointName) => _endpointToTools.TryGetValue(endpointName, out var toolNames) ? toolNames.Count : 0;

    public int RemoveToolsForEndpoint(string endpointName) => RemoveToolsForEndpointInternal(endpointName);

    private int RemoveToolsForEndpointInternal(string endpointName)
    {
        var toolsRemoved = 0;

        if (_endpointToTools.TryGetValue(endpointName, out var toolNames))
        {
            lock (toolNames)
            {
                toolsRemoved += toolNames.Count(toolName => _dynamicTools.TryRemove(toolName, out _));

                toolNames.Clear();
            }
        }

        return toolsRemoved;
    }

    #endregion

    #region Statistics

    public int TotalEndpoints => _endpoints.Count;
    public int TotalDynamicTools => _dynamicTools.Count;

    #endregion
}