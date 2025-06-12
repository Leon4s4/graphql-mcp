using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tools;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions => consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient("GraphQLClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<IGraphQLHttpClient, GraphQLHttpClient>();

var name = Environment.GetEnvironmentVariable("NAME") ?? "mcp-graphql";
var schemaPath = Environment.GetEnvironmentVariable("SCHEMA");
if (!string.IsNullOrWhiteSpace(schemaPath) && File.Exists(schemaPath))
{
    var schemaContent = File.ReadAllText(schemaPath);
    builder.Services.AddSingleton(schemaContent);
}

var app = builder.Build();

await app.RunAsync();
