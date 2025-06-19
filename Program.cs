using Graphql.Mcp.Helpers;
using Graphql.Mcp.Prompts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithPrompts<GraphQlPrompts>()
    .WithPrompts<GraphQlWorkflowPrompts>()
    .WithToolsFromAssembly();

// Add caching services for smart responses
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.25;
});

// Add HTTP client with optimized configuration
builder.Services.AddHttpClient("GraphQLClient", client => 
{ 
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "GraphQL-MCP-Server/2.0");
});

// Register services for smart responses
builder.Services.AddSingleton<IGraphQlHttpClient, GraphQlHttpClient>();
// Note: CombinedOperationsService provides stateless operations for MCP tools
builder.Services.AddScoped<SmartResponseService>();

var app = builder.Build();

// Initialize service locator for static tool contexts
ServiceLocator.Initialize(app.Services);

await app.RunAsync();