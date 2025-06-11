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

var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? "http://localhost:4000/graphql";
var headersJson = Environment.GetEnvironmentVariable("HEADERS") ?? "{}";
Dictionary<string, string> headers;
try
{
    headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson) ?? new();
}
catch
{
    headers = new();
}
var allowMutations = (Environment.GetEnvironmentVariable("ALLOW_MUTATIONS") ?? "false").ToLower() == "true";

// Register HttpClient with proper DI configuration
builder.Services.AddHttpClient("GraphQLClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register GraphQL HTTP client service
builder.Services.AddSingleton<IGraphQLHttpClient, GraphQLHttpClient>();

// Register QueryGraphQLTool with proper DI
builder.Services.AddSingleton(provider => 
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("GraphQLClient");
    HttpClientHelper.ConfigureHeaders(httpClient, headers);
    return new QueryGraphQLTool(httpClient, endpoint, headers, allowMutations);
});

var name = Environment.GetEnvironmentVariable("NAME") ?? "mcp-graphql";
var schemaPath = Environment.GetEnvironmentVariable("SCHEMA");
if (!string.IsNullOrWhiteSpace(schemaPath) && File.Exists(schemaPath))
{
    var schemaContent = File.ReadAllText(schemaPath);
    builder.Services.AddSingleton(schemaContent);
}

Console.WriteLine($"Starting MCP server: {name}");

var app = builder.Build();

// Initialize the service provider for static tools
Tools.ServiceProvider.Initialize(app.Services);

await app.RunAsync();
