using Graphql.Mcp.Helpers;
using Graphql.Mcp.Prompts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions => consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithPrompts<GraphQlPrompts>()
    .WithPrompts<GraphQlWorkflowPrompts>()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient("GraphQLClient", client => { client.Timeout = TimeSpan.FromSeconds(30); });

builder.Services.AddSingleton<IGraphQlHttpClient, GraphQlHttpClient>();
builder.Services.AddSingleton<CombinedOperationsService>(_ => CombinedOperationsService.Instance);

var app = builder.Build();

await app.RunAsync();