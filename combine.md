using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GraphQLMCPServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<GraphQLService>();
                services.AddSingleton<MCPServer>();
            })
            .Build();

        var server = host.Services.GetRequiredService<MCPServer>();
        await server.RunAsync();
    }
}

public class GraphQLService
{
    private readonly ILogger<GraphQLService> _logger;
    
    // Cache schema information to avoid repeated introspection
    private static readonly ConcurrentDictionary<string, object> _schemaCache = new();
    private static readonly List<string> _availableEndpoints = new() { "local-crm", "inventory", "analytics" };

    public GraphQLService(ILogger<GraphQLService> logger)
    {
        _logger = logger;
    }

    public async Task<object> GetCompleteServiceInfo(string endpoint, string? action = null, string? query = null)
    {
        var result = new Dictionary<string, object>();

        switch (action?.ToLower())
        {
            case "get_schema":
                result["schema"] = await GetSchemaInfo(endpoint);
                break;
                
            case "list_queries":
                result["availableQueries"] = GetAvailableQueries(endpoint);
                result["exampleQueries"] = GetExampleQueries(endpoint);
                break;
                
            case "execute_query":
                if (string.IsNullOrEmpty(query))
                {
                    throw new ArgumentException("Query is required for execute_query action");
                }
                result["queryResult"] = await ExecuteQuery(endpoint, query);
                break;
                
            case "get_all_info":
            default:
                // Return everything at once
                result["endpoints"] = _availableEndpoints;
                result["schema"] = await GetSchemaInfo(endpoint);
                result["availableQueries"] = GetAvailableQueries(endpoint);
                result["exampleQueries"] = GetExampleQueries(endpoint);
                result["queryCapabilities"] = GetQueryCapabilities(endpoint);
                break;
        }

        return result;
    }

    private async Task<object> GetSchemaInfo(string endpoint)
    {
        if (_schemaCache.TryGetValue($"schema_{endpoint}", out var cachedSchema))
        {
            return cachedSchema;
        }

        // Simulate schema introspection
        var schema = endpoint switch
        {
            "local-crm" => new
            {
                types = new[]
                {
                    new { name = "User", fields = new[] { "id", "name", "email", "createdAt", "role" } },
                    new { name = "Contact", fields = new[] { "id", "firstName", "lastName", "email", "phone", "userId" } },
                    new { name = "Deal", fields = new[] { "id", "title", "amount", "status", "userId", "contactId" } }
                },
                queries = new[] { "getUsers", "getUserById", "getContacts", "getDeals" },
                mutations = new[] { "createUser", "updateUser", "createContact", "createDeal" }
            },
            _ => new { types = Array.Empty<object>(), queries = Array.Empty<string>(), mutations = Array.Empty<string>() }
        };

        _schemaCache[$"schema_{endpoint}"] = schema;
        return schema;
    }

    private string[] GetAvailableQueries(string endpoint)
    {
        return endpoint switch
        {
            "local-crm" => new[]
            {
                "getUsers(limit: Int, offset: Int): [User]",
                "getUserById(id: ID!): User",
                "getContacts(userId: ID): [Contact]",
                "getDeals(status: String): [Deal]"
            },
            _ => Array.Empty<string>()
        };
    }

    private string[] GetExampleQueries(string endpoint)
    {
        return endpoint switch
        {
            "local-crm" => new[]
            {
                "query { getUsers(limit: 10) { id name email role } }",
                "query { getUserById(id: \"123\") { name email } }",
                "query { getContacts { id firstName lastName email } }",
                "mutation { createUser(input: {name: \"John Doe\", email: \"john@example.com\"}) { id } }"
            },
            _ => Array.Empty<string>()
        };
    }

    private object GetQueryCapabilities(string endpoint)
    {
        return new
        {
            supportsFiltering = true,
            supportsPagination = true,
            supportsMutations = true,
            maxPageSize = 100,
            supportedOperations = new[] { "query", "mutation" }
        };
    }

    private async Task<object> ExecuteQuery(string endpoint, string query)
    {
        // Simulate query execution
        _logger.LogInformation("Executing query on {Endpoint}: {Query}", endpoint, query);
        
        await Task.Delay(100); // Simulate network call
        
        return new
        {
            data = new { message = "Query executed successfully", timestamp = DateTime.UtcNow },
            executionTime = "120ms"
        };
    }
}

public class MCPServer
{
    private readonly GraphQLService _graphQLService;
    private readonly ILogger<MCPServer> _logger;
    private Server _server;

    public MCPServer(GraphQLService graphQLService, ILogger<MCPServer> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
        
        _server = new Server(
            new ServerInfo("graphql-mcp-server", "1.0.0"),
            new ServerCapabilities()
        );

        ConfigureTools();
    }

    private void ConfigureTools()
    {
        // Single comprehensive tool that combines multiple operations
        _server.AddTool(
            "graphql_service_manager",
            "Complete GraphQL service management tool. Can get schema info, list queries, execute queries, or return all information at once.",
            new
            {
                type = "object",
                properties = new
                {
                    endpoint = new
                    {
                        type = "string",
                        description = "GraphQL endpoint name (e.g., 'local-crm', 'inventory', 'analytics')",
                        @enum = new[] { "local-crm", "inventory", "analytics" }
                    },
                    action = new
                    {
                        type = "string",
                        description = "Action to perform",
                        @enum = new[] { "get_all_info", "get_schema", "list_queries", "execute_query" },
                        @default = "get_all_info"
                    },
                    query = new
                    {
                        type = "string",
                        description = "GraphQL query string (required when action is 'execute_query')"
                    },
                    variables = new
                    {
                        type = "object",
                        description = "Query variables (optional)"
                    }
                },
                required = new[] { "endpoint" }
            },
            async (arguments) =>
            {
                var endpoint = arguments["endpoint"]?.ToString() ?? "local-crm";
                var action = arguments["action"]?.ToString() ?? "get_all_info";
                var query = arguments["query"]?.ToString();

                try
                {
                    var result = await _graphQLService.GetCompleteServiceInfo(endpoint, action, query);
                    
                    return new[]
                    {
                        new TextContent($"GraphQL Service Info for '{endpoint}':\n{JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}")
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing GraphQL service request");
                    return new[]
                    {
                        new TextContent($"Error: {ex.Message}")
                    };
                }
            }
        );

        // Optional: Keep a simple query executor for direct execution
        _server.AddTool(
            "execute_graphql_query",
            "Execute a GraphQL query directly against an endpoint. Use this when you already know the schema and just want to run a query.",
            new
            {
                type = "object",
                properties = new
                {
                    endpoint = new
                    {
                        type = "string",
                        description = "GraphQL endpoint name",
                        @enum = new[] { "local-crm", "inventory", "analytics" }
                    },
                    query = new
                    {
                        type = "string",
                        description = "GraphQL query string. Examples: 'query { getUsers { id name } }' or 'mutation { createUser(input: {name: \"John\"}) { id } }'"
                    },
                    variables = new
                    {
                        type = "object",
                        description = "Query variables"
                    }
                },
                required = new[] { "endpoint", "query" }
            },
            async (arguments) =>
            {
                var endpoint = arguments["endpoint"]?.ToString() ?? "";
                var query = arguments["query"]?.ToString() ?? "";

                try
                {
                    var result = await _graphQLService.GetCompleteServiceInfo(endpoint, "execute_query", query);
                    
                    return new[]
                    {
                        new TextContent($"Query Result:\n{JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}")
                    };
                }
                catch (Exception ex)
                {
                    return new[]
                    {
                        new TextContent($"Query execution failed: {ex.Message}")
                    };
                }
            }
        );
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting GraphQL MCP Server...");
        await _server.RunAsync();
    }
}