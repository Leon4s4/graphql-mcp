# SchemaIntrospectionTools Refactoring Documentation

## Overview
The `SchemaIntrospectionTools` class has been successfully refactored to support dual interfaces, optimizing performance for internal tool usage while maintaining backward compatibility for MCP tool calls.

## Refactoring Pattern Applied

### Before Refactoring
```csharp
[McpServerTool]
public static async Task<string> IntrospectSchema(
    string endpointName, 
    string? headers = null)
{
    var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
    // ... rest of implementation
}
```

### After Refactoring
```csharp
// MCP Tool Interface (external)
[McpServerTool]
public static async Task<string> IntrospectSchema(
    string endpointName, 
    string? headers = null)
{
    var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
    if (endpointInfo == null)
        return $"Error: Endpoint '{endpointName}' not found...";
    
    return await IntrospectSchemaInternal(endpointInfo, headers);
}

// Internal Tool Interface (for other tools)
public static async Task<string> IntrospectSchema(
    GraphQlEndpointInfo endpointInfo, 
    string? headers = null)
{
    return await IntrospectSchemaInternal(endpointInfo, headers);
}

// Shared Implementation
private static async Task<string> IntrospectSchemaInternal(
    GraphQlEndpointInfo endpointInfo, 
    string? headers)
{
    // ... actual implementation logic
}
```

## Performance Benefits

### Registry Service Call Elimination
```csharp
// BEFORE: Tools made redundant registry calls
var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointName, headers);
// ↑ This calls GetEndpointInfo AGAIN internally

// AFTER: Tools can pass endpoint info directly
var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo, headers);
// ↑ No redundant registry lookup
```

## Methods Refactored

| Method | MCP Interface | Internal Interface | Implementation |
|--------|---------------|-------------------|----------------|
| `IntrospectSchema` | `(string endpointName, string? headers)` | `(GraphQlEndpointInfo, string? headers)` | `IntrospectSchemaInternal` |
| `GetSchemaDocs` | `(string endpointName, string? typeName, string? headers)` | `(GraphQlEndpointInfo, string? typeName, string? headers)` | `GetSchemaDocsInternal` |
| `ValidateQuery` | `(string endpointName, string query, string? headers)` | `(GraphQlEndpointInfo, string query, string? headers)` | `ValidateQueryInternal` |

## Usage Examples

### From External MCP Clients
```csharp
// Called directly by MCP clients - no change needed
var result = await SchemaIntrospectionTools.IntrospectSchema("github-api", headers);
```

### From Internal Tools (Optimized)
```csharp
// Tools that already have endpoint info can avoid redundant lookups
var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
if (endpointInfo == null) return "Error...";

// Use optimized interface
var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo, headers);
var docsJson = await SchemaIntrospectionTools.GetSchemaDocs(endpointInfo, typeName, headers);
var validationResult = await SchemaIntrospectionTools.ValidateQuery(endpointInfo, query, headers);
```

## Files Updated

### Core Refactoring
- **SchemaIntrospectionTools.cs**: Added dual interfaces and extracted shared implementation
- **GraphQLSchemaTools.cs**: Updated to use optimized interface where endpoint info was available

### Additional Optimizations Possible
The following tools still use the endpoint name interface but could be optimized:
- `SecurityAnalysisTools.cs`
- `QueryAnalyzerTools.cs` 
- `QueryValidationTools.cs`
- `TestingMockingTools.cs`
- `FieldUsageAnalyticsTools.cs`
- `ResolverDocumentationTools.cs`
- `CodeGenerationTools.cs`

## Implementation Guidelines

### When to Use Each Interface

**Use MCP Interface** (`string endpointName`) when:
- Called directly by MCP clients
- Endpoint info is not already available
- Error handling for missing endpoints is needed

**Use Internal Interface** (`GraphQlEndpointInfo endpointInfo`) when:
- Called from other tools that already have endpoint info
- Performance optimization is important
- Reducing registry service calls

### Error Handling Pattern
```csharp
public static async Task<string> SomeMethod(string endpointName)
{
    var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
    if (endpointInfo == null)
    {
        return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
    }
    
    // Use optimized interface
    return await SchemaIntrospectionTools.IntrospectSchema(endpointInfo, headers);
}
```

## Benefits Achieved

1. **Performance**: Eliminated redundant registry service lookups
2. **Maintainability**: Single source of truth for implementation logic
3. **Compatibility**: Existing MCP tool calls continue to work unchanged
4. **Consistency**: Standardized error handling and validation
5. **Flexibility**: Tools can choose the appropriate interface for their needs

## Build Status
✅ **Compilation**: Successful  
✅ **Functionality**: All existing interfaces preserved  
✅ **Performance**: Registry lookups reduced in optimized paths
