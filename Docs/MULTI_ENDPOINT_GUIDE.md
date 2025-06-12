# GraphQL MCP Server - Multi-Endpoint Configuration

The GraphQL MCP Server now supports **dynamic multi-endpoint configuration** through the Dynamic Tool Registry system.
This allows you to register and manage multiple GraphQL endpoints without pre-configuration.

## 🚀 Key Changes

### **Dynamic Endpoint Management**

- **No Pre-Configuration Required**: No need to set `ENDPOINT`, `HEADERS`, or `ALLOW_MUTATIONS` environment variables
- **Runtime Registration**: Register GraphQL endpoints dynamically using MCP tools
- **Multi-Endpoint Support**: Handle multiple GraphQL APIs simultaneously
- **Flexible Authentication**: Configure headers per endpoint

### **New Workflow**

1. **Start the Server**: `dotnet run` (no endpoint configuration needed)
2. **Register Endpoints**: Use `RegisterEndpoint` tool to add GraphQL APIs
3. **Execute Queries**: Use `QueryGraphQL` tool or dynamically generated tools
4. **Manage Endpoints**: List, refresh, or remove endpoints as needed

## 🛠️ Available Tools

### **Endpoint Management**

- `RegisterEndpoint` - Register a GraphQL endpoint for tool generation and queries
- `ListDynamicTools` - View all registered endpoints and generated tools
- `RefreshEndpointTools` - Update tools when schemas change
- `UnregisterEndpoint` - Remove an endpoint and its tools

### **Query Execution**

- `QueryGraphQL` - Execute queries against any registered endpoint
- `ExecuteDynamicOperation` - Execute auto-generated operation tools
- Dynamic tools (auto-generated per endpoint operations)

## 📝 Usage Examples

### 1. Register a GraphQL Endpoint

```json
{
  "endpoint": "https://api.github.com/graphql",
  "endpointName": "github",
  "headers": "{\"Authorization\": \"Bearer YOUR_TOKEN\"}",
  "allowMutations": false,
  "toolPrefix": "gh"
}
```

### 2. Execute a Query

```json
{
  "query": "query { viewer { login name } }",
  "endpointName": "github"
}
```

### 3. Execute with Variables

```json
{
  "query": "query($owner: String!, $name: String!) { repository(owner: $owner, name: $name) { stargazerCount } }",
  "endpointName": "github",
  "variables": "{\"owner\": \"microsoft\", \"name\": \"vscode\"}"
}
```

### 4. Use Auto-Generated Tools

After registering an endpoint, individual tools are generated for each GraphQL operation:

- `gh_query_viewer` - Get authenticated user info
- `gh_query_repository` - Fetch repository details
- `gh_query_user` - Get user information

## 🔄 Migration from Single Endpoint

### **Old Approach (Environment Variables)**

```bash
ENDPOINT=http://localhost:4000/graphql
HEADERS={"Authorization": "Bearer token"}
ALLOW_MUTATIONS=true
```

### **New Approach (Dynamic Registration)**

```json
// Use RegisterEndpoint tool
{
  "endpoint": "http://localhost:4000/graphql",
  "endpointName": "local_api",
  "headers": "{\"Authorization\": \"Bearer token\"}",
  "allowMutations": true,
  "toolPrefix": "local"
}
```

## 🎯 Benefits

1. **No Configuration Required**: Start server immediately without setup
2. **Multiple APIs**: Connect to different GraphQL services simultaneously
3. **Per-Endpoint Settings**: Different authentication and mutation policies
4. **Auto-Tool Generation**: Instant access to all endpoint operations
5. **Runtime Flexibility**: Add, remove, or modify endpoints without restart

## 🚦 Getting Started

1. **Start the Server**
   ```bash
   dotnet run
   ```

2. **Connect MCP Client** (Claude Desktop, etc.)

3. **Register Your First Endpoint**
    - Use the `RegisterEndpoint` tool
    - Provide endpoint URL, name, and optional headers/settings

4. **Start Querying**
    - Use `QueryGraphQL` for custom queries
    - Use auto-generated tools for specific operations
    - Use `ListDynamicTools` to see all available tools

## 📊 Endpoint Features

- **Authentication**: Custom headers per endpoint
- **Mutation Control**: Enable/disable mutations per endpoint
- **Tool Prefixes**: Avoid naming conflicts between endpoints
- **Schema Tracking**: Automatic tool updates when schemas change
- **Error Handling**: Detailed error context and suggestions

This new approach makes the GraphQL MCP Server more flexible and powerful, enabling seamless integration with multiple
GraphQL APIs in a single session.
