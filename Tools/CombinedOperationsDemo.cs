using System.ComponentModel;
using System.Text.Json;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Demonstration and example tools for combined operations
/// Shows practical usage patterns for the combined operations tools
/// </summary>
[McpServerToolType]
public static class CombinedOperationsDemo
{
    [McpServerTool, Description(@"Demonstrate the power of combined operations with real-world scenarios.

This tool showcases several practical use cases:
1. Multi-service data aggregation
2. Complex workflow execution
3. Schema comparison and migration planning
4. Performance monitoring across endpoints

Examples included:
- E-commerce order processing across multiple services
- User profile aggregation from different systems
- API migration compatibility checking
- Microservices health monitoring

This tool helps you understand how to leverage combined operations for:
- Reducing API round trips
- Building complex business workflows
- Monitoring distributed GraphQL services
- Planning schema migrations")]
    public static async Task<string> RunCombinedOperationsDemo(
        [Description("Demo scenario: 'ecommerce_order', 'user_profile_aggregation', 'api_migration_check', 'health_monitoring'")]
        string scenario,
        [Description("Mock data configuration as JSON (optional, uses defaults if not provided)")]
        string? mockConfig = null)
    {
        try
        {
            var config = string.IsNullOrEmpty(mockConfig) 
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(mockConfig) ?? new Dictionary<string, object>();

            object result = scenario.ToLower() switch
            {
                "ecommerce_order" => await DemoEcommerceOrderProcessing(config),
                "user_profile_aggregation" => await DemoUserProfileAggregation(config),
                "api_migration_check" => await DemoApiMigrationCheck(config),
                "health_monitoring" => await DemoHealthMonitoring(config),
                _ => throw new ArgumentException($"Unknown demo scenario: {scenario}")
            };

            return JsonSerializer.Serialize(new
            {
                scenario,
                demoDescription = GetScenarioDescription(scenario),
                result,
                tips = GetTipsForScenario(scenario),
                nextSteps = GetNextStepsForScenario(scenario),
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
                scenario,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description(@"Generate example GraphQL queries and operations for testing combined operations.

This utility tool helps you:
1. Generate realistic test queries for different domains
2. Create mock endpoint configurations
3. Build sample workflow configurations
4. Test combined operations with generated data

Domains supported:
- E-commerce (products, orders, customers, inventory)
- Social Media (users, posts, comments, likes)
- CRM (contacts, deals, companies, activities)
- Analytics (events, metrics, reports)
- Content Management (articles, categories, authors)")]
    public static string GenerateTestQueries(
        [Description("Domain type: 'ecommerce', 'social_media', 'crm', 'analytics', 'cms'")]
        string domain,
        [Description("Number of sample queries to generate (default: 5)")]
        int queryCount = 5,
        [Description("Include mutation examples (default: true)")]
        bool includeMutations = true,
        [Description("Complexity level: 'simple', 'medium', 'complex' (default: 'medium')")]
        string complexity = "medium")
    {
        try
        {
            var queries = GenerateQueriesForDomain(domain, queryCount, includeMutations, complexity);
            var mockEndpoints = GenerateMockEndpointsForDomain(domain);
            var workflowExamples = GenerateWorkflowExamplesForDomain(domain);

            return JsonSerializer.Serialize(new
            {
                domain,
                complexity,
                generatedQueries = queries,
                mockEndpoints,
                workflowExamples,
                usageInstructions = GetUsageInstructions(domain),
                generatedAt = DateTime.UtcNow
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
                domain,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    #region Demo Scenarios

    private static async Task<object> DemoEcommerceOrderProcessing(Dictionary<string, object> config)
    {
        await Task.CompletedTask; // Make it actually async
        // Simulate processing an order across multiple services
        var orderSteps = new List<object>
        {
            new { step = "validate_customer", service = "customer-service", duration = "50ms", status = "success" },
            new { step = "check_inventory", service = "inventory-service", duration = "120ms", status = "success" },
            new { step = "calculate_pricing", service = "pricing-service", duration = "80ms", status = "success" },
            new { step = "process_payment", service = "payment-service", duration = "200ms", status = "success" },
            new { step = "create_order", service = "order-service", duration = "90ms", status = "success" },
            new { step = "update_inventory", service = "inventory-service", duration = "60ms", status = "success" },
            new { step = "send_confirmation", service = "notification-service", duration = "40ms", status = "success" }
        };

        var totalDuration = orderSteps.Sum(s => int.Parse(s.GetType().GetProperty("duration")?.GetValue(s)?.ToString()?.Replace("ms", "") ?? "0"));

        return new
        {
            scenario = "E-commerce Order Processing",
            description = "Demonstrates how combined operations can orchestrate a complex multi-service order flow",
            workflow = new
            {
                type = "dependency_chain",
                steps = orderSteps,
                totalDuration = $"{totalDuration}ms",
                successRate = "100%"
            },
            benefits = new[]
            {
                "Single API call instead of 7 separate calls",
                "Automatic error handling and rollback",
                "Performance monitoring across all services",
                "Centralized business logic orchestration"
            },
            realWorldEquivalent = "Instead of the client making 7 API calls, one combined operation handles the entire flow"
        };
    }

    private static async Task<object> DemoUserProfileAggregation(Dictionary<string, object> config)
    {
        await Task.CompletedTask; // Make it actually async
        var dataSources = new Dictionary<string, object>
        {
            ["user-management"] = new { basicInfo = true, preferences = true, lastLogin = DateTime.UtcNow.AddHours(-2) },
            ["social-service"] = new { posts = 45, followers = 234, following = 123, socialScore = 8.5 },
            ["activity-service"] = new { loginStreak = 12, actionsToday = 8, totalActions = 1250 },
            ["recommendation-service"] = new { personalizedContent = true, recommendations = 15, engagementRate = 0.65 }
        };

        return new
        {
            scenario = "User Profile Aggregation",
            description = "Shows how to combine user data from multiple microservices into a unified profile",
            aggregatedProfile = new
            {
                userId = "user_12345",
                completeness = "95%",
                dataSources,
                lastUpdated = DateTime.UtcNow,
                dataQuality = new
                {
                    overall = "excellent",
                    freshness = "< 5 minutes",
                    consistency = "high"
                }
            },
            performanceMetrics = new
            {
                totalFetchTime = "180ms",
                parallelExecution = true,
                cacheHitRatio = "60%",
                dataCorrelationTime = "25ms"
            },
            advantages = new[]
            {
                "Complete user context in one call",
                "Automatic data correlation and merging",
                "Consistent error handling across services",
                "Built-in caching and performance optimization"
            }
        };
    }

    private static async Task<object> DemoApiMigrationCheck(Dictionary<string, object> config)
    {
        await Task.CompletedTask; // Make it actually async
        var migrationAnalysis = new
        {
            sourceEndpoint = "legacy-api-v1",
            targetEndpoint = "new-api-v2",
            compatibilityScore = 0.85,
            analysis = new
            {
                schemaComparison = new
                {
                    addedTypes = new[] { "ProductReview", "ShippingInfo", "PaymentMethod" },
                    removedTypes = new[] { "LegacyUser" },
                    modifiedTypes = new[] { "Product", "Order", "Customer" },
                    breakingChanges = new[]
                    {
                        "Product.price field changed from String to Float",
                        "Order.status enum values updated"
                    }
                },
                queryMigration = new
                {
                    directlyCompatible = 12,
                    needsModification = 3,
                    requiresRewrite = 1,
                    examples = new Dictionary<string, object>
                    {
                        ["getProduct"] = new { status = "compatible", confidence = "high" },
                        ["getOrders"] = new { status = "needs_modification", reason = "pagination parameters changed" },
                        ["getUserStats"] = new { status = "requires_rewrite", reason = "endpoint moved to analytics service" }
                    }
                }
            },
            migrationPlan = new
            {
                phases = new[]
                {
                    new { name = "Schema Analysis", duration = "1 week", risk = "low" },
                    new { name = "Query Mapping", duration = "2 weeks", risk = "medium" },
                    new { name = "Parallel Testing", duration = "2 weeks", risk = "medium" },
                    new { name = "Gradual Migration", duration = "3 weeks", risk = "high" },
                    new { name = "Legacy Sunset", duration = "1 week", risk = "low" }
                },
                totalDuration = "9 weeks",
                riskLevel = "medium"
            }
        };

        return new
        {
            scenario = "API Migration Compatibility Check",
            description = "Analyzes compatibility between API versions and generates migration plans",
            analysis = migrationAnalysis,
            recommendations = new[]
            {
                "Start with read-only operations migration",
                "Implement feature flags for gradual rollout",
                "Maintain parallel execution during transition",
                "Create automated compatibility tests"
            },
            toolsUsed = new[]
            {
                "Schema introspection and comparison",
                "Breaking change detection",
                "Query compatibility analysis",
                "Migration workflow generation"
            }
        };
    }

    private static async Task<object> DemoHealthMonitoring(Dictionary<string, object> config)
    {
        await Task.CompletedTask; // Make it actually async
        var endpointHealth = new Dictionary<string, object>
        {
            ["user-service"] = new
            {
                status = "healthy",
                responseTime = "45ms",
                lastCheck = DateTime.UtcNow,
                uptime = "99.98%",
                activeConnections = 23,
                queryComplexity = "low"
            },
            ["product-service"] = new
            {
                status = "warning",
                responseTime = "250ms", 
                lastCheck = DateTime.UtcNow,
                uptime = "99.85%",
                activeConnections = 89,
                queryComplexity = "high",
                warnings = new[] { "High response time", "Approaching connection limit" }
            },
            ["order-service"] = new
            {
                status = "healthy",
                responseTime = "80ms",
                lastCheck = DateTime.UtcNow,
                uptime = "99.95%",
                activeConnections = 45,
                queryComplexity = "medium"
            },
            ["inventory-service"] = new
            {
                status = "degraded",
                responseTime = "400ms",
                lastCheck = DateTime.UtcNow,
                uptime = "98.20%",
                activeConnections = 12,
                queryComplexity = "medium",
                issues = new[] { "Slow database queries", "Cache misses" }
            }
        };

        return new
        {
            scenario = "Distributed GraphQL Health Monitoring",
            description = "Monitors health and performance across multiple GraphQL endpoints",
            overallHealth = new
            {
                status = "warning",
                healthyServices = 2,
                warningServices = 1,
                degradedServices = 1,
                criticalServices = 0,
                averageResponseTime = "194ms"
            },
            detailedHealth = endpointHealth,
            alerts = new[]
            {
                new { service = "product-service", severity = "warning", message = "Response time exceeding threshold" },
                new { service = "inventory-service", severity = "critical", message = "Performance degradation detected" }
            },
            recommendations = new[]
            {
                "Scale product-service to handle increased load",
                "Investigate inventory-service database performance",
                "Implement query complexity limits",
                "Enable response caching for frequently accessed data"
            },
            monitoringCapabilities = new[]
            {
                "Real-time health status across all endpoints",
                "Performance trend analysis",
                "Automatic alert generation",
                "Service dependency mapping",
                "Query complexity monitoring"
            }
        };
    }

    #endregion

    #region Helper Methods

    private static string GetScenarioDescription(string scenario)
    {
        return scenario.ToLower() switch
        {
            "ecommerce_order" => "Demonstrates orchestrating a complex multi-service order processing workflow",
            "user_profile_aggregation" => "Shows how to aggregate user data from multiple microservices",
            "api_migration_check" => "Analyzes API compatibility and generates migration plans",
            "health_monitoring" => "Monitors health and performance across distributed GraphQL services",
            _ => "Unknown scenario"
        };
    }

    private static List<string> GetTipsForScenario(string scenario)
    {
        return scenario.ToLower() switch
        {
            "ecommerce_order" => new List<string>
            {
                "Use dependency chains for operations that must run in sequence",
                "Implement proper error handling and rollback mechanisms",
                "Consider using parallel execution where operations are independent",
                "Monitor performance across the entire workflow"
            },
            "user_profile_aggregation" => new List<string>
            {
                "Use parallel collection for independent data sources",
                "Implement intelligent caching to reduce repeated calls",
                "Handle partial failures gracefully",
                "Correlate data using consistent user identifiers"
            },
            "api_migration_check" => new List<string>
            {
                "Always compare schemas before migration",
                "Test query compatibility with real data",
                "Plan for gradual migration with parallel execution",
                "Document all breaking changes and their impacts"
            },
            "health_monitoring" => new List<string>
            {
                "Set up automated health checks for all endpoints",
                "Monitor both performance and functionality",
                "Use combined operations to reduce monitoring overhead",
                "Implement alerting based on multiple metrics"
            },
            _ => new List<string> { "Explore the combined operations documentation for more insights" }
        };
    }

    private static List<string> GetNextStepsForScenario(string scenario)
    {
        return scenario.ToLower() switch
        {
            "ecommerce_order" => new List<string>
            {
                "Register your actual endpoints using RegisterEndpoint tool",
                "Use ExecuteMultipleOperations with real queries",
                "Implement error handling strategies",
                "Monitor workflow performance with ExecuteAdvancedWorkflow"
            },
            "user_profile_aggregation" => new List<string>
            {
                "Set up endpoints for each microservice",
                "Use GraphqlServiceManager with action='get_all_info'",
                "Implement data correlation logic",
                "Test with ExecuteAdvancedWorkflow using workflowType='data_aggregation'"
            },
            "api_migration_check" => new List<string>
            {
                "Register both source and target endpoints",
                "Run CompareAndAnalyzeSchemas tool",
                "Use ExecuteAdvancedWorkflow with workflowType='schema_migration'",
                "Create migration test plans"
            },
            "health_monitoring" => new List<string>
            {
                "Register all your GraphQL endpoints",
                "Set up regular health checks using GraphqlServiceManager",
                "Implement alerting based on performance metrics",
                "Use ExecuteAdvancedWorkflow for comprehensive monitoring"
            },
            _ => new List<string> { "Start by registering your GraphQL endpoints" }
        };
    }

    private static List<object> GenerateQueriesForDomain(string domain, int queryCount, bool includeMutations, string complexity)
    {
        var queries = new List<object>();
        
        switch (domain.ToLower())
        {
            case "ecommerce":
                queries.AddRange(GenerateEcommerceQueries(queryCount, includeMutations, complexity));
                break;
            case "social_media":
                queries.AddRange(GenerateSocialMediaQueries(queryCount, includeMutations, complexity));
                break;
            case "crm":
                queries.AddRange(GenerateCrmQueries(queryCount, includeMutations, complexity));
                break;
            case "analytics":
                queries.AddRange(GenerateAnalyticsQueries(queryCount, includeMutations, complexity));
                break;
            case "cms":
                queries.AddRange(GenerateCmsQueries(queryCount, includeMutations, complexity));
                break;
            default:
                queries.Add(new { query = "query { __schema { types { name } } }", type = "introspection", complexity = "simple" });
                break;
        }

        return queries.Take(queryCount).ToList();
    }

    private static List<object> GenerateEcommerceQueries(int count, bool includeMutations, string complexity)
    {
        var queries = new List<object>
        {
            new { query = "query { products(limit: 10) { id name price category } }", type = "query", complexity = "simple" },
            new { query = "query { product(id: \"123\") { id name description price images { url } } }", type = "query", complexity = "medium" },
            new { query = "query { orders(status: PENDING) { id customer { name email } items { product { name } quantity } } }", type = "query", complexity = "complex" }
        };

        if (includeMutations)
        {
            queries.AddRange(new[]
            {
                new { query = "mutation { addToCart(productId: \"123\", quantity: 2) { cart { totalItems } } }", type = "mutation", complexity = "simple" },
                new { query = "mutation { createOrder(input: {customerId: \"456\", items: [{productId: \"123\", quantity: 1}]}) { id status } }", type = "mutation", complexity = "medium" }
            });
        }

        return queries;
    }

    private static List<object> GenerateSocialMediaQueries(int count, bool includeMutations, string complexity)
    {
        var queries = new List<object>
        {
            new { query = "query { posts(limit: 20) { id content author { username } likes comments { count } } }", type = "query", complexity = "medium" },
            new { query = "query { user(username: \"johndoe\") { profile { bio followers following posts { count } } } }", type = "query", complexity = "complex" },
            new { query = "query { timeline { posts { id content timestamp author { username avatar } } } }", type = "query", complexity = "complex" }
        };

        if (includeMutations)
        {
            queries.AddRange(new[]
            {
                new { query = "mutation { createPost(content: \"Hello world!\") { id timestamp } }", type = "mutation", complexity = "simple" },
                new { query = "mutation { likePost(postId: \"789\") { post { likes } } }", type = "mutation", complexity = "simple" }
            });
        }

        return queries;
    }

    private static List<object> GenerateCrmQueries(int count, bool includeMutations, string complexity)
    {
        var queries = new List<object>
        {
            new { query = "query { contacts(limit: 50) { id firstName lastName email company } }", type = "query", complexity = "simple" },
            new { query = "query { deals(stage: NEGOTIATION) { id name amount contact { name company } activities { type date } } }", type = "query", complexity = "complex" },
            new { query = "query { companies { id name contacts { count } deals { totalValue } } }", type = "query", complexity = "medium" }
        };

        if (includeMutations)
        {
            queries.AddRange(new[]
            {
                new { query = "mutation { createContact(input: {firstName: \"John\", lastName: \"Doe\", email: \"john@example.com\"}) { id } }", type = "mutation", complexity = "medium" },
                new { query = "mutation { updateDeal(id: \"456\", stage: CLOSED_WON) { deal { stage amount } } }", type = "mutation", complexity = "simple" }
            });
        }

        return queries;
    }

    private static List<object> GenerateAnalyticsQueries(int count, bool includeMutations, string complexity)
    {
        var queries = new List<object>
        {
            new { query = "query { metrics(timeRange: LAST_7_DAYS) { pageViews uniqueVisitors conversionRate } }", type = "query", complexity = "medium" },
            new { query = "query { events(type: \"purchase\", limit: 100) { timestamp userId properties { value currency } } }", type = "query", complexity = "complex" },
            new { query = "query { reports { dashboard { widgets { type data } } } }", type = "query", complexity = "complex" }
        };

        if (includeMutations)
        {
            queries.Add(new { query = "mutation { trackEvent(type: \"page_view\", properties: {page: \"/home\"}) { success } }", type = "mutation", complexity = "simple" });
        }

        return queries;
    }

    private static List<object> GenerateCmsQueries(int count, bool includeMutations, string complexity)
    {
        var queries = new List<object>
        {
            new { query = "query { articles(published: true, limit: 10) { id title excerpt author publishedAt } }", type = "query", complexity = "medium" },
            new { query = "query { article(slug: \"getting-started\") { title content author { name bio } tags categories } }", type = "query", complexity = "complex" },
            new { query = "query { categories { id name articles { count } } }", type = "query", complexity = "simple" }
        };

        if (includeMutations)
        {
            queries.AddRange(new[]
            {
                new { query = "mutation { createArticle(input: {title: \"New Article\", content: \"Content here\"}) { id slug } }", type = "mutation", complexity = "medium" },
                new { query = "mutation { publishArticle(id: \"123\") { article { status publishedAt } } }", type = "mutation", complexity = "simple" }
            });
        }

        return queries;
    }

    private static object GenerateMockEndpointsForDomain(string domain)
    {
        return domain.ToLower() switch
        {
            "ecommerce" => new
            {
                endpoints = new[]
                {
                    new { name = "product-service", url = "http://localhost:4001/graphql", description = "Product catalog and inventory" },
                    new { name = "order-service", url = "http://localhost:4002/graphql", description = "Order management and fulfillment" },
                    new { name = "customer-service", url = "http://localhost:4003/graphql", description = "Customer profiles and authentication" }
                }
            },
            "social_media" => new
            {
                endpoints = new[]
                {
                    new { name = "user-service", url = "http://localhost:4001/graphql", description = "User profiles and authentication" },
                    new { name = "content-service", url = "http://localhost:4002/graphql", description = "Posts, comments, and media" },
                    new { name = "social-graph", url = "http://localhost:4003/graphql", description = "Followers, friends, and relationships" }
                }
            },
            _ => new { endpoints = new[] { new { name = "main-service", url = "http://localhost:4000/graphql", description = "Main GraphQL service" } } }
        };
    }

    private static object GenerateWorkflowExamplesForDomain(string domain)
    {
        return domain.ToLower() switch
        {
            "ecommerce" => new
            {
                workflows = new object[]
                {
                    new
                    {
                        name = "Order Processing Workflow",
                        type = "dependency_chain",
                        description = "Process a complete order from cart to fulfillment",
                        steps = new[] { "validate_customer", "check_inventory", "process_payment", "create_order", "update_inventory" }
                    },
                    new
                    {
                        name = "Product Data Aggregation",
                        type = "data_aggregation",
                        description = "Combine product info, reviews, and inventory data",
                        endpoints = new[] { "product-service", "review-service", "inventory-service" }
                    }
                }
            },
            _ => new { workflows = new object[] { new { name = "Basic Health Check", type = "parallel_collection", description = "Check all services" } } }
        };
    }

    private static List<string> GetUsageInstructions(string domain)
    {
        return new List<string>
        {
            $"1. Register the mock endpoints for {domain} using the RegisterEndpoint tool",
            "2. Use the generated queries with GraphqlServiceManager",
            "3. Test workflows with ExecuteAdvancedWorkflow",
            "4. Monitor performance with the combined operations tools"
        };
    }

    #endregion
}
