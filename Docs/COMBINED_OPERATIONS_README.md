# Combined Operations for GraphQL MCP Server

This document explains how to use the combined operations tools that have been added to the GraphQL MCP Server. These tools allow you to perform multiple GraphQL operations in a single call, reducing round trips and improving performance.

## Overview

The combined operations implementation includes:

1. **CombinedOperationsTools** - Main tools for combined GraphQL operations
2. **CombinedOperationsService** - Service layer with caching and state management
3. **CombinedOperationsDemo** - Demonstration and example tools

## Key Benefits

- **Reduced Round Trips**: Combine multiple operations into single tool calls
- **Intelligent Caching**: Schema information is cached to avoid repeated introspection
- **Performance Monitoring**: Built-in metrics and performance tracking
- **Error Handling**: Comprehensive error handling with continue-on-error options
- **Result Chaining**: Sequential operations can use results from previous operations

## Main Tools

### 1. GraphqlServiceManager

The primary combined operations tool that can perform multiple actions in a single call.

```json
{
  "tool": "graphql_service_manager",
  "arguments": {
    "endpoint": "github-api",
    "action": "get_all_info",
    "includeMutations": false,
    "maxDepth": 3
  }
}
```

**Available Actions:**
- `get_all_info` (default): Returns comprehensive service information
- `get_schema`: Returns detailed schema information
- `list_queries`: Returns available operations with examples
- `execute_query`: Executes a GraphQL query
- `get_capabilities`: Returns service capabilities and metadata

### 2. ExecuteMultipleOperations

Execute multiple GraphQL operations in sequence or parallel.

```json
{
  "tool": "execute_multiple_operations",
  "arguments": {
    "operations": "[{\"endpoint\": \"github-api\", \"query\": \"query { viewer { login } }\", \"name\": \"get_user\"}, {\"endpoint\": \"github-api\", \"query\": \"query { repositories(first: 5) { nodes { name } } }\", \"name\": \"get_repos\"}]",
    "executionMode": "parallel",
    "continueOnError": true,
    "timeoutSeconds": 30
  }
}
```

**Execution Modes:**
- `sequential`: Execute operations one after another (allows result chaining)
- `parallel`: Execute all operations simultaneously (better performance)

### 3. CompareAndAnalyzeSchemas

Compare schemas between different endpoints and analyze differences.

```json
{
  "tool": "compare_and_analyze_schemas",
  "arguments": {
    "primaryEndpoint": "api-v1",
    "secondaryEndpoint": "api-v2",
    "includeFieldAnalysis": true,
    "detectBreakingChanges": true,
    "includeComplexityMetrics": true
  }
}
```

### 4. ExecuteAdvancedWorkflow

Execute sophisticated workflows that combine operations intelligently.

```json
{
  "tool": "execute_advanced_workflow",
  "arguments": {
    "workflowType": "data_aggregation",
    "endpoints": "[\"user-service\", \"profile-service\", \"activity-service\"]",
    "workflowConfig": "{\"primaryEntity\": \"user\", \"correlationField\": \"userId\"}",
    "primaryEntity": "user",
    "workflowTimeoutSeconds": 120
  }
}
```

**Workflow Types:**
- `data_aggregation`: Collect related data from multiple endpoints
- `dependency_chain`: Execute operations in dependency order
- `parallel_collection`: Execute independent operations in parallel
- `schema_migration`: Help migrate operations between endpoints

## Demo Tools

### RunCombinedOperationsDemo

Demonstrates practical use cases with realistic scenarios.

```json
{
  "tool": "run_combined_operations_demo",
  "arguments": {
    "scenario": "ecommerce_order"
  }
}
```

**Available Scenarios:**
- `ecommerce_order`: Multi-service order processing workflow
- `user_profile_aggregation`: User data aggregation from microservices
- `api_migration_check`: API compatibility analysis
- `health_monitoring`: Distributed service health monitoring

### GenerateTestQueries

Generate example queries and configurations for testing.

```json
{
  "tool": "generate_test_queries",
  "arguments": {
    "domain": "ecommerce",
    "queryCount": 5,
    "includeMutations": true,
    "complexity": "medium"
  }
}
```

## Usage Examples

### Example 1: Complete Service Information

Get everything about a GraphQL endpoint in one call:

```json
{
  "tool": "graphql_service_manager",
  "arguments": {
    "endpoint": "my-api",
    "action": "get_all_info"
  }
}
```

This returns:
- Endpoint information and status
- Complete schema with types and operations
- Available queries and mutations
- Example queries
- Service capabilities
- Performance metrics
- Operation history

### Example 2: Batch Query Execution

Execute multiple queries in parallel:

```json
{
  "tool": "execute_multiple_operations",
  "arguments": {
    "operations": "[{\"endpoint\": \"user-api\", \"query\": \"query { users { id name } }\", \"name\": \"get_users\"}, {\"endpoint\": \"order-api\", \"query\": \"query { orders { id status } }\", \"name\": \"get_orders\"}]",
    "executionMode": "parallel"
  }
}
```

### Example 3: Sequential Operations with Result Chaining

Execute operations in sequence, using results from previous operations:

```json
{
  "tool": "execute_multiple_operations",
  "arguments": {
    "operations": "[{\"endpoint\": \"user-api\", \"query\": \"query { user(id: \\\"123\\\") { id name } }\", \"name\": \"get_user\"}, {\"endpoint\": \"order-api\", \"query\": \"query { orders(userId: \\\"{{result.0.data.user.id}}\\\") { id status } }\", \"name\": \"get_user_orders\"}]",
    "executionMode": "sequential"
  }
}
```

### Example 4: E-commerce Order Workflow

Complex workflow for processing an e-commerce order:

```json
{
  "tool": "execute_advanced_workflow",
  "arguments": {
    "workflowType": "dependency_chain",
    "endpoints": "[\"customer-service\", \"inventory-service\", \"payment-service\", \"order-service\"]",
    "workflowConfig": "{\"orderData\": {\"customerId\": \"123\", \"items\": [{\"productId\": \"456\", \"quantity\": 2}]}, \"continueOnError\": false}"
  }
}
```

## Performance Features

### Caching

The combined operations service includes intelligent caching:

- **Schema Caching**: Schemas are cached for 30 minutes to avoid repeated introspection
- **Operation History**: Recent operations are tracked for analysis
- **Performance Metrics**: Execution times and success rates are recorded

### Monitoring

Built-in performance monitoring provides:

- Response time tracking
- Success/failure rates
- Operation frequency analysis
- Service health indicators

### Error Handling

Comprehensive error handling includes:

- Individual operation error isolation
- Continue-on-error options
- Detailed error reporting
- Automatic retry logic (where appropriate)

## Best Practices

### 1. Use Combined Operations for Related Data

Instead of making multiple separate calls:

```
// ❌ Multiple separate calls
1. Get user profile
2. Get user posts
3. Get user followers
4. Get user activity
```

Use data aggregation workflow:

```json
{
  "tool": "execute_advanced_workflow",
  "arguments": {
    "workflowType": "data_aggregation",
    "endpoints": "[\"user-service\", \"content-service\", \"social-service\", \"activity-service\"]",
    "primaryEntity": "user"
  }
}
```

### 2. Use Parallel Execution for Independent Operations

When operations don't depend on each other, use parallel execution:

```json
{
  "executionMode": "parallel"
}
```

### 3. Use Sequential Execution for Dependent Operations

When later operations need results from earlier ones:

```json
{
  "executionMode": "sequential"
}
```

### 4. Monitor Performance

Regularly check performance metrics:

```json
{
  "tool": "graphql_service_manager",
  "arguments": {
    "endpoint": "my-api",
    "action": "get_capabilities"
  }
}
```

## Integration with Existing Tools

The combined operations work seamlessly with existing MCP tools:

1. **Register endpoints** using `RegisterEndpoint`
2. **Use combined operations** for efficient data access
3. **Monitor with existing tools** for comprehensive observability

## Troubleshooting

### Common Issues

1. **Endpoint Not Found**: Make sure to register endpoints first using `RegisterEndpoint`
2. **Schema Introspection Fails**: Check endpoint URL and authentication headers
3. **Operations Timeout**: Increase timeout settings for complex workflows
4. **Result Chaining Issues**: Verify JSON path syntax in sequential operations

### Performance Optimization

1. **Use Caching**: Schema information is automatically cached
2. **Batch Related Operations**: Group related operations in single calls
3. **Choose Appropriate Execution Mode**: Parallel for independent, sequential for dependent operations
4. **Monitor Metrics**: Use performance metrics to identify bottlenecks

## Future Enhancements

Planned improvements include:

- Advanced result correlation algorithms
- More sophisticated caching strategies
- Enhanced workflow templates
- Better error recovery mechanisms
- Integration with GraphQL subscriptions
- Advanced schema evolution tracking

## Example Scenarios

See the demo tools for complete examples of:

- E-commerce order processing
- User profile aggregation
- API migration planning
- Distributed service monitoring

Use `run_combined_operations_demo` with different scenarios to see these in action.
