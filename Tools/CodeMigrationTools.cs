using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Tools for migrating from REST API code to GraphQL queries
/// Analyzes C# code to extract REST calls and generate equivalent GraphQL queries
/// </summary>
[McpServerToolType]
public static class CodeMigrationTools
{
    [McpServerTool, Description(@"Analyze C# code that makes REST API calls and generate equivalent GraphQL queries.

This tool helps migrate from REST to GraphQL by:
1. Extracting REST API calls from C# code
2. Analyzing data aggregation patterns
3. Identifying entity relationships and dependencies
4. Generating equivalent GraphQL queries that combine multiple REST calls
5. Providing migration recommendations and optimizations

Supported C# Patterns:
- HttpClient REST calls (GET, POST, PUT, DELETE)
- Multiple API calls with data aggregation
- Async/await patterns
- LINQ operations on API responses
- Entity mapping and transformation
- Conditional API calls based on previous responses

Output includes:
- Extracted REST endpoints and parameters
- Generated GraphQL query equivalents
- Data aggregation analysis
- Performance improvement estimates
- Migration step-by-step guide")]
    public static async Task<string> ExtractGraphQLFromCSharpCode(
        [Description("C# code containing REST API calls to analyze")]
        string csharpCode,
        [Description("Target GraphQL endpoint name (must be registered)")]
        string graphqlEndpoint,
        [Description("Include detailed analysis of data flows and dependencies")]
        bool includeDataFlowAnalysis = true,
        [Description("Generate optimized GraphQL queries with fragments and variables")]
        bool includeOptimizations = true,
        [Description("Include migration recommendations and best practices")]
        bool includeMigrationGuide = true,
        [Description("Analysis mode: 'basic', 'detailed', 'comprehensive'")]
        string analysisMode = "detailed")
    {
        try
        {
            var result = new StringBuilder();
            result.AppendLine("# C# to GraphQL Migration Analysis\n");

            // Validate GraphQL endpoint
            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(graphqlEndpoint);
            if (endpointInfo == null)
            {
                return $"Error: GraphQL endpoint '{graphqlEndpoint}' not found. Please register it first using RegisterEndpoint.";
            }

            // Analyze C# code
            var codeAnalysis = AnalyzeCSharpCode(csharpCode, analysisMode);
            
            result.AppendLine("## Code Analysis Summary");
            result.AppendLine($"**Target GraphQL Endpoint:** {graphqlEndpoint}");
            result.AppendLine($"**Analysis Mode:** {analysisMode}");
            result.AppendLine($"**REST Calls Found:** {codeAnalysis.RestCalls.Count}");
            result.AppendLine($"**Entities Identified:** {codeAnalysis.Entities.Count}");
            result.AppendLine($"**Data Aggregations:** {codeAnalysis.DataAggregations.Count}");
            result.AppendLine();

            // Extract REST calls
            if (codeAnalysis.RestCalls.Any())
            {
                result.AppendLine("## Extracted REST API Calls\n");
                foreach (var restCall in codeAnalysis.RestCalls)
                {
                    result.AppendLine($"### {restCall.Method} {restCall.Endpoint}");
                    result.AppendLine($"**Purpose:** {restCall.Purpose}");
                    if (restCall.Parameters.Any())
                    {
                        result.AppendLine($"**Parameters:** {string.Join(", ", restCall.Parameters)}");
                    }
                    if (!string.IsNullOrEmpty(restCall.ResponseType))
                    {
                        result.AppendLine($"**Response Type:** {restCall.ResponseType}");
                    }
                    result.AppendLine($"**Code Location:** Line {restCall.LineNumber}");
                    result.AppendLine();
                }
            }

            // Generate GraphQL queries
            var graphqlQueries = await GenerateGraphQLQueries(codeAnalysis, endpointInfo, includeOptimizations);
            
            result.AppendLine("## Generated GraphQL Queries\n");
            foreach (var query in graphqlQueries)
            {
                result.AppendLine($"### {query.Name}");
                result.AppendLine($"**Replaces:** {string.Join(", ", query.ReplacesRestCalls)}");
                result.AppendLine($"**Performance Improvement:** {query.PerformanceImprovement}");
                result.AppendLine();
                result.AppendLine("```graphql");
                result.AppendLine(query.Query);
                result.AppendLine("```");
                
                if (query.Variables.Any())
                {
                    result.AppendLine("\n**Variables:**");
                    result.AppendLine("```json");
                    result.AppendLine(JsonSerializer.Serialize(query.Variables, new JsonSerializerOptions { WriteIndented = true }));
                    result.AppendLine("```");
                }
                result.AppendLine();
            }

            // Data flow analysis
            if (includeDataFlowAnalysis && codeAnalysis.DataFlows.Any())
            {
                result.AppendLine("## Data Flow Analysis\n");
                foreach (var dataFlow in codeAnalysis.DataFlows)
                {
                    result.AppendLine($"### {dataFlow.Name}");
                    result.AppendLine($"**Flow:** {dataFlow.Description}");
                    result.AppendLine($"**Dependencies:** {string.Join(" → ", dataFlow.Dependencies)}");
                    result.AppendLine($"**GraphQL Optimization:** {dataFlow.GraphQLOptimization}");
                    result.AppendLine();
                }
            }

            // Performance analysis
            result.AppendLine("## Performance Analysis\n");
            var performanceAnalysis = AnalyzePerformanceImprovements(codeAnalysis, graphqlQueries);
            result.AppendLine($"**Current REST Calls:** {performanceAnalysis.CurrentCallCount}");
            result.AppendLine($"**GraphQL Queries:** {performanceAnalysis.GraphQLQueryCount}");
            result.AppendLine($"**Network Reduction:** {performanceAnalysis.NetworkReduction}");
            result.AppendLine($"**Estimated Performance Gain:** {performanceAnalysis.PerformanceGain}");
            result.AppendLine($"**Data Over-fetching Reduction:** {performanceAnalysis.OverFetchingReduction}");
            result.AppendLine();

            // Migration guide
            if (includeMigrationGuide)
            {
                result.AppendLine("## Migration Guide\n");
                var migrationSteps = GenerateMigrationSteps(codeAnalysis, graphqlQueries);
                for (int i = 0; i < migrationSteps.Count; i++)
                {
                    result.AppendLine($"{i + 1}. {migrationSteps[i]}");
                }
                result.AppendLine();

                result.AppendLine("## Migration Code Example\n");
                var migrationCode = GenerateMigrationCode(codeAnalysis, graphqlQueries.First());
                result.AppendLine("```csharp");
                result.AppendLine(migrationCode);
                result.AppendLine("```");
                result.AppendLine();
            }

            // Recommendations
            result.AppendLine("## Recommendations\n");
            var recommendations = GenerateRecommendations(codeAnalysis, graphqlQueries);
            foreach (var recommendation in recommendations)
            {
                result.AppendLine($"- {recommendation}");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error analyzing C# code: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Generate optimized GraphQL queries to replace multiple REST API calls.

This tool analyzes REST API usage patterns and creates equivalent GraphQL queries that:
- Combine multiple REST calls into single GraphQL operations
- Optimize data fetching with precise field selection
- Implement proper pagination and filtering
- Use fragments for reusable field sets
- Include variables for dynamic queries

Input can be REST endpoint patterns, entity relationships, or existing API documentation.")]
    public static async Task<string> GenerateOptimizedGraphQLQueries(
        [Description("JSON array of REST endpoints to replace. Format: [{\"method\": \"GET\", \"endpoint\": \"/users/{id}\", \"purpose\": \"Get user details\"}]")]
        string restEndpoints,
        [Description("Target GraphQL endpoint name")]
        string graphqlEndpoint,
        [Description("Entity relationships as JSON. Format: {\"User\": [\"posts\", \"comments\"], \"Post\": [\"author\", \"tags\"]}")]
        string? entityRelationships = null,
        [Description("Include query optimization techniques (fragments, variables, aliases)")]
        bool includeOptimizations = true,
        [Description("Generate queries for different use cases (list, detail, search, etc.)")]
        bool includeVariations = true)
    {
        try
        {
            var result = new StringBuilder();
            result.AppendLine("# Optimized GraphQL Query Generation\n");

            // Parse REST endpoints
            var endpoints = JsonSerializer.Deserialize<List<RestEndpointInfo>>(restEndpoints);
            if (endpoints == null || !endpoints.Any())
            {
                return "Error: No valid REST endpoints provided.";
            }

            // Validate GraphQL endpoint
            var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(graphqlEndpoint);
            if (endpointInfo == null)
            {
                return $"Error: GraphQL endpoint '{graphqlEndpoint}' not found.";
            }

            // Parse entity relationships
            Dictionary<string, string[]>? relationships = null;
            if (!string.IsNullOrEmpty(entityRelationships))
            {
                relationships = JsonSerializer.Deserialize<Dictionary<string, string[]>>(entityRelationships);
            }

            result.AppendLine($"**Target GraphQL Endpoint:** {graphqlEndpoint}");
            result.AppendLine($"**REST Endpoints to Replace:** {endpoints.Count}");
            result.AppendLine($"**Entity Relationships:** {relationships?.Count ?? 0}");
            result.AppendLine();

            // Generate optimized queries
            var optimizedQueries = await GenerateOptimizedQueries(endpoints, relationships, endpointInfo, includeOptimizations);

            result.AppendLine("## Optimized GraphQL Queries\n");
            foreach (var query in optimizedQueries)
            {
                result.AppendLine($"### {query.Name}");
                result.AppendLine($"**Replaces:** {string.Join(", ", query.ReplacesRestCalls)}");
                result.AppendLine($"**Optimization Techniques:** {string.Join(", ", query.OptimizationTechniques)}");
                result.AppendLine();

                result.AppendLine("```graphql");
                result.AppendLine(query.Query);
                result.AppendLine("```");

                if (query.Variables.Any())
                {
                    result.AppendLine("\n**Variables:**");
                    result.AppendLine("```json");
                    result.AppendLine(JsonSerializer.Serialize(query.Variables, new JsonSerializerOptions { WriteIndented = true }));
                    result.AppendLine("```");
                }

                if (query.Fragments.Any())
                {
                    result.AppendLine("\n**Fragments:**");
                    foreach (var fragment in query.Fragments)
                    {
                        result.AppendLine("```graphql");
                        result.AppendLine(fragment);
                        result.AppendLine("```");
                    }
                }
                result.AppendLine();
            }

            // Query variations
            if (includeVariations)
            {
                result.AppendLine("## Query Variations\n");
                var variations = GenerateQueryVariations(optimizedQueries);
                foreach (var variation in variations)
                {
                    result.AppendLine($"### {variation.Type}");
                    result.AppendLine($"**Use Case:** {variation.UseCase}");
                    result.AppendLine("```graphql");
                    result.AppendLine(variation.Query);
                    result.AppendLine("```");
                    result.AppendLine();
                }
            }

            // Performance comparison
            result.AppendLine("## Performance Comparison\n");
            var comparison = CompareRestVsGraphQL(endpoints, optimizedQueries);
            result.AppendLine($"- **REST API Calls:** {comparison.RestCallCount}");
            result.AppendLine($"- **GraphQL Queries:** {comparison.GraphQLQueryCount}");
            result.AppendLine($"- **Network Round Trips Reduced:** {comparison.RoundTripReduction}%");
            result.AppendLine($"- **Data Over-fetching Reduced:** {comparison.OverFetchingReduction}%");
            result.AppendLine($"- **Query Complexity:** {comparison.QueryComplexity}");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error generating optimized queries: {ex.Message}";
        }
    }

    #region Data Models

    private class CodeAnalysis
    {
        public List<RestCall> RestCalls { get; set; } = new();
        public List<string> Entities { get; set; } = new();
        public List<DataAggregation> DataAggregations { get; set; } = new();
        public List<DataFlow> DataFlows { get; set; } = new();
    }

    private class RestCall
    {
        public string Method { get; set; } = "";
        public string Endpoint { get; set; } = "";
        public string Purpose { get; set; } = "";
        public List<string> Parameters { get; set; } = new();
        public string ResponseType { get; set; } = "";
        public int LineNumber { get; set; }
    }

    private class DataAggregation
    {
        public string Name { get; set; } = "";
        public List<string> Sources { get; set; } = new();
        public string TargetType { get; set; } = "";
    }

    private class DataFlow
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Dependencies { get; set; } = new();
        public string GraphQLOptimization { get; set; } = "";
    }

    private class GeneratedGraphQLQuery
    {
        public string Name { get; set; } = "";
        public string Query { get; set; } = "";
        public List<string> ReplacesRestCalls { get; set; } = new();
        public Dictionary<string, object> Variables { get; set; } = new();
        public List<string> Fragments { get; set; } = new();
        public string PerformanceImprovement { get; set; } = "";
        public List<string> OptimizationTechniques { get; set; } = new();
    }

    private class RestEndpointInfo
    {
        public string Method { get; set; } = "";
        public string Endpoint { get; set; } = "";
        public string Purpose { get; set; } = "";
        public List<string> Parameters { get; set; } = new();
        public string ResponseType { get; set; } = "";
    }

    private class QueryVariation
    {
        public string Type { get; set; } = "";
        public string UseCase { get; set; } = "";
        public string Query { get; set; } = "";
    }

    private class PerformanceAnalysis
    {
        public int CurrentCallCount { get; set; }
        public int GraphQLQueryCount { get; set; }
        public string NetworkReduction { get; set; } = "";
        public string PerformanceGain { get; set; } = "";
        public string OverFetchingReduction { get; set; } = "";
    }

    private class PerformanceComparison
    {
        public int RestCallCount { get; set; }
        public int GraphQLQueryCount { get; set; }
        public int RoundTripReduction { get; set; }
        public int OverFetchingReduction { get; set; }
        public string QueryComplexity { get; set; } = "";
    }

    #endregion

    #region Analysis Methods

    private static CodeAnalysis AnalyzeCSharpCode(string csharpCode, string analysisMode)
    {
        var analysis = new CodeAnalysis();

        // Extract REST API calls
        var httpClientPattern = @"(?:await\s+)?(?:httpClient|client|http)\.(?:GetAsync|PostAsync|PutAsync|DeleteAsync|SendAsync)\s*\(\s*[""']([^""']+)[""']";
        var httpMatches = Regex.Matches(csharpCode, httpClientPattern, RegexOptions.IgnoreCase);

        var lineNumber = 1;
        foreach (Match match in httpMatches)
        {
            var endpoint = match.Groups[1].Value;
            var method = DetermineHttpMethod(match.Value);
            
            analysis.RestCalls.Add(new RestCall
            {
                Method = method,
                Endpoint = endpoint,
                Purpose = DeterminePurpose(endpoint),
                Parameters = ExtractParameters(endpoint),
                ResponseType = GuessResponseType(endpoint),
                LineNumber = lineNumber++
            });
        }

        // Extract entities
        var entityPattern = @"class\s+(\w+)|public\s+(\w+)\s+\w+\s*\{|var\s+\w+\s*=\s*new\s+(\w+)";
        var entityMatches = Regex.Matches(csharpCode, entityPattern);
        
        foreach (Match match in entityMatches)
        {
            var entityName = match.Groups[1].Value ?? match.Groups[2].Value ?? match.Groups[3].Value;
            if (!string.IsNullOrEmpty(entityName) && !analysis.Entities.Contains(entityName))
            {
                analysis.Entities.Add(entityName);
            }
        }

        // Extract data aggregations
        var aggregationPattern = @"(\w+)\s*=.*?(?:Select|Where|Join|Aggregate)";
        var aggregationMatches = Regex.Matches(csharpCode, aggregationPattern);
        
        foreach (Match match in aggregationMatches)
        {
            analysis.DataAggregations.Add(new DataAggregation
            {
                Name = match.Groups[1].Value,
                Sources = ExtractDataSources(match.Value),
                TargetType = "Object"
            });
        }

        // Analyze data flows
        if (analysisMode == "detailed" || analysisMode == "comprehensive")
        {
            analysis.DataFlows = AnalyzeDataFlows(csharpCode, analysis.RestCalls);
        }

        return analysis;
    }

    private static async Task<List<GeneratedGraphQLQuery>> GenerateGraphQLQueries(
        CodeAnalysis codeAnalysis, 
        GraphQlEndpointInfo endpointInfo,
        bool includeOptimizations)
    {
        var queries = new List<GeneratedGraphQLQuery>();

        // Group related REST calls
        var groupedCalls = GroupRelatedRestCalls(codeAnalysis.RestCalls);

        foreach (var group in groupedCalls)
        {
            var queryName = GenerateQueryName(group);
            var graphqlQuery = await BuildGraphQLQuery(group, endpointInfo, includeOptimizations);
            
            queries.Add(new GeneratedGraphQLQuery
            {
                Name = queryName,
                Query = graphqlQuery,
                ReplacesRestCalls = group.Select(c => $"{c.Method} {c.Endpoint}").ToList(),
                Variables = GenerateVariables(group),
                PerformanceImprovement = CalculatePerformanceImprovement(group.Count),
                OptimizationTechniques = includeOptimizations ? 
                    new List<string> { "Field Selection", "Single Request", "Variable Usage" } : 
                    new List<string>()
            });
        }

        return queries;
    }

    private static async Task<List<GeneratedGraphQLQuery>> GenerateOptimizedQueries(
        List<RestEndpointInfo> endpoints,
        Dictionary<string, string[]>? relationships,
        GraphQlEndpointInfo endpointInfo,
        bool includeOptimizations)
    {
        var queries = new List<GeneratedGraphQLQuery>();

        // Group endpoints by entity type
        var groupedEndpoints = GroupEndpointsByEntity(endpoints);

        foreach (var group in groupedEndpoints)
        {
            var queryBuilder = new StringBuilder();
            var queryName = $"Get{group.Key}Data";
            
            queryBuilder.AppendLine($"query {queryName}($id: ID!, $limit: Int = 10, $offset: Int = 0) {{");
            
            // Main entity query
            queryBuilder.AppendLine($"  {group.Key.ToLower()}(id: $id) {{");
            queryBuilder.AppendLine("    id");
            queryBuilder.AppendLine("    name");
            
            // Add relationships if available
            if (relationships != null && relationships.ContainsKey(group.Key))
            {
                foreach (var relation in relationships[group.Key])
                {
                    queryBuilder.AppendLine($"    {relation}(limit: $limit, offset: $offset) {{");
                    queryBuilder.AppendLine("      id");
                    queryBuilder.AppendLine("      name");
                    queryBuilder.AppendLine("    }");
                }
            }
            
            queryBuilder.AppendLine("  }");
            queryBuilder.AppendLine("}");

            var optimizationTechniques = new List<string>();
            if (includeOptimizations)
            {
                optimizationTechniques.AddRange(new[] { "Variables", "Pagination", "Field Selection" });
            }

            queries.Add(new GeneratedGraphQLQuery
            {
                Name = queryName,
                Query = queryBuilder.ToString(),
                ReplacesRestCalls = group.Value.Select(e => $"{e.Method} {e.Endpoint}").ToList(),
                Variables = new Dictionary<string, object>
                {
                    ["id"] = "user-123",
                    ["limit"] = 10,
                    ["offset"] = 0
                },
                OptimizationTechniques = optimizationTechniques,
                PerformanceImprovement = CalculatePerformanceImprovement(group.Value.Count)
            });
        }

        return queries;
    }

    private static List<QueryVariation> GenerateQueryVariations(List<GeneratedGraphQLQuery> baseQueries)
    {
        var variations = new List<QueryVariation>();

        foreach (var baseQuery in baseQueries)
        {
            // List variation
            variations.Add(new QueryVariation
            {
                Type = "List Query",
                UseCase = "Fetch paginated list of items",
                Query = CreateListVariation(baseQuery)
            });

            // Search variation
            variations.Add(new QueryVariation
            {
                Type = "Search Query",
                UseCase = "Search with filters and criteria",
                Query = CreateSearchVariation(baseQuery)
            });

            // Minimal variation
            variations.Add(new QueryVariation
            {
                Type = "Minimal Query",
                UseCase = "Fetch only essential fields for performance",
                Query = CreateMinimalVariation(baseQuery)
            });
        }

        return variations;
    }

    private static PerformanceComparison CompareRestVsGraphQL(List<RestEndpointInfo> restEndpoints, List<GeneratedGraphQLQuery> graphqlQueries)
    {
        var restCallCount = restEndpoints.Count;
        var graphqlQueryCount = graphqlQueries.Count;
        var roundTripReduction = Math.Max(0, ((restCallCount - graphqlQueryCount) * 100) / restCallCount);
        
        return new PerformanceComparison
        {
            RestCallCount = restCallCount,
            GraphQLQueryCount = graphqlQueryCount,
            RoundTripReduction = roundTripReduction,
            OverFetchingReduction = 60, // Estimated
            QueryComplexity = graphqlQueryCount switch
            {
                1 => "Low",
                <= 3 => "Medium",
                _ => "High"
            }
        };
    }

    #endregion

    #region Helper Methods

    private static string DetermineHttpMethod(string matchValue)
    {
        return matchValue.ToLower() switch
        {
            var x when x.Contains("getasync") => "GET",
            var x when x.Contains("postasync") => "POST",
            var x when x.Contains("putasync") => "PUT",
            var x when x.Contains("deleteasync") => "DELETE",
            _ => "GET"
        };
    }

    private static string DeterminePurpose(string endpoint)
    {
        return endpoint.ToLower() switch
        {
            var x when x.Contains("user") => "User management",
            var x when x.Contains("product") => "Product data",
            var x when x.Contains("order") => "Order processing",
            var x when x.Contains("search") => "Search operations",
            _ => "Data retrieval"
        };
    }

    private static List<string> ExtractParameters(string endpoint)
    {
        var parameters = new List<string>();
        var paramPattern = @"\{(\w+)\}";
        var matches = Regex.Matches(endpoint, paramPattern);
        
        foreach (Match match in matches)
        {
            parameters.Add(match.Groups[1].Value);
        }
        
        return parameters;
    }

    private static string GuessResponseType(string endpoint)
    {
        return endpoint.ToLower() switch
        {
            var x when x.Contains("user") => "User",
            var x when x.Contains("product") => "Product",
            var x when x.Contains("order") => "Order",
            _ => "Object"
        };
    }

    private static List<string> ExtractDataSources(string aggregationCode)
    {
        var sources = new List<string>();
        var variablePattern = @"\b(\w+)\b";
        var matches = Regex.Matches(aggregationCode, variablePattern);
        
        foreach (Match match in matches)
        {
            var variable = match.Groups[1].Value;
            if (char.IsLower(variable[0]))
            {
                sources.Add(variable);
            }
        }
        
        return sources.Distinct().ToList();
    }

    private static List<DataFlow> AnalyzeDataFlows(string code, List<RestCall> restCalls)
    {
        var dataFlows = new List<DataFlow>();
        
        // Simple analysis - look for variable assignments and usage
        foreach (var restCall in restCalls)
        {
            dataFlows.Add(new DataFlow
            {
                Name = $"Flow for {restCall.Endpoint}",
                Description = $"Data from {restCall.Method} {restCall.Endpoint}",
                Dependencies = new List<string> { restCall.ResponseType },
                GraphQLOptimization = "Can be combined with related queries"
            });
        }
        
        return dataFlows;
    }

    private static List<List<RestCall>> GroupRelatedRestCalls(List<RestCall> restCalls)
    {
        var groups = new List<List<RestCall>>();
        var processed = new HashSet<RestCall>();
        
        foreach (var call in restCalls)
        {
            if (processed.Contains(call)) continue;
            
            var group = new List<RestCall> { call };
            processed.Add(call);
            
            // Find related calls (same entity type)
            foreach (var otherCall in restCalls)
            {
                if (processed.Contains(otherCall)) continue;
                
                if (AreRelatedCalls(call, otherCall))
                {
                    group.Add(otherCall);
                    processed.Add(otherCall);
                }
            }
            
            groups.Add(group);
        }
        
        return groups;
    }

    private static bool AreRelatedCalls(RestCall call1, RestCall call2)
    {
        // Simple heuristic: same entity type in endpoint
        var entity1 = ExtractEntityFromEndpoint(call1.Endpoint);
        var entity2 = ExtractEntityFromEndpoint(call2.Endpoint);
        
        return entity1.Equals(entity2, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractEntityFromEndpoint(string endpoint)
    {
        var parts = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.FirstOrDefault(p => !p.StartsWith("{") && !p.StartsWith("api") && !p.StartsWith("v")) ?? "unknown";
    }

    private static Dictionary<string, List<RestEndpointInfo>> GroupEndpointsByEntity(List<RestEndpointInfo> endpoints)
    {
        return endpoints
            .GroupBy(e => ExtractEntityFromEndpoint(e.Endpoint))
            .ToDictionary(g => char.ToUpper(g.Key[0]) + g.Key.Substring(1), g => g.ToList());
    }

    private static string GenerateQueryName(List<RestCall> restCalls)
    {
        var entities = restCalls.Select(c => ExtractEntityFromEndpoint(c.Endpoint)).Distinct();
        return $"Get{string.Join("And", entities.Select(e => char.ToUpper(e[0]) + e.Substring(1)))}Data";
    }

    private static async Task<string> BuildGraphQLQuery(List<RestCall> restCalls, GraphQlEndpointInfo endpointInfo, bool includeOptimizations)
    {
        var queryBuilder = new StringBuilder();
        var queryName = GenerateQueryName(restCalls);
        
        queryBuilder.AppendLine($"query {queryName}($id: ID!) {{");
        
        foreach (var call in restCalls)
        {
            var entityName = ExtractEntityFromEndpoint(call.Endpoint).ToLower();
            queryBuilder.AppendLine($"  {entityName}(id: $id) {{");
            queryBuilder.AppendLine("    id");
            queryBuilder.AppendLine("    name");
            
            // Add common fields based on entity type
            if (entityName.Contains("user"))
            {
                queryBuilder.AppendLine("    email");
                queryBuilder.AppendLine("    createdAt");
            }
            else if (entityName.Contains("product"))
            {
                queryBuilder.AppendLine("    price");
                queryBuilder.AppendLine("    description");
            }
            
            queryBuilder.AppendLine("  }");
        }
        
        queryBuilder.AppendLine("}");
        
        return queryBuilder.ToString();
    }

    private static Dictionary<string, object> GenerateVariables(List<RestCall> restCalls)
    {
        var variables = new Dictionary<string, object>();
        
        foreach (var call in restCalls)
        {
            foreach (var param in call.Parameters)
            {
                if (!variables.ContainsKey(param))
                {
                    variables[param] = param.ToLower() switch
                    {
                        "id" => "user-123",
                        "limit" => 10,
                        "offset" => 0,
                        _ => "value"
                    };
                }
            }
        }
        
        return variables;
    }

    private static string CalculatePerformanceImprovement(int restCallCount)
    {
        return restCallCount switch
        {
            1 => "Minimal improvement (single call)",
            <= 3 => $"Moderate improvement ({restCallCount} calls → 1 query)",
            <= 5 => $"Significant improvement ({restCallCount} calls → 1 query)",
            _ => $"Major improvement ({restCallCount} calls → 1 query)"
        };
    }

    private static PerformanceAnalysis AnalyzePerformanceImprovements(CodeAnalysis codeAnalysis, List<GeneratedGraphQLQuery> graphqlQueries)
    {
        var currentCalls = codeAnalysis.RestCalls.Count;
        var graphqlQueries_count = graphqlQueries.Count;
        var reduction = Math.Max(0, ((currentCalls - graphqlQueries_count) * 100) / currentCalls);
        
        return new PerformanceAnalysis
        {
            CurrentCallCount = currentCalls,
            GraphQLQueryCount = graphqlQueries_count,
            NetworkReduction = $"{reduction}%",
            PerformanceGain = reduction switch
            {
                >= 75 => "Excellent (75%+ reduction)",
                >= 50 => "Good (50%+ reduction)",
                >= 25 => "Moderate (25%+ reduction)",
                _ => "Minimal improvement"
            },
            OverFetchingReduction = "60-80% (estimated)"
        };
    }

    private static List<string> GenerateMigrationSteps(CodeAnalysis codeAnalysis, List<GeneratedGraphQLQuery> graphqlQueries)
    {
        return new List<string>
        {
            "Install GraphQL client library (e.g., GraphQL.Client, StrawberryShake)",
            "Create GraphQL client service with endpoint configuration",
            "Replace individual REST calls with GraphQL queries",
            "Update data models to match GraphQL response structure",
            "Add error handling for GraphQL-specific errors",
            "Test queries with different parameters and scenarios",
            "Optimize queries based on performance monitoring",
            "Remove unused REST API code and dependencies"
        };
    }

    private static string GenerateMigrationCode(CodeAnalysis codeAnalysis, GeneratedGraphQLQuery graphqlQuery)
    {
        return $@"// Before: Multiple REST calls
// var user = await httpClient.GetAsync(""/api/users/{{id}}"");
// var posts = await httpClient.GetAsync(""/api/users/{{id}}/posts"");

// After: Single GraphQL query
public async Task<UserWithPostsResponse> GetUserWithPostsAsync(string userId)
{{
    var query = @""
{graphqlQuery.Query.Trim()}
"";

    var variables = new {{ id = userId }};
    
    var request = new GraphQLRequest
    {{
        Query = query,
        Variables = variables
    }};
    
    var response = await graphqlClient.SendQueryAsync<UserWithPostsResponse>(request);
    
    if (response.Errors?.Any() == true)
    {{
        throw new GraphQLException(response.Errors);
    }}
    
    return response.Data;
}}";
    }

    private static List<string> GenerateRecommendations(CodeAnalysis codeAnalysis, List<GeneratedGraphQLQuery> graphqlQueries)
    {
        var recommendations = new List<string>
        {
            "Use GraphQL fragments for commonly requested field sets",
            "Implement query result caching to improve performance",
            "Add query complexity analysis to prevent expensive operations",
            "Use variables for all dynamic query parameters",
            "Implement proper error handling for GraphQL-specific errors"
        };

        if (graphqlQueries.Count > 3)
        {
            recommendations.Add("Consider query batching for multiple related operations");
        }

        if (codeAnalysis.RestCalls.Count > 10)
        {
            recommendations.Add("Implement incremental migration approach");
            recommendations.Add("Consider GraphQL subscriptions for real-time data");
        }

        return recommendations;
    }

    private static string CreateListVariation(GeneratedGraphQLQuery baseQuery)
    {
        return baseQuery.Query.Replace("(id: $id)", "(limit: $limit, offset: $offset)")
                              .Replace("$id: ID!", "$limit: Int = 10, $offset: Int = 0");
    }

    private static string CreateSearchVariation(GeneratedGraphQLQuery baseQuery)
    {
        return baseQuery.Query.Replace("(id: $id)", "(search: $searchTerm, filters: $filters)")
                              .Replace("$id: ID!", "$searchTerm: String!, $filters: FilterInput");
    }

    private static string CreateMinimalVariation(GeneratedGraphQLQuery baseQuery)
    {
        // Remove all fields except id and name
        var lines = baseQuery.Query.Split('\n');
        var minimalLines = lines.Where(line => 
            line.Contains("query ") || 
            line.Contains("{") || 
            line.Contains("}") ||
            line.Contains("id") ||
            line.Contains("name")).ToArray();
        
        return string.Join('\n', minimalLines);
    }

    #endregion
}