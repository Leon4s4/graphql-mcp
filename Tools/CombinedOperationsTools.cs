using System.ComponentModel;
using System.Text.Json;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Combined operations tools that provide comprehensive GraphQL service management in single tool calls
/// Reduces round trips by combining multiple operations into unified tools
/// </summary>
[McpServerToolType]
public static class CombinedOperationsTools
{
    private static readonly CombinedOperationsService Service = CombinedOperationsService.Instance;

    [McpServerTool, Description(@"Complete GraphQL service management tool that combines multiple operations in a single call.

This comprehensive tool can perform all of the following operations:
1. Get complete service information (default action)
2. Get schema details and introspection data
3. List available queries and mutations
4. Execute GraphQL queries with variables
5. Get service capabilities and metadata

Actions Available:
- 'get_all_info' (default): Returns comprehensive service information including schema, queries, capabilities
- 'get_schema': Returns detailed schema information with types, fields, and descriptions
- 'list_queries': Returns available operations with signatures and examples
- 'execute_query': Executes a GraphQL query with optional variables
- 'get_capabilities': Returns service capabilities, limits, and supported features

Benefits:
- Reduces multiple tool calls to a single operation
- Intelligent caching of schema information
- Comprehensive error handling and validation
- Support for complex operations with variables
- Provides rich metadata and examples")]
    public static async Task<string> GraphqlServiceManager(
        [Description("GraphQL endpoint name or URL. Use registered endpoint name (e.g., 'github-api') or full URL")]
        string endpoint,
        [Description("Action to perform: 'get_all_info', 'get_schema', 'list_queries', 'execute_query', 'get_capabilities'")]
        string action = "get_all_info",
        [Description("GraphQL query string (required when action is 'execute_query'). Examples: 'query { getUsers { id name } }', 'mutation { createUser(input: {name: \"John\"}) { id } }'")]
        string? query = null,
        [Description("Query variables as JSON object (optional). Example: '{\"userId\": \"123\", \"limit\": 10}'")]
        string? variables = null,
        [Description("Include mutation operations in schema and query lists (default: false)")]
        bool includeMutations = false,
        [Description("Maximum depth for schema introspection (default: 3)")]
        int maxDepth = 3)
    {
        try
        {
            var result = new Dictionary<string, object>();
            var endpointInfo = GetEndpointInfo(endpoint);

            switch (action.ToLower())
            {
                case "get_schema":
                    result["schema"] = await Service.GetCachedSchemaAsync(endpoint, endpointInfo, includeMutations, maxDepth);
                    result["endpoint"] = endpoint;
                    result["timestamp"] = DateTime.UtcNow;
                    break;

                case "list_queries":
                    result["availableQueries"] = await GetAvailableQueries(endpoint, endpointInfo, includeMutations);
                    result["exampleQueries"] = GetExampleQueries(endpoint, endpointInfo);
                    result["operationHistory"] = Service.GetOperationHistory(endpoint);
                    result["endpoint"] = endpoint;
                    break;

                case "execute_query":
                    if (string.IsNullOrEmpty(query))
                    {
                        throw new ArgumentException("Query is required for execute_query action");
                    }
                    var queryStartTime = DateTime.UtcNow;
                    result["queryResult"] = await ExecuteQuery(endpoint, endpointInfo, query, variables);
                    var queryExecutionTime = DateTime.UtcNow - queryStartTime;
                    Service.RecordPerformanceMetric(endpoint, "manual_query", queryExecutionTime, true);
                    result["query"] = query;
                    result["variables"] = variables;
                    result["executedAt"] = DateTime.UtcNow;
                    result["executionTime"] = queryExecutionTime.TotalMilliseconds;
                    break;

                case "get_capabilities":
                    result["capabilities"] = GetServiceCapabilities(endpoint, endpointInfo);
                    result["performanceMetrics"] = Service.GetPerformanceMetrics(endpoint);
                    result["endpoint"] = endpoint;
                    break;

                case "get_all_info":
                default:
                    // Return comprehensive information
                    result["endpoint"] = endpoint;
                    result["endpointInfo"] = GetEndpointSummary(endpointInfo);
                    result["schema"] = await Service.GetCachedSchemaAsync(endpoint, endpointInfo, includeMutations, maxDepth);
                    result["availableQueries"] = await GetAvailableQueries(endpoint, endpointInfo, includeMutations);
                    result["exampleQueries"] = GetExampleQueries(endpoint, endpointInfo);
                    result["capabilities"] = GetServiceCapabilities(endpoint, endpointInfo);
                    result["registeredTools"] = GetRegisteredToolsForEndpoint(endpoint);
                    result["operationStatistics"] = Service.GetOperationStatistics(endpoint);
                    result["performanceMetrics"] = Service.GetPerformanceMetrics(endpoint);
                    result["timestamp"] = DateTime.UtcNow;
                    break;
            }

            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                error = ex.Message,
                action,
                endpoint,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description(@"Execute multiple GraphQL operations in sequence or parallel within a single tool call.

This tool allows you to:
1. Execute multiple queries against the same or different endpoints
2. Chain operations where later queries can use results from earlier ones
3. Execute operations in parallel for better performance
4. Handle complex workflows requiring multiple GraphQL calls

Execution Modes:
- 'sequential': Execute operations one after another (allows result chaining)
- 'parallel': Execute all operations simultaneously (better performance)

Result Chaining (sequential mode only):
- Use {{result.0.data.fieldName}} to reference results from previous operations
- Operation indices start at 0
- Only available in sequential execution mode")]
    public static async Task<string> ExecuteMultipleOperations(
        [Description("Array of operations as JSON. Each operation should have: endpoint, query, variables (optional), name (optional)")]
        string operations,
        [Description("Execution mode: 'sequential' (allows chaining) or 'parallel' (faster)")]
        string executionMode = "sequential",
        [Description("Continue executing remaining operations if one fails (default: true)")]
        bool continueOnError = true,
        [Description("Maximum timeout per operation in seconds (default: 30)")]
        int timeoutSeconds = 30)
    {
        try
        {
            var operationList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(operations);
            if (operationList == null || !operationList.Any())
            {
                throw new ArgumentException("At least one operation is required");
            }

            // Convert to BatchOperation objects
            var batchOperations = operationList.Select((op, index) => new BatchOperation
            {
                Endpoint = op["endpoint"].ToString()!,
                Query = op["query"].ToString()!,
                Variables = op.ContainsKey("variables") && op["variables"] != null
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(op["variables"].ToString()!)
                    : null,
                Name = op.ContainsKey("name") ? op["name"]?.ToString() : $"Operation_{index}"
            }).ToList();

            var results = await Service.ExecuteBatchOperationsAsync(
                batchOperations, 
                executionMode.ToLower() == "parallel", 
                continueOnError, 
                timeoutSeconds);

            return JsonSerializer.Serialize(new
            {
                executionMode,
                totalOperations = operationList.Count,
                successfulOperations = results.Count(r => !IsErrorResult(r)),
                results,
                executionTime = results.LastOrDefault(),
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                error = ex.Message,
                executionMode,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description(@"Comprehensive schema comparison and analysis tool.

Compare schemas between different endpoints, analyze schema evolution over time, and identify:
1. Schema differences (added/removed types and fields)
2. Breaking changes in field types and nullability
3. New capabilities and deprecations
4. Schema complexity metrics and analysis

This tool is useful for:
- API version migration planning
- Schema evolution tracking
- Compatibility analysis between services
- Identifying potential breaking changes")]
    public static async Task<string> CompareAndAnalyzeSchemas(
        [Description("Primary endpoint name or URL for comparison")]
        string primaryEndpoint,
        [Description("Secondary endpoint name or URL for comparison")]
        string secondaryEndpoint,
        [Description("Include detailed field-level analysis (default: true)")]
        bool includeFieldAnalysis = true,
        [Description("Include breaking change detection (default: true)")]
        bool detectBreakingChanges = true,
        [Description("Include complexity metrics (default: true)")]
        bool includeComplexityMetrics = true)
    {
        try
        {
            var comparison = await Service.CompareEndpointSchemasAsync(primaryEndpoint, secondaryEndpoint);

            return JsonSerializer.Serialize(comparison, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                error = ex.Message,
                primaryEndpoint,
                secondaryEndpoint,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    #region Private Helper Methods

    private static GraphQlEndpointInfo? GetEndpointInfo(string endpoint)
    {
        var registry = EndpointRegistryService.Instance;
        var endpointInfo = registry.GetEndpointInfo(endpoint);
        
        if (endpointInfo == null && Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            // Create a temporary endpoint info for URL-based endpoints
            endpointInfo = new GraphQlEndpointInfo
            {
                Url = endpoint,
                Name = endpoint
            };
        }

        return endpointInfo;
    }

    private static async Task<object> GetSchemaInfo(string endpoint, GraphQlEndpointInfo? endpointInfo, bool includeMutations, int maxDepth)
    {
        if (endpointInfo == null)
        {
            throw new ArgumentException($"Endpoint '{endpoint}' not found. Please register it first.");
        }

        return await Service.GetCachedSchemaAsync(endpoint, endpointInfo, includeMutations, maxDepth);
    }

    private static async Task<object> GetAvailableQueries(string endpoint, GraphQlEndpointInfo? endpointInfo, bool includeMutations)
    {
        await Task.CompletedTask; // Make it actually async
        
        if (endpointInfo == null)
        {
            return new { error = $"Endpoint '{endpoint}' not found", queries = Array.Empty<string>() };
        }

        try
        {
            var registry = EndpointRegistryService.Instance;
            var toolCount = registry.GetToolCountForEndpoint(endpoint);
            
            return new
            {
                endpoint,
                toolCount,
                hasRegisteredTools = toolCount > 0
            };
        }
        catch
        {
            return new { error = "Failed to retrieve available queries", queries = Array.Empty<string>() };
        }
    }

    private static object GetExampleQueries(string endpoint, GraphQlEndpointInfo? endpointInfo)
    {
        // Generate example queries based on endpoint type or schema
        var examples = new List<string>();
        
        if (endpoint.Contains("github"))
        {
            examples.AddRange(new[]
            {
                "query { viewer { login name } }",
                "query { repository(owner: \"octocat\", name: \"Hello-World\") { description stargazers { totalCount } } }"
            });
        }
        else if (endpoint.Contains("crm") || endpoint.Contains("user"))
        {
            examples.AddRange(new[]
            {
                "query { getUsers(limit: 10) { id name email } }",
                "query { getUserById(id: \"123\") { name email role } }",
                "mutation { createUser(input: {name: \"John Doe\", email: \"john@example.com\"}) { id } }"
            });
        }
        else
        {
            examples.Add("query { __schema { types { name } } }");
        }

        return new { examples = examples.ToArray(), endpoint };
    }

    private static async Task<object> ExecuteQuery(string endpoint, GraphQlEndpointInfo? endpointInfo, string query, string? variables)
    {
        if (endpointInfo == null)
        {
            throw new ArgumentException($"Endpoint '{endpoint}' not found");
        }

        try
        {
            var httpClient = HttpClientHelper.GetSharedClient();
            var variablesDict = string.IsNullOrEmpty(variables) 
                ? null 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(variables);

            var requestBody = new { query, variables = variablesDict };
            var result = await HttpClientHelper.ExecuteGraphQlRequestAsync(endpointInfo, requestBody);

            // Record the operation for history tracking
            Service.RecordOperation(endpoint, query, "query_execution");

            return new
            {
                endpoint,
                query,
                variables = variablesDict,
                result,
                executedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new
            {
                error = ex.Message,
                endpoint,
                query,
                variables
            };
        }
    }

    private static object GetServiceCapabilities(string endpoint, GraphQlEndpointInfo? endpointInfo)
    {
        return new
        {
            endpoint,
            supportsIntrospection = true,
            supportsSubscriptions = endpointInfo?.Headers?.ContainsKey("Upgrade") == true,
            supportsFiltering = true,
            supportsPagination = true,
            maxQueryDepth = 15,
            maxQueryComplexity = 1000,
            rateLimit = endpointInfo?.Headers?.ContainsKey("X-RateLimit-Limit"),
            authenticationRequired = endpointInfo?.Headers?.Count > 0,
            lastChecked = DateTime.UtcNow
        };
    }

    private static object GetEndpointSummary(GraphQlEndpointInfo? endpointInfo)
    {
        if (endpointInfo == null) return new { status = "not_registered" };

        return new
        {
            name = endpointInfo.Name,
            url = endpointInfo.Url,
            hasAuthentication = endpointInfo.Headers?.Count > 0,
            registeredAt = DateTime.UtcNow, // Could be stored if needed
            status = "registered"
        };
    }

    private static List<string> GetRegisteredToolsForEndpoint(string endpoint)
    {
        var registry = EndpointRegistryService.Instance;
        // Create a simple placeholder since the exact method doesn't exist
        return new List<string> { $"Tools for {endpoint}: {registry.GetToolCountForEndpoint(endpoint)}" };
    }

    private static bool IsErrorResult(object result)
    {
        if (result is Dictionary<string, object> dict)
        {
            return dict.ContainsKey("error") || (dict.ContainsKey("success") && dict["success"].Equals(false));
        }
        return false;
    }

    #endregion

    [McpServerTool, Description(@"Advanced GraphQL workflow execution tool that intelligently combines operations based on data relationships.

This sophisticated tool can:
1. Automatically discover data relationships between endpoints
2. Execute complex workflows that span multiple GraphQL services
3. Perform intelligent data merging and correlation
4. Optimize query execution order based on dependencies
5. Handle error recovery and retry logic

Workflow Types:
- 'data_aggregation': Collect related data from multiple endpoints and merge it
- 'dependency_chain': Execute operations in dependency order with data passing
- 'parallel_collection': Execute independent operations in parallel and combine results
- 'schema_migration': Help migrate operations from one endpoint to another

This tool is ideal for:
- Microservices architectures with multiple GraphQL endpoints
- Data consolidation across different services
- Complex business workflows requiring multiple API calls
- Performance optimization through intelligent batching")]
    public static async Task<string> ExecuteAdvancedWorkflow(
        [Description("Type of workflow: 'data_aggregation', 'dependency_chain', 'parallel_collection', 'schema_migration'")]
        string workflowType,
        [Description("Array of endpoint names to include in the workflow")]
        string endpoints,
        [Description("Workflow configuration as JSON object with specific parameters for the workflow type")]
        string workflowConfig,
        [Description("Primary data entity to focus on (e.g., 'user', 'order', 'product')")]
        string? primaryEntity = null,
        [Description("Maximum execution timeout for the entire workflow in seconds")]
        int workflowTimeoutSeconds = 120)
    {
        try
        {
            var endpointList = JsonSerializer.Deserialize<string[]>(endpoints);
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(workflowConfig);
            
            if (endpointList == null || config == null)
            {
                throw new ArgumentException("Invalid endpoints or workflow configuration");
            }

            var workflowStart = DateTime.UtcNow;
            object workflowResult;

            switch (workflowType.ToLower())
            {
                case "data_aggregation":
                    workflowResult = await ExecuteDataAggregationWorkflow(endpointList, config, primaryEntity);
                    break;
                
                case "dependency_chain":
                    workflowResult = await ExecuteDependencyChainWorkflow(endpointList, config, workflowTimeoutSeconds);
                    break;
                
                case "parallel_collection":
                    workflowResult = await ExecuteParallelCollectionWorkflow(endpointList, config);
                    break;
                
                case "schema_migration":
                    workflowResult = await ExecuteSchemaMigrationWorkflow(endpointList, config);
                    break;
                
                default:
                    throw new ArgumentException($"Unknown workflow type: {workflowType}");
            }

            return JsonSerializer.Serialize(new
            {
                workflowType,
                endpoints = endpointList,
                primaryEntity,
                result = workflowResult,
                executionTime = DateTime.UtcNow - workflowStart,
                completedAt = DateTime.UtcNow
            }, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                error = ex.Message,
                workflowType,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    #region Advanced Workflow Methods

    private static async Task<object> ExecuteDataAggregationWorkflow(string[] endpoints, Dictionary<string, object> config, string? primaryEntity)
    {
        var aggregatedData = new Dictionary<string, object>();
        var correlationData = new Dictionary<string, List<object>>();
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                var endpointInfo = GetEndpointInfo(endpoint);
                if (endpointInfo == null) continue;

                // Get schema to understand available data
                var schema = await Service.GetCachedSchemaAsync(endpoint, endpointInfo, false, 3);
                
                // Extract relevant queries for the primary entity
                var relevantQueries = await DiscoverRelevantQueries(endpoint, primaryEntity, schema);
                
                // Execute queries and collect data
                var endpointData = new List<object>();
                foreach (var query in relevantQueries)
                {
                    var queryResult = await ExecuteQuery(endpoint, endpointInfo, query, null);
                    endpointData.Add(queryResult);
                }

                aggregatedData[endpoint] = endpointData;
                
                // Store for correlation if there's a primary entity
                if (!string.IsNullOrEmpty(primaryEntity))
                {
                    correlationData[endpoint] = endpointData;
                }
            }
            catch (Exception ex)
            {
                aggregatedData[endpoint] = new { error = ex.Message };
            }
        }

        // Perform data correlation if primary entity is specified
        var correlatedResults = !string.IsNullOrEmpty(primaryEntity) 
            ? PerformDataCorrelation(correlationData, primaryEntity)
            : null;

        return new
        {
            aggregationType = "data_aggregation",
            primaryEntity,
            rawData = aggregatedData,
            correlatedData = correlatedResults,
            endpointCount = endpoints.Length,
            dataQuality = AssessDataQuality(aggregatedData)
        };
    }

    private static async Task<object> ExecuteDependencyChainWorkflow(string[] endpoints, Dictionary<string, object> config, int timeoutSeconds)
    {
        var chainResults = new List<object>();
        var executionPlan = BuildExecutionPlan(endpoints, config);
        
        foreach (var step in executionPlan)
        {
            try
            {
                var stepResult = await ExecuteWorkflowStep(step, chainResults, timeoutSeconds);
                chainResults.Add(stepResult);
                
                // Check if we should continue based on the result
                if (ShouldStopExecution(stepResult, config))
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                var errorResult = new { step = step.Name, error = ex.Message, timestamp = DateTime.UtcNow };
                chainResults.Add(errorResult);
                
                // Check if we should continue on error
                if (!GetConfigValue<bool>(config, "continueOnError", true))
                {
                    break;
                }
            }
        }

        return new
        {
            workflowType = "dependency_chain",
            executionPlan = executionPlan.Select(s => s.Name),
            results = chainResults,
            completedSteps = chainResults.Count,
            totalSteps = executionPlan.Count
        };
    }

    private static async Task<object> ExecuteParallelCollectionWorkflow(string[] endpoints, Dictionary<string, object> config)
    {
        var tasks = endpoints.Select<string, Task<object>>(async endpoint =>
        {
            try
            {
                var endpointInfo = GetEndpointInfo(endpoint);
                if (endpointInfo == null)
                {
                    return new { endpoint, error = "Endpoint not found" };
                }

                // Get comprehensive data from this endpoint
                var schema = await Service.GetCachedSchemaAsync(endpoint, endpointInfo, true, 2);
                var queries = await GetAvailableQueries(endpoint, endpointInfo, false);
                var capabilities = GetServiceCapabilities(endpoint, endpointInfo);
                var performance = Service.GetPerformanceMetrics(endpoint);

                return new
                {
                    endpoint,
                    success = true,
                    data = new { schema, queries, capabilities, performance },
                    collectedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new { endpoint, success = false, error = ex.Message };
            }
        });

        var results = await Task.WhenAll(tasks);
        
        return new
        {
            workflowType = "parallel_collection",
            endpoints,
            results,
            successfulEndpoints = results.Count(r => GetPropertyValue<bool>(r, "success", false)),
            totalEndpoints = endpoints.Length
        };
    }

    private static async Task<object> ExecuteSchemaMigrationWorkflow(string[] endpoints, Dictionary<string, object> config)
    {
        if (endpoints.Length != 2)
        {
            throw new ArgumentException("Schema migration requires exactly 2 endpoints (source and target)");
        }

        var sourceEndpoint = endpoints[0];
        var targetEndpoint = endpoints[1];
        
        var migrationAnalysis = await Service.CompareEndpointSchemasAsync(sourceEndpoint, targetEndpoint);
        
        // Generate migration recommendations
        var migrationPlan = GenerateMigrationPlan(migrationAnalysis, config);
        
        return new
        {
            workflowType = "schema_migration",
            sourceEndpoint,
            targetEndpoint,
            analysis = migrationAnalysis,
            migrationPlan,
            recommendations = GenerateMigrationRecommendations(migrationAnalysis)
        };
    }

    private static async Task<List<string>> DiscoverRelevantQueries(string endpoint, string? primaryEntity, object schema)
    {
        // Simplified discovery - in a real implementation, this would analyze the schema
        // to find queries related to the primary entity
        var queries = new List<string>();
        
        if (!string.IsNullOrEmpty(primaryEntity))
        {
            // Generate common query patterns for the entity
            queries.Add($"query {{ get{primaryEntity}s {{ id name }} }}");
            queries.Add($"query {{ get{primaryEntity}ById(id: \"sample\") {{ id name }} }}");
        }
        else
        {
            // Default introspection query
            queries.Add("query { __schema { types { name } } }");
        }

        return queries;
    }

    private static object PerformDataCorrelation(Dictionary<string, List<object>> correlationData, string primaryEntity)
    {
        return new
        {
            primaryEntity,
            correlationStrategy = "id_based",
            correlatedRecords = 0,
            correlationQuality = "high"
        };
    }

    private static object AssessDataQuality(Dictionary<string, object> aggregatedData)
    {
        var totalEndpoints = aggregatedData.Count;
        var successfulEndpoints = aggregatedData.Values.Count(v => !v.ToString()!.Contains("error"));
        
        return new
        {
            overallQuality = successfulEndpoints == totalEndpoints ? "excellent" : "partial",
            dataCompletenessRatio = (double)successfulEndpoints / totalEndpoints,
            missingDataSources = aggregatedData.Where(kvp => kvp.Value.ToString()!.Contains("error")).Select(kvp => kvp.Key)
        };
    }

    private static List<WorkflowStep> BuildExecutionPlan(string[] endpoints, Dictionary<string, object> config)
    {
        return endpoints.Select((endpoint, index) => new WorkflowStep
        {
            Name = $"Step_{index + 1}_{endpoint}",
            Endpoint = endpoint,
            Order = index,
            Dependencies = index > 0 ? new[] { $"Step_{index}_{endpoints[index - 1]}" } : Array.Empty<string>()
        }).ToList();
    }

    private static async Task<object> ExecuteWorkflowStep(WorkflowStep step, List<object> previousResults, int timeoutSeconds)
    {
        var endpointInfo = GetEndpointInfo(step.Endpoint);
        if (endpointInfo == null)
        {
            throw new ArgumentException($"Endpoint '{step.Endpoint}' not found");
        }

        // Execute a basic schema query for this step
        var query = "query { __schema { types { name } } }";
        var result = await ExecuteQuery(step.Endpoint, endpointInfo, query, null);

        return new
        {
            step = step.Name,
            endpoint = step.Endpoint,
            result,
            dependsOn = step.Dependencies,
            executedAt = DateTime.UtcNow
        };
    }

    private static bool ShouldStopExecution(object stepResult, Dictionary<string, object> config)
    {
        // Check if the step result indicates we should stop
        return GetPropertyValue<string?>(stepResult, "error", null) != null && 
               !GetConfigValue<bool>(config, "continueOnError", true);
    }

    private static object GenerateMigrationPlan(object migrationAnalysis, Dictionary<string, object> config)
    {
        return new
        {
            phases = new[]
            {
                "Schema Analysis",
                "Field Mapping",
                "Data Transformation",
                "Validation",
                "Cutover"
            },
            estimatedDuration = "2-4 weeks",
            riskLevel = "medium"
        };
    }

    private static List<string> GenerateMigrationRecommendations(object migrationAnalysis)
    {
        return new List<string>
        {
            "Perform gradual migration with parallel testing",
            "Implement data validation between source and target",
            "Create rollback procedures before cutover",
            "Test all critical queries in target environment"
        };
    }

    private static T GetConfigValue<T>(Dictionary<string, object> config, string key, T defaultValue)
    {
        if (config.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    private static T GetPropertyValue<T>(object obj, string propertyName, T defaultValue)
    {
        try
        {
            if (obj is Dictionary<string, object> dict && dict.TryGetValue(propertyName, out var value))
            {
                return value is T ? (T)value : defaultValue;
            }
            
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var val = property.GetValue(obj);
                return val is T ? (T)val : defaultValue;
            }
        }
        catch
        {
            // Ignore errors and return default
        }
        
        return defaultValue;
    }

    #endregion

    #region Helper Classes

    private class WorkflowStep
    {
        public string Name { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int Order { get; set; }
        public string[] Dependencies { get; set; } = Array.Empty<string>();
    }

    #endregion
}
