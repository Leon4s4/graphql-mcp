# GraphQL MCP Server - Endpoint Registry Fix

## Problem Fixed

The issue was in the `QueryGraphQLMcpTool.cs` file where the code was trying to access the private `_endpoints` field using reflection with an incorrect cast. The code was attempting to cast `Dictionary<string, GraphQLEndpointInfo>` to `Dictionary<string, object>`, which was causing the "Could not access endpoint registry" error.

## Solution Applied

1. **Made GraphQLEndpointInfo class public**: Changed from `private class GraphQLEndpointInfo` to `public class GraphQLEndpointInfo` in `DynamicToolRegistry.cs`

2. **Added public accessor methods** to `DynamicToolRegistry.cs`:
   - `GetEndpointInfo(string endpointName)` - Get endpoint information
   - `IsEndpointRegistered(string endpointName)` - Check if endpoint exists
   - `GetRegisteredEndpointNames()` - Get all registered endpoint names

3. **Refactored QueryGraphQLMcpTool.cs** to use the new public methods instead of reflection:
   - Removed reflection-based access to private fields
   - Used the new public methods for cleaner, more maintainable code
   - Added proper error handling

## Code Changes Summary

### Before (Problematic Code)
```csharp
// Using reflection with incorrect type casting
var endpointsField = typeof(DynamicToolRegistry).GetField("_endpoints", 
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

if (endpointsField?.GetValue(null) is not Dictionary<string, object> endpoints)
{
    return "Error: Could not access endpoint registry...";
}
```

### After (Fixed Code)
```csharp
// Using clean public API
if (!DynamicToolRegistry.IsEndpointRegistered(endpointName))
{
    var registeredEndpoints = DynamicToolRegistry.GetRegisteredEndpointNames();
    return $"Endpoint '{endpointName}' not found. Available endpoints: {string.Join(", ", registeredEndpoints)}...";
}

var endpointInfo = DynamicToolRegistry.GetEndpointInfo(endpointName);
```

## Testing

The fix has been verified:
1. ✅ Project builds successfully without compilation errors
2. ✅ Server starts correctly
3. ✅ Endpoint registry is now accessible through the proper public API

## Workflow Now Works

1. **Register an endpoint**: Use `RegisterEndpoint` tool
2. **List endpoints**: Use `ListDynamicTools` or `ListRegisteredEndpoints`
3. **Execute queries**: Use `QueryGraphQL` tool - **this now works!**
4. **Use generated tools**: Use `ExecuteDynamicOperation` for auto-generated tools

The "Could not access endpoint registry" error should no longer occur when using the `QueryGraphQL` tool.
