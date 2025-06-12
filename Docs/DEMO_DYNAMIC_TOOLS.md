# Demo: Automatic API-to-Tools Mapping

This demonstration shows how the new **automatic API-to-tools mapping** feature works with the GraphQL MCP server.

## Overview

The dynamic tool generation feature can introspect any GraphQL endpoint and automatically create individual MCP tools for each available query and mutation. This means you can connect to any GraphQL API and immediately have all its operations available as separate MCP tools.

## Step-by-Step Demo

### 1. Start the MCP Server

First, start the GraphQL MCP server:

```bash
cd /Users/Git/graphql-mcp
dotnet run
```

### 2. Register a GraphQL Endpoint

Use the `registerEndpoint` tool to add a GraphQL API. For example, with a public API like Countries GraphQL API:

**Tool:** `registerEndpoint`
**Parameters:**
```json
{
  "endpoint": "https://countries.trevorblades.com/",
  "endpointName": "countries",
  "allowMutations": false,
  "toolPrefix": "country"
}
```

This will:
- Introspect the Countries GraphQL schema
- Generate individual tools for each query operation
- Name them with the "country" prefix

### 3. List Generated Tools

Use the `listDynamicTools` tool to see what was created:

**Tool:** `listDynamicTools`

Expected output:
```
# Registered Dynamic Tools

## Endpoint: countries
**URL:** https://countries.trevorblades.com/
**Operations:** 3

### Queries
- **country_query_countries**: Get all countries
- **country_query_country**: Get country by code  
- **country_query_continents**: Get all continents
```

### 4. Execute Generated Operations

Now you can execute any of the generated tools using `executeDynamicOperation`:

**Tool:** `executeDynamicOperation`
**Parameters:**
```json
{
  "toolName": "country_query_countries",
  "variables": "{}"
}
```

Or with parameters:
```json
{
  "toolName": "country_query_country", 
  "variables": "{\"code\": \"US\"}"
}
```

### 5. Register Another Endpoint

You can register multiple endpoints. For example, a local development API:

**Tool:** `registerEndpoint`
**Parameters:**
```json
{
  "endpoint": "http://localhost:4000/graphql",
  "endpointName": "local_dev",
  "allowMutations": true,
  "toolPrefix": "dev"
}
```

### 6. Refresh When Schema Changes

If the GraphQL schema changes, refresh the tools:

**Tool:** `refreshEndpointTools`
**Parameters:**
```json
{
  "endpointName": "countries"
}
```

## Real-World Examples

### GitHub GraphQL API

Register the GitHub API (requires authentication):

```json
{
  "endpoint": "https://api.github.com/graphql",
  "endpointName": "github",
  "headers": "{\"Authorization\": \"Bearer YOUR_GITHUB_TOKEN\"}",
  "allowMutations": false,
  "toolPrefix": "gh"
}
```

Generated tools would include:
- `gh_query_viewer` - Get authenticated user
- `gh_query_repository` - Get repository information  
- `gh_query_organization` - Get organization details
- `gh_query_user` - Get user profile
- etc.

### E-commerce API Example

```json
{
  "endpoint": "https://api.shop.com/graphql",
  "endpointName": "shop",
  "headers": "{\"Authorization\": \"Bearer API_KEY\"}",
  "allowMutations": true,
  "toolPrefix": "shop"
}
```

Generated tools could include:
- **Queries:** `shop_query_products`, `shop_query_orders`, `shop_query_customers`
- **Mutations:** `shop_mutation_createOrder`, `shop_mutation_updateProduct`, etc.

## Key Benefits Demonstrated

1. **Zero Manual Configuration**: No need to manually create tools for each GraphQL operation
2. **Instant API Integration**: Any GraphQL API becomes immediately usable through MCP
3. **Type-Safe Operations**: All GraphQL types and parameters are preserved
4. **Multi-API Support**: Can work with multiple GraphQL endpoints simultaneously  
5. **Authentication Support**: Handles authenticated APIs through custom headers
6. **Dynamic Updates**: Tools stay in sync with schema changes

## Architecture Highlights

The automatic tool generation:

1. **Leverages Existing Infrastructure**: Uses proven `SchemaIntrospectionTools` and `GraphQLTypeHelpers`
2. **Maintains MCP Compliance**: All generated tools follow MCP standards
3. **Preserves Type Information**: GraphQL types are properly converted and maintained
4. **Provides Execution Engine**: `ExecuteDynamicOperation` handles all generated tool execution
5. **Manages Tool Lifecycle**: Registration, updates, and cleanup are all handled automatically

This feature transforms the GraphQL MCP server from a general-purpose toolkit into a dynamic, auto-configuring interface that can provide instant MCP access to any GraphQL API.
