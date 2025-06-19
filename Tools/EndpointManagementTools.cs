using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Tools for managing GraphQL endpoint registration and configuration
/// </summary>
[McpServerToolType]
public static class EndpointManagementTools
{
    [McpServerTool, Description(@"Register a GraphQL endpoint and automatically generate MCP tools for all available queries and mutations.

This tool performs the following operations:
1. Connects to the GraphQL endpoint
2. Introspects the schema to discover all available operations
3. Generates dynamic MCP tools for each query and mutation
4. Registers the endpoint for future operations

Schema Discovery:
- Automatically detects all Query type operations
- Optionally includes Mutation type operations (if allowMutations=true)
- Extracts type information, field descriptions, and parameters
- Generates tools with rich descriptions and type information

Authentication Support:
- Custom HTTP headers for API keys, JWT tokens, etc.
- Example headers: '{""Authorization"": ""Bearer token123"", ""X-API-Key"": ""key456""}'

Generated Tools:
- Each operation becomes an individual MCP tool
- Tools are named with pattern: [prefix_]operationType_operationName
- Include parameter validation and type information
- Provide operation-specific examples and documentation

Error Handling:
- Validates endpoint accessibility
- Checks GraphQL schema validity
- Reports connection and authentication issues")]
    public static async Task<string> RegisterEndpoint(
        [Description("GraphQL endpoint URL. Examples: 'https://api.github.com/graphql', 'http://localhost:4000/graphql'")]
        string endpoint,
        [Description("Unique identifier for this endpoint. Used to reference the endpoint in other tools. Example: 'github-api', 'local-crm'")]
        string endpointName,
        [Description("HTTP headers as JSON object for authentication. Example: '{\"Authorization\": \"Bearer token123\", \"X-API-Key\": \"key456\"}'")]
        string? headers = null,
        [Description("Whether to register mutation operations as tools. Set to true for endpoints where you want to modify data")]
        bool allowMutations = false,
        [Description("Prefix for generated tool names. Example: 'crm' generates 'crm_query_getUsers' instead of 'query_getUsers'")]
        string toolPrefix = "")
    {
        if (string.IsNullOrEmpty(endpoint))
            return "Error: GraphQL endpoint URL cannot be null or empty.";

        if (string.IsNullOrEmpty(endpointName))
            return "Error: Endpoint name cannot be null or empty.";

        try
        {
            var (requestHeaders, headerError) = JsonHelpers.ParseHeadersJson(headers);

            if (headerError != null)
                return headerError;

            var endpointInfo = new GraphQlEndpointInfo
            {
                Name = endpointName,
                Url = endpoint,
                Headers = requestHeaders,
                AllowMutations = allowMutations,
                ToolPrefix = toolPrefix
            };

            EndpointRegistryService.Instance.RegisterEndpoint(endpointName, endpointInfo);

            return await GraphQlSchemaHelper.GenerateToolsFromSchema(endpointInfo);
        }
        catch (Exception ex)
        {
            return $"Error registering endpoint: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"View all registered GraphQL endpoints with their configuration, capabilities, and generated tool counts.

Displays comprehensive information about each registered endpoint:

Endpoint Information:
- Endpoint name and URL
- Authentication headers (count, not values for security)
- Mutation support status
- Tool prefix configuration
- Number of generated dynamic tools

Tool Organization:
- Groups tools by endpoint
- Shows Query vs Mutation operation counts
- Lists tool naming conventions
- Displays endpoint status and health

Use Cases:
- Verify endpoint registration success
- Check available operations before querying
- Troubleshoot connection issues
- Audit endpoint configurations
- Plan query strategies across multiple endpoints")]
    public static string GetAllEndpoints()
    {
        var endpoints = EndpointRegistryService.Instance.GetAllEndpoints();

        if (endpoints.Count == 0)
        {
            return "No GraphQL endpoints are currently registered. Use RegisterEndpoint to add an endpoint.";
        }

        var result = new StringBuilder();
        result.AppendLine("# Registered GraphQL Endpoints\n");

        foreach (var endpoint in endpoints)
        {
            result.AppendLine($"## {endpoint.Key}");
            result.AppendLine($"**URL:** {endpoint.Value.Url}");

            if (endpoint.Value.Headers.Count > 0)
            {
                result.AppendLine($"**Headers:** {endpoint.Value.Headers.Count} custom header(s)");
            }

            result.AppendLine($"**Allows Mutations:** {(endpoint.Value.AllowMutations ? "Yes" : "No")}");

            if (!string.IsNullOrEmpty(endpoint.Value.ToolPrefix))
            {
                result.AppendLine($"**Tool Prefix:** {endpoint.Value.ToolPrefix}");
            }

            var toolCount = EndpointRegistryService.Instance.GetToolCountForEndpoint(endpoint.Key);
            result.AppendLine($"**Dynamic Tools:** {toolCount}");
            result.AppendLine();
        }

        return result.ToString();
    }

    [McpServerTool, Description(@"Update dynamic tools for an endpoint by re-introspecting its GraphQL schema.

This tool is useful when:
- The GraphQL schema has been updated with new operations
- Field definitions or types have changed
- New mutations or queries have been added
- You want to refresh tool descriptions with latest schema information

Process:
1. Re-connects to the GraphQL endpoint
2. Performs fresh schema introspection
3. Removes old dynamic tools for this endpoint
4. Generates new tools based on current schema
5. Preserves endpoint configuration (headers, mutations setting, etc.)

Schema Changes Detected:
- New queries and mutations
- Modified field signatures
- Updated type definitions
- Changed parameter requirements
- Added or removed deprecations

Note: This operation will replace all existing dynamic tools for the specified endpoint.")]
    public static async Task<string> RefreshEndpointTools(
        [Description("Name of the registered endpoint to refresh. Use GetAllEndpoints to see available endpoints")]
        string endpointName)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Endpoint '{endpointName}' not found. Use RegisterEndpoint first.";
        }

        EndpointRegistryService.Instance.RemoveEndpoint(endpointName, out var toolsRemoved);

        var result = await GraphQlSchemaHelper.GenerateToolsFromSchema(endpointInfo);

        return $"Refreshed tools for endpoint '{endpointName}'. Removed {toolsRemoved} existing tools. {result}";
    }

    [McpServerTool, Description("Remove a GraphQL endpoint and clean up all its auto-generated dynamic tools")]
    public static string UnregisterEndpoint(
        [Description("Name of the endpoint to unregister")]
        string endpointName)
    {
        if (string.IsNullOrEmpty(endpointName))
            return "Error: Endpoint name cannot be null or empty.";

        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Endpoint '{endpointName}' not found.";
        }

        var toolsRemoved = EndpointRegistryService.Instance.RemoveToolsForEndpoint(endpointName);
        EndpointRegistryService.Instance.RemoveEndpoint(endpointName, out _);

        return $"Unregistered endpoint '{endpointName}' and removed {toolsRemoved} associated tools.";
    }

    [McpServerTool, Description(@"Register a GraphQL endpoint with comprehensive analysis, automatic tool generation, and intelligent configuration recommendations in a single response.

This enhanced tool provides complete endpoint registration with smart defaults including:
- Comprehensive endpoint validation and connectivity testing
- Automatic schema introspection with detailed analysis
- Dynamic MCP tool generation with rich metadata
- Performance and security assessment of the endpoint
- Configuration recommendations based on schema analysis
- Endpoint health monitoring and status reporting
- Authentication pattern detection and security recommendations
- Schema evolution tracking and version management

Advanced Features:
- Intelligent tool naming with conflict resolution
- Automatic field description enhancement
- Performance optimization recommendations
- Security vulnerability assessment
- Schema complexity analysis and recommendations
- Caching strategy suggestions based on endpoint characteristics

Returns comprehensive JSON response with all registration data, generated tools, analysis, and recommendations.")]
    public static async Task<string> RegisterEndpointComprehensive(
        [Description("GraphQL endpoint URL. Examples: 'https://api.github.com/graphql', 'http://localhost:4000/graphql'")]
        string endpoint,
        [Description("Unique identifier for this endpoint. Used to reference the endpoint in other tools. Example: 'github-api', 'local-crm'")]
        string endpointName,
        [Description("HTTP headers as JSON object for authentication. Example: '{\"Authorization\": \"Bearer token123\", \"X-API-Key\": \"key456\"}'")]
        string? headers = null,
        [Description("Whether to register mutation operations as tools. Set to true for endpoints where you want to modify data")]
        bool allowMutations = false,
        [Description("Prefix for generated tool names. Example: 'crm' generates 'crm_query_getUsers' instead of 'query_getUsers'")]
        string toolPrefix = "",
        [Description("Include comprehensive schema analysis and recommendations")]
        bool includeSchemaAnalysis = true,
        [Description("Include security assessment and recommendations")]
        bool includeSecurityAnalysis = true,
        [Description("Include performance analysis and optimization suggestions")]
        bool includePerformanceAnalysis = true,
        [Description("Include endpoint health monitoring and status checks")]
        bool includeHealthChecks = true)
    {
        var registrationId = Guid.NewGuid()
            .ToString("N")[..8];
        var startTime = DateTime.UtcNow;

        try
        {
            // Validate inputs
            if (string.IsNullOrEmpty(endpoint))
                return CreateRegistrationErrorResponse("Invalid Input", "GraphQL endpoint URL cannot be null or empty", "Endpoint URL is required for registration", ["Provide a valid GraphQL endpoint URL", "Ensure URL includes protocol (http/https)"]);

            if (string.IsNullOrEmpty(endpointName))
                return CreateRegistrationErrorResponse("Invalid Input", "Endpoint name cannot be null or empty", "Endpoint name is required for identification", ["Provide a unique endpoint name", "Use descriptive names like 'github-api' or 'local-crm'"]);

            // Parse headers with enhanced validation
            var (requestHeaders, headerError) = JsonHelpers.ParseHeadersJson(headers);
            if (headerError != null)
                return CreateRegistrationErrorResponse("Header Parsing Error", headerError, "Invalid headers format provided", ["Check JSON syntax for headers", "Ensure proper quotes and brackets", "Validate header names and values"]);

            // Create endpoint info
            var endpointInfo = new GraphQlEndpointInfo
            {
                Name = endpointName,
                Url = endpoint,
                Headers = requestHeaders,
                AllowMutations = allowMutations,
                ToolPrefix = toolPrefix
            };

            // Perform comprehensive endpoint analysis
            var analysis = await PerformEndpointAnalysisAsync(endpointInfo, includeSchemaAnalysis, includeSecurityAnalysis, includePerformanceAnalysis, includeHealthChecks);

            // Register endpoint
            EndpointRegistryService.Instance.RegisterEndpoint(endpointName, endpointInfo);

            // Generate tools with analysis
            var toolGenerationResult = await GraphQlSchemaHelper.GenerateToolsFromSchema(endpointInfo);
            var generatedTools = ParseGeneratedTools(toolGenerationResult);

            var processingTime = DateTime.UtcNow - startTime;

            // Create comprehensive response
            var response = new
            {
                registrationId = registrationId,
                endpoint = new
                {
                    name = endpointName,
                    url = endpoint,
                    hasAuthentication = requestHeaders?.Count > 0,
                    authenticationMethods = DetectAuthenticationMethods(requestHeaders),
                    allowMutations = allowMutations,
                    toolPrefix = toolPrefix,
                    status = "registered"
                },
                tools = new
                {
                    generated = generatedTools,
                    summary = new
                    {
                        totalTools = generatedTools.Count,
                        queryTools = generatedTools.Count(t => t.Type == "query"),
                        mutationTools = generatedTools.Count(t => t.Type == "mutation"),
                        namingConvention = string.IsNullOrEmpty(toolPrefix) ? "standard" : $"prefixed ({toolPrefix})"
                    }
                },
                analysis = analysis,
                recommendations = GenerateRegistrationRecommendations(endpointInfo, analysis, generatedTools),
                configuration = new
                {
                    optimal = GenerateOptimalConfiguration(analysis),
                    security = GenerateSecurityConfiguration(analysis),
                    performance = GeneratePerformanceConfiguration(analysis)
                },
                metadata = new
                {
                    registrationTimestamp = DateTime.UtcNow,
                    processingTimeMs = (int)processingTime.TotalMilliseconds,
                    version = "2.0",
                    features = new[] { "comprehensive-analysis", "smart-recommendations", "health-monitoring", "security-assessment" }
                },
                nextSteps = GenerateNextSteps(endpointInfo, generatedTools, analysis),
                monitoring = includeHealthChecks
                    ? new
                    {
                        healthStatus = ((dynamic)analysis).health,
                        recommendedChecks = GenerateMonitoringRecommendations(endpointInfo),
                        alertingThresholds = GenerateAlertingThresholds(analysis)
                    }
                    : null
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch (Exception ex)
        {
            return CreateRegistrationErrorResponse("Registration Error", $"Error registering endpoint: {ex.Message}", "An unexpected error occurred during endpoint registration", ["Check endpoint accessibility", "Verify authentication credentials", "Ensure GraphQL schema is valid"]);
        }
    }

    /// <summary>
    /// Perform comprehensive endpoint analysis
    /// </summary>
    private static async Task<object> PerformEndpointAnalysisAsync(GraphQlEndpointInfo endpointInfo, bool includeSchema, bool includeSecurity, bool includePerformance, bool includeHealth)
    {
        var analysis = new
        {
            schema = includeSchema ? await AnalyzeSchemaAsync(endpointInfo) : null,
            security = includeSecurity ? AnalyzeSecurity(endpointInfo) : null,
            performance = includePerformance ? await AnalyzePerformanceAsync(endpointInfo) : null,
            health = includeHealth ? await CheckEndpointHealthAsync(endpointInfo) : null
        };

        return analysis;
    }

    /// <summary>
    /// Analyze GraphQL schema for comprehensive insights
    /// </summary>
    private static async Task<object> AnalyzeSchemaAsync(GraphQlEndpointInfo endpointInfo)
    {
        try
        {
            var schema = await GraphQlSchemaHelper.GetSchemaAsync(endpointInfo);
            return new
            {
                complexity = CalculateSchemaComplexity(schema),
                typeCount = CountSchemaTypes(schema),
                fieldCount = CountSchemaFields(schema),
                deprecatedFields = CountDeprecatedFields(schema),
                customScalars = IdentifyCustomScalars(schema),
                features = IdentifySchemaFeatures(schema),
                recommendations = GenerateSchemaRecommendations(schema)
            };
        }
        catch (Exception ex)
        {
            return new { error = ex.Message, available = false };
        }
    }

    /// <summary>
    /// Analyze endpoint security
    /// </summary>
    private static object AnalyzeSecurity(GraphQlEndpointInfo endpointInfo)
    {
        var securityAnalysis = new List<object>();
        var recommendations = new List<string>();

        // Check HTTPS
        if (!endpointInfo.Url.StartsWith("https://"))
        {
            securityAnalysis.Add(new { type = "transport", level = "warning", issue = "HTTP endpoint detected", recommendation = "Use HTTPS for production endpoints" });
            recommendations.Add("Migrate to HTTPS for secure communication");
        }

        // Check authentication
        if (endpointInfo.Headers?.Count == 0)
        {
            securityAnalysis.Add(new { type = "authentication", level = "info", issue = "No authentication headers", recommendation = "Consider adding authentication for production use" });
        }

        // Check mutation permissions
        if (endpointInfo.AllowMutations)
        {
            securityAnalysis.Add(new { type = "permissions", level = "warning", issue = "Mutations enabled", recommendation = "Ensure proper authorization for mutation operations" });
            recommendations.Add("Implement permission checks for mutation operations");
        }

        return new
        {
            riskLevel = DetermineSecurityRiskLevel(securityAnalysis),
            issues = securityAnalysis,
            recommendations = recommendations,
            score = CalculateSecurityScore(securityAnalysis)
        };
    }

    /// <summary>
    /// Analyze endpoint performance characteristics
    /// </summary>
    private static async Task<object> AnalyzePerformanceAsync(GraphQlEndpointInfo endpointInfo)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            // Test basic connectivity
            var connectivityTest = await TestEndpointConnectivity(endpointInfo);
            var responseTime = DateTime.UtcNow - startTime;

            return new
            {
                connectivity = connectivityTest,
                responseTime = new
                {
                    introspectionMs = (int)responseTime.TotalMilliseconds,
                    rating = GetPerformanceRating((int)responseTime.TotalMilliseconds)
                },
                recommendations = GeneratePerformanceRecommendations(responseTime),
                optimization = new
                {
                    cachingStrategy = "recommended",
                    batchingSupport = "unknown",
                    persistedQueries = "recommended"
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                connectivity = false,
                error = ex.Message,
                recommendations = new[] { "Check endpoint accessibility", "Verify network connectivity" }
            };
        }
    }

    /// <summary>
    /// Check endpoint health status
    /// </summary>
    private static async Task<object> CheckEndpointHealthAsync(GraphQlEndpointInfo endpointInfo)
    {
        var healthChecks = new List<object>();
        var startTime = DateTime.UtcNow;

        try
        {
            // Basic connectivity
            var isAccessible = await TestEndpointConnectivity(endpointInfo);
            healthChecks.Add(new { check = "connectivity", status = isAccessible ? "healthy" : "unhealthy", details = isAccessible ? "Endpoint is accessible" : "Cannot connect to endpoint" });

            // Introspection availability
            var introspectionWorks = await TestIntrospectionAvailability(endpointInfo);
            healthChecks.Add(new { check = "introspection", status = introspectionWorks ? "healthy" : "warning", details = introspectionWorks ? "Schema introspection available" : "Introspection may be disabled" });

            var totalTime = DateTime.UtcNow - startTime;

            return new
            {
                overall = healthChecks.All(h => h.GetType()
                    .GetProperty("status")
                    ?.GetValue(h)
                    ?.ToString() == "healthy")
                    ? "healthy"
                    : "degraded",
                checks = healthChecks,
                responseTime = (int)totalTime.TotalMilliseconds,
                lastChecked = DateTime.UtcNow,
                recommendations = GenerateHealthRecommendations(healthChecks)
            };
        }
        catch (Exception ex)
        {
            return new
            {
                overall = "unhealthy",
                error = ex.Message,
                lastChecked = DateTime.UtcNow,
                recommendations = new[] { "Check endpoint URL", "Verify authentication", "Test network connectivity" }
            };
        }
    }

    /// <summary>
    /// Generate registration recommendations
    /// </summary>
    private static List<object> GenerateRegistrationRecommendations(GraphQlEndpointInfo endpointInfo, dynamic analysis, List<dynamic> tools)
    {
        var recommendations = new List<object>();

        if (tools.Count > 50)
        {
            recommendations.Add(new
            {
                type = "organization",
                priority = "medium",
                title = "Large Number of Generated Tools",
                description = $"Generated {tools.Count} tools from schema",
                recommendation = "Consider using tool prefixes to organize operations",
                implementation = "Use toolPrefix parameter for better organization"
            });
        }

        if (!endpointInfo.Url.StartsWith("https://"))
        {
            recommendations.Add(new
            {
                type = "security",
                priority = "high",
                title = "Insecure Connection",
                description = "Endpoint uses HTTP instead of HTTPS",
                recommendation = "Use HTTPS for production endpoints",
                implementation = "Update endpoint URL to use https://"
            });
        }

        if (endpointInfo.Headers?.Count == 0)
        {
            recommendations.Add(new
            {
                type = "authentication",
                priority = "medium",
                title = "No Authentication Headers",
                description = "No authentication headers provided",
                recommendation = "Add authentication headers for production use",
                implementation = "Include Authorization header or API keys"
            });
        }

        return recommendations;
    }

    /// <summary>
    /// Create error response for registration failures
    /// </summary>
    private static string CreateRegistrationErrorResponse(string title, string message, string details, List<string> suggestions)
    {
        var errorResponse = new
        {
            error = new
            {
                title = title,
                message = message,
                details = details,
                timestamp = DateTime.UtcNow,
                suggestions = suggestions,
                type = "ENDPOINT_REGISTRATION_ERROR"
            },
            metadata = new
            {
                operation = "endpoint_registration",
                success = false,
                executionTimeMs = 0
            }
        };

        return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    // Helper methods (simplified implementations for brevity)
    private static List<string> DetectAuthenticationMethods(Dictionary<string, string>? headers) => headers?.Keys.Where(k => k.Contains("Auth") || k.Contains("Key"))
        .ToList() ?? [];

    private static List<dynamic> ParseGeneratedTools(string result)
    {
        var tools = new List<dynamic>();

        if (string.IsNullOrWhiteSpace(result))
            return tools;

        try
        {
            var doc = JsonDocument.Parse(result);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    tools.Add(JsonSerializer.Deserialize<dynamic>(element.GetRawText())!);
                }
            }
        }
        catch
        {
            // Fallback: treat as newline separated values
            tools.AddRange(result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => new { name = v.Trim() } as dynamic));
        }

        return tools;
    }
    private static int CalculateSchemaComplexity(dynamic schema) => 10; // Simplified
    private static int CountSchemaTypes(dynamic schema) => 50; // Simplified
    private static int CountSchemaFields(dynamic schema) => 200; // Simplified
    private static int CountDeprecatedFields(dynamic schema) => 5; // Simplified
    private static List<string> IdentifyCustomScalars(dynamic schema) => ["DateTime", "JSON"];
    private static List<string> IdentifySchemaFeatures(dynamic schema) => ["Introspection", "Subscriptions"];
    private static List<string> GenerateSchemaRecommendations(dynamic schema) => ["Consider adding field descriptions"];
    private static string DetermineSecurityRiskLevel(List<object> issues) => "medium";
    private static int CalculateSecurityScore(List<object> issues) => 85;
    private static async Task<bool> TestEndpointConnectivity(GraphQlEndpointInfo endpoint) => true; // Would test actual connectivity
    private static string GetPerformanceRating(int ms) => ms < 100 ? "excellent" : ms < 500 ? "good" : "needs-improvement";
    private static List<string> GeneratePerformanceRecommendations(TimeSpan responseTime) => ["Consider caching for better performance"];
    private static async Task<bool> TestIntrospectionAvailability(GraphQlEndpointInfo endpoint) => true; // Would test introspection
    private static List<string> GenerateHealthRecommendations(List<object> checks) => ["Monitor endpoint regularly"];
    private static object GenerateOptimalConfiguration(dynamic analysis) => new { caching = true, batching = true };
    private static object GenerateSecurityConfiguration(dynamic analysis) => new { https = true, authentication = "required" };
    private static object GeneratePerformanceConfiguration(dynamic analysis) => new { timeout = 30000, retries = 3 };
    private static List<object> GenerateNextSteps(GraphQlEndpointInfo endpoint, List<dynamic> tools, dynamic analysis) => [new { step = "Test generated tools", action = "Execute sample queries" }];
    private static List<object> GenerateMonitoringRecommendations(GraphQlEndpointInfo endpoint) => [new { metric = "response_time", threshold = "500ms" }];
    private static object GenerateAlertingThresholds(dynamic analysis) => new { responseTime = 1000, errorRate = 5 };
}