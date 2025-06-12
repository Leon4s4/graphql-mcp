using Graphql.Mcp.Helpers;
using Graphql.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions => consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient("GraphQLClient", client => { client.Timeout = TimeSpan.FromSeconds(30); });

builder.Services.AddSingleton<IGraphQlHttpClient, GraphQlHttpClient>();

var app = builder.Build();

await app.RunAsync();