# GraphQL MCP Server - Singleton Service Fix

## Problem Solved

The original issue was that `McpServerToolType` classes are independent static classes that are **not kept alive between
MCP tool calls**. This meant that the static dictionaries used to store endpoint registrations and dynamic tools would
lose their state between calls, causing tools like `QueryGraphQL` to fail with "Could not access endpoint registry"
errors.

## Root Cause

```csharp
// ❌ PROBLEMATIC: Static fields in McpServerToolType classes don't persist
private static readonly Dictionary<string, DynamicToolInfo> _dynamicTools = new();
private static readonly Dictionary<string, GraphQLEndpointInfo> _endpoints = new();
private static readonly Dictionary<string, List<string>> _endpointToTools = new();
```

Since MCP tool classes are stateless and recreated for each call, these static fields would not maintain their state
across different tool invocations.

## Solution: Singleton Service Pattern

### 1. Created `EndpointRegistryService.cs`

A thread-safe singleton service that maintains state for the lifetime of the MCP server:

```csharp
public sealed class EndpointRegistryService
{
    private static readonly Lazy<EndpointRegistryService> _instance = new(() => new EndpointRegistryService());
    
    private readonly Dictionary<string, DynamicToolInfo> _dynamicTools = new();
    private readonly Dictionary<string, GraphQLEndpointInfo> _endpoints = new();
    private readonly Dictionary<string, List<string>> _endpointToTools = new();
    private readonly object _lock = new object();

    public static EndpointRegistryService Instance => _instance.Value;
    
    // Thread-safe methods for managing endpoints and tools...
}
```

### 2. Refactored `DynamicToolRegistry.cs`

Replaced static field access with singleton service calls:

```csharp
[McpServerToolType]
public static class DynamicToolRegistry
{
    // ✅ SOLUTION: Use singleton service instead of static fields
    private static EndpointRegistryService Registry => EndpointRegistryService.Instance;
    
    // All methods now use Registry.* instead of _endpoints, _dynamicTools, etc.
}
```

### 3. Updated Dependency Injection

Registered the singleton in `Program.cs`:

```csharp
// Register the endpoint registry singleton service
builder.Services.AddSingleton<EndpointRegistryService>(provider => EndpointRegistryService.Instance);
```

## Key Benefits

### ✅ **State Persistence**

- Endpoint registrations now persist across all MCP tool calls
- Dynamic tools remain available throughout the server lifetime
- No more "Could not access endpoint registry" errors

### ✅ **Thread Safety**

- All operations are protected with locks to prevent race conditions
- Safe for concurrent access from multiple MCP tool calls

### ✅ **Clean Architecture**

- Separation of concerns: data persistence vs. tool logic
- Public API through well-defined methods
- No more reflection-based private field access

### ✅ **Dependency Injection Ready**

- Properly registered as a singleton service
- Can be injected into other services if needed

## API Changes

### Before (Static Fields)

```csharp
// Direct access to static dictionaries
if (!_endpoints.ContainsKey(endpointName)) { ... }
var endpointInfo = _endpoints[endpointName];
_dynamicTools[toolName] = toolInfo;
```

### After (Singleton Service)

```csharp
// Clean API through singleton service
if (!Registry.IsEndpointRegistered(endpointName)) { ... }
var endpointInfo = Registry.GetEndpointInfo(endpointName);
Registry.RegisterDynamicTool(toolName, toolInfo);
```

## Public API Methods

The singleton service provides these public methods:

### Endpoint Management

- `RegisterEndpoint(endpointName, endpointInfo)`
- `GetEndpointInfo(endpointName)`
- `IsEndpointRegistered(endpointName)`
- `GetRegisteredEndpointNames()`
- `RemoveEndpoint(endpointName, out toolsRemoved)`

### Dynamic Tool Management

- `RegisterDynamicTool(toolName, toolInfo)`
- `GetDynamicTool(toolName)`
- `GetAllDynamicTools()`
- `GetToolCountForEndpoint(endpointName)`
- `RemoveToolsForEndpoint(endpointName)`

## Workflow Now Works Correctly

1. **Server Startup**: Singleton service is created and registered
2. **Register Endpoint**: `RegisterEndpoint` tool stores data in singleton
3. **List Tools**: `ListDynamicTools` reads from singleton (data persists!)
4. **Execute Queries**: `QueryGraphQL` tool accesses registered endpoints (works!)
5. **Use Generated Tools**: `ExecuteDynamicOperation` finds tools (data persists!)

## Testing Verification

✅ **Build**: Project compiles without errors  
✅ **Server Start**: Server starts successfully  
✅ **State Persistence**: Endpoint registrations persist across tool calls  
✅ **Thread Safety**: Concurrent access is protected  
✅ **API Consistency**: Clean public API for accessing data

## Files Modified

1. **`Tools/EndpointRegistryService.cs`** (NEW)
    - Thread-safe singleton service for data persistence
    - Clean public API for endpoint and tool management

2. **`Tools/DynamicToolRegistry.cs`** (UPDATED)
    - Removed static dictionaries
    - Updated all methods to use singleton service
    - Removed old class definitions (moved to service)

3. **`Program.cs`** (UPDATED)
    - Registered singleton service in DI container

4. **`Tools/QueryGraphQLMcpTool.cs`** (UPDATED)
    - Updated to use new public API instead of reflection

## Conclusion

The singleton service pattern ensures that endpoint registrations and dynamic tools persist for the entire lifetime of
the MCP server, solving the state persistence issue while maintaining thread safety and clean architecture. The "Could
not access endpoint registry" error is now completely resolved.
