# GraphQL MCP Server - Connection Error Handling Demo

## Overview

The GraphQL MCP Server now has **centralized HTTP client error handling** that provides explicit and actionable error
messages when connection issues occur, preventing code continuation after errors and ensuring `EnsureSuccessStatusCode`
is called on every request.

## 🎯 Key Improvements

### 1. **Explicit Connection Error Handling**

- **Network connectivity issues** are clearly identified
- **DNS resolution failures** show specific error messages
- **Endpoint unreachability** provides troubleshooting steps
- **Connection refused errors** no longer show generic messages

### 2. **Prevents Code Continuation After Errors**

- **Early error detection** stops execution immediately on connection failure
- **No partial processing** when the endpoint is unreachable
- **Clear error categorization** (Connection, HTTP, GraphQL, Unexpected)

### 3. **Centralized HTTP Execution**

- **Single point of HTTP calling** eliminates code duplication
- **Consistent error handling** across all GraphQL tools
- **Automatic EnsureSuccessStatusCode** on every request
- **Standardized response formatting**

## 🔧 Technical Implementation

### New Centralized Method

```csharp
// Single method handles all GraphQL HTTP requests
public static async Task<GraphQLResponse> ExecuteGraphQLRequestAsync(
    string endpoint, 
    object requestBody, 
    string? headers = null, 
    TimeSpan? timeout = null)
```

### Error Categories

1. **Connection Errors** - Network, DNS, endpoint unreachable
2. **HTTP Errors** - 4xx, 5xx status codes with troubleshooting
3. **GraphQL Errors** - Query errors from the GraphQL service
4. **Unexpected Errors** - Unforeseen issues with guidance

## 📋 Before vs After Examples

### Before (Generic Error)

```
GraphQL request failed: HttpRequestException
```

### After (Explicit Connection Error)

```
# GraphQL Connection Error

❌ **Status:** Connection Failed

## Error Details
**Message:** Cannot connect to GraphQL endpoint 'http://localhost:4000/graphql': No connection could be made because the target machine actively refused it.

## Troubleshooting Steps
1. **Check the endpoint URL** - Ensure it's correct and accessible
2. **Verify network connectivity** - Can you reach the server?
3. **Check firewall settings** - Are there any blocked ports?
4. **Validate DNS resolution** - Does the hostname resolve correctly?
5. **Review authentication** - Are the required headers/tokens provided?
```

### HTTP Error Example

```
# GraphQL HTTP Error

❌ **Status:** HTTP 401 Unauthorized

## Error Details
```json
{
  "error": "Invalid authentication token"
}
```

## Troubleshooting Steps

- **Check authentication** - Verify API keys, tokens, or credentials

```

## 🚀 Updated Tools

The following tools now use the centralized error handling:

- **QueryGraphQLMcpTool** - Main query execution tool
- **SchemaIntrospectionTools** - Schema discovery with connection validation
- **QueryValidationTools** - Query testing with proper error handling
- **PerformanceMonitoringTools** - Performance testing with connection checks
- **GraphQLSchemaTools** - Schema comparison with error handling

## 🧪 Testing Connection Errors

### Test 1: Invalid Endpoint
```json
{
  "query": "{ __typename }",
  "endpointName": "test",
  "variables": null
}
```

**Error Response:**

```
# GraphQL Connection Error
❌ **Status:** Connection Failed
**Message:** Cannot connect to GraphQL endpoint 'http://invalid-endpoint:4000/graphql': No such host is known.
```

### Test 2: Connection Refused

```json
{
  "query": "{ __typename }",
  "endpointName": "local_down",
  "variables": null
}
```

**Error Response:**

```
# GraphQL Connection Error
❌ **Status:** Connection Failed
**Message:** Cannot connect to GraphQL endpoint 'http://localhost:9999/graphql': Connection refused.
```

### Test 3: Timeout

```json
{
  "query": "{ __typename }",
  "endpointName": "slow_endpoint",
  "variables": null
}
```

**Error Response:**

```
# GraphQL Connection Error
❌ **Status:** Connection Failed
**Message:** Request to GraphQL endpoint 'http://slow-endpoint/graphql' timed out
```

## 💡 Benefits

### For Developers

- **Clear error diagnosis** - Know exactly what went wrong
- **Actionable troubleshooting** - Step-by-step resolution guidance
- **Faster debugging** - No more generic "request failed" messages
- **Consistent experience** - Same error handling across all tools

### For Production

- **Better monitoring** - Connection issues are clearly categorized
- **Improved reliability** - No partial processing on connection failures
- **Enhanced security** - Proper HTTP status code validation
- **Centralized maintenance** - Single point for HTTP logic updates

## 🔗 Related Files

- `Tools/HttpClientHelper.cs` - Centralized HTTP execution and error handling
- `Tools/QueryGraphQLMcpTool.cs` - Updated to use centralized method
- `Tools/SchemaIntrospectionTools.cs` - Schema discovery with connection validation
- `Tools/QueryValidationTools.cs` - Query testing with proper error handling
- `Tools/PerformanceMonitoringTools.cs` - Performance testing with error handling

The GraphQL MCP Server now provides professional-grade connection error handling that helps developers quickly identify
and resolve connectivity issues.
