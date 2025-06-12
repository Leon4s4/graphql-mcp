# GraphQL MCP Server - Dynamic Tool Generation

This project now includes **automatic API-to-tools mapping** functionality that can introspect any GraphQL endpoint and
dynamically generate individual MCP tools for each available query and mutation.

## New Dynamic Tool Generation Feature

The `DynamicToolRegistry` provides the following capabilities:

### 🚀 Key Features

1. **Automatic Schema Introspection**: Analyzes any GraphQL endpoint to discover available operations
2. **Individual Tool Generation**: Creates one MCP tool per GraphQL operation (query/mutation)
3. **Smart Tool Naming**: Generates descriptive tool names with configurable prefixes
4. **Type-Safe Operations**: Preserves GraphQL type information and parameter requirements
5. **Multi-Endpoint Support**: Can register and manage multiple GraphQL endpoints
6. **Authentication Support**: Handles custom headers for authenticated endpoints
7. **Mutation Control**: Optional enabling/disabling of mutation operations
8. **Dynamic Updates**: Can refresh tools when schemas change

### 🛠️ Available Tools

The dynamic registry adds these new MCP tools:

- **`registerEndpoint`** - Register a GraphQL endpoint for automatic tool generation
- **`listDynamicTools`** - View all currently generated tools
- **`executeDynamicOperation`** - Execute any generated GraphQL operation
- **`refreshEndpointTools`** - Update tools when schemas change
- **`unregisterEndpoint`** - Remove an endpoint and its generated tools

### 📝 Usage Examples

#### 1. Register a Public GraphQL API

```json
{
  "endpoint": "https://api.github.com/graphql",
  "endpointName": "github",
  "headers": "{\"Authorization\": \"Bearer YOUR_TOKEN\"}",
  "allowMutations": false,
  "toolPrefix": "gh"
}
```

This would generate tools like:

- `gh_query_viewer` - Get the authenticated user
- `gh_query_repository` - Fetch repository information
- `gh_query_user` - Get user details
- etc.

#### 2. Register a Local Development API

```json
{
  "endpoint": "http://localhost:4000/graphql",
  "endpointName": "local_api",
  "allowMutations": true,
  "toolPrefix": "dev"
}
```

#### 3. Execute Generated Operations

```json
{
  "toolName": "gh_query_repository",
  "variables": "{\"owner\": \"microsoft\", \"name\": \"vscode\"}"
}
```

### 🔧 Implementation Details

The dynamic tool generation works by:

1. **Schema Introspection**: Uses existing `SchemaIntrospectionTools.IntrospectSchema()`
2. **Operation Analysis**: Parses Query and Mutation types to find available fields
3. **Tool Generation**: Creates GraphQL operation templates for each field
4. **Parameter Mapping**: Extracts arguments and converts them to GraphQL variables
5. **Tool Registration**: Stores tools in memory for execution via `ExecuteDynamicOperation`

### 🎯 Benefits

1. **Rapid API Integration**: No manual tool creation needed
2. **Always Up-to-Date**: Tools reflect current schema state
3. **Type Safety**: Preserves GraphQL type information
4. **Developer Productivity**: Instant access to entire GraphQL APIs
5. **Consistent Interface**: All GraphQL operations available through uniform MCP interface

### 🔄 Architecture Integration

The dynamic tools integrate seamlessly with the existing MCP server:

- Uses the same `[McpServerTool]` attribute system
- Leverages existing `GraphQLTypeHelpers` for type conversion
- Builds on proven `SchemaIntrospectionTools` foundation
- Maintains MCP standard compliance

### 🚦 Getting Started

1. Start the MCP server as usual
2. Use `registerEndpoint` to add a GraphQL API
3. Use `listDynamicTools` to see generated tools
4. Execute operations with `executeDynamicOperation`
5. Refresh when schemas change with `refreshEndpointTools`

This feature transforms the GraphQL MCP server from a general-purpose GraphQL toolkit into a dynamic, auto-configuring
interface that can instantly provide MCP tools for any GraphQL API.
