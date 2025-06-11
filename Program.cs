using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System.Text.Json;
using System.Net.Http;
using Tools;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Register HttpClient with proper DI configuration
builder.Services.AddHttpClient("GraphQLClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register GraphQL HTTP client service
builder.Services.AddSingleton<IGraphQLHttpClient, GraphQLHttpClient>();

var name = Environment.GetEnvironmentVariable("NAME") ?? "mcp-graphql";
var schemaPath = Environment.GetEnvironmentVariable("SCHEMA");
if (!string.IsNullOrWhiteSpace(schemaPath) && File.Exists(schemaPath))
{
    var schemaContent = File.ReadAllText(schemaPath);
    builder.Services.AddSingleton(schemaContent);
}

Console.WriteLine($"Starting MCP server: {name}");
Console.WriteLine("Use the RegisterEndpoint tool to configure GraphQL endpoints dynamically");
Console.WriteLine("Available tools: RegisterEndpoint, ListDynamicTools, ExecuteDynamicOperation, RefreshEndpointTools, UnregisterEndpoint");

var app = builder.Build();

// Initialize the service provider for static tools
Tools.ServiceProvider.Initialize(app.Services);

await app.RunAsync();
