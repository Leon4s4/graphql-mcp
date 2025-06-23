using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Prompts;

[McpServerPromptType]
public class IntegrationPrompts
{
    [McpServerPrompt, Description("Dynamics 365 to C# GraphQL integration patterns")]
    public static string DynamicsGraphQLIntegration(
        [Description("Dynamics focus: entities, queries, operations, authentication")]
        string focus = "entities",
        [Description("C# client type: console, web, service, wpf")]
        string clientType = "web",
        [Description("Authentication method: service-principal, user-delegation, certificate")]
        string authMethod = "service-principal")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# Dynamics 365 GraphQL Integration Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a Dynamics 365 and C# GraphQL integration expert. Provide guidance for connecting Dynamics 365 to C# applications via GraphQL.");
        prompt.AppendLine();
        prompt.AppendLine($"**Dynamics Focus:** {focus}");
        prompt.AppendLine($"**C# Client Type:** {clientType}");
        prompt.AppendLine($"**Authentication Method:** {authMethod}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "entities":
                prompt.AppendLine("## Dynamics Entity Integration:");
                prompt.AppendLine("### Entity Mapping:");
                prompt.AppendLine("- Dynamics entity to GraphQL type mapping");
                prompt.AppendLine("- Custom attribute and field handling");
                prompt.AppendLine("- Relationship navigation and expansion");
                prompt.AppendLine("- Option set and lookup value resolution");
                prompt.AppendLine();
                prompt.AppendLine("### Data Access Patterns:");
                prompt.AppendLine("- Efficient OData query generation from GraphQL");
                prompt.AppendLine("- Batch operations and change tracking");
                prompt.AppendLine("- Error handling for Dynamics-specific errors");
                prompt.AppendLine("- Connection management and retry policies");
                break;

            case "queries":
                prompt.AppendLine("## Query Translation Patterns:");
                prompt.AppendLine("### GraphQL to OData:");
                prompt.AppendLine("- Field selection to $select parameters");
                prompt.AppendLine("- Filtering arguments to OData $filter");
                prompt.AppendLine("- Pagination to $skip and $top");
                prompt.AppendLine("- Sorting to $orderby expressions");
                prompt.AppendLine();
                prompt.AppendLine("### Complex Queries:");
                prompt.AppendLine("- Multi-entity joins and relationships");
                prompt.AppendLine("- Aggregate functions and grouping");
                prompt.AppendLine("- Conditional logic and dynamic queries");
                break;

            case "operations":
                prompt.AppendLine("## CRUD Operations:");
                prompt.AppendLine("### Create Operations:");
                prompt.AppendLine("- Entity creation with validation");
                prompt.AppendLine("- Related entity association");
                prompt.AppendLine("- Bulk insert operations");
                prompt.AppendLine();
                prompt.AppendLine("### Update Operations:");
                prompt.AppendLine("- Patch vs full update strategies");
                prompt.AppendLine("- Optimistic concurrency handling");
                prompt.AppendLine("- Relationship updates and associations");
                break;

            case "authentication":
                prompt.AppendLine("## Authentication Integration:");
                prompt.AppendLine("### Service Principal:");
                prompt.AppendLine("- Azure AD app registration setup");
                prompt.AppendLine("- Client credentials flow implementation");
                prompt.AppendLine("- Token caching and refresh strategies");
                prompt.AppendLine();
                prompt.AppendLine("### Security Best Practices:");
                prompt.AppendLine("- Principle of least privilege");
                prompt.AppendLine("- Secure credential storage");
                prompt.AppendLine("- Connection string protection");
                break;
        }

        switch (clientType.ToLower())
        {
            case "console":
                prompt.AppendLine();
                prompt.AppendLine("## Console Application Integration:");
                prompt.AppendLine("- HttpClient configuration for GraphQL");
                prompt.AppendLine("- Command-line argument parsing");
                prompt.AppendLine("- Batch processing and automation scenarios");
                prompt.AppendLine("- Error handling and logging");
                break;

            case "web":
                prompt.AppendLine();
                prompt.AppendLine("## Web Application Integration:");
                prompt.AppendLine("- ASP.NET Core dependency injection setup");
                prompt.AppendLine("- Controller-based GraphQL consumption");
                prompt.AppendLine("- Blazor component integration patterns");
                prompt.AppendLine("- API middleware and authentication");
                break;

            case "service":
                prompt.AppendLine();
                prompt.AppendLine("## Windows Service Integration:");
                prompt.AppendLine("- Background service implementation");
                prompt.AppendLine("- Scheduled data synchronization");
                prompt.AppendLine("- Service lifecycle management");
                prompt.AppendLine("- Health monitoring and diagnostics");
                break;

            case "wpf":
                prompt.AppendLine();
                prompt.AppendLine("## WPF Desktop Integration:");
                prompt.AppendLine("- MVVM pattern with GraphQL data binding");
                prompt.AppendLine("- Async/await patterns in UI thread");
                prompt.AppendLine("- ObservableCollection updates");
                prompt.AppendLine("- Offline capability and sync");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Dynamics Integration Tools:");
        prompt.AppendLine("1. **QueryTools** - Create optimized Dynamics queries");
        prompt.AppendLine("2. **SchemaTools** - Map Dynamics entities to GraphQL");
        prompt.AppendLine("3. **DevelopmentTools** - Generate C# client code");
        prompt.AppendLine("4. **UtilityTools** - Test Dynamics connectivity");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("REST/WCF to GraphQL migration patterns")]
    public static string LegacyMigrationPatterns(
        [Description("Legacy type: rest-api, wcf-service, soap, web-service")]
        string legacyType = "rest-api",
        [Description("Migration approach: wrapper, replacement, hybrid")]
        string approach = "wrapper",
        [Description("Client impact: minimal, moderate, breaking")]
        string clientImpact = "minimal")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# Legacy to GraphQL Migration Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a legacy system migration expert. Provide guidance for migrating REST/WCF services to GraphQL.");
        prompt.AppendLine();
        prompt.AppendLine($"**Legacy Type:** {legacyType}");
        prompt.AppendLine($"**Migration Approach:** {approach}");
        prompt.AppendLine($"**Client Impact:** {clientImpact}");
        prompt.AppendLine();

        prompt.AppendLine("## Migration Challenges:");
        prompt.AppendLine("### Legacy System Analysis:");
        prompt.AppendLine("- Endpoint inventory and documentation");
        prompt.AppendLine("- Data model and schema analysis");
        prompt.AppendLine("- Authentication mechanism mapping");
        prompt.AppendLine("- Performance baseline establishment");
        prompt.AppendLine();
        prompt.AppendLine("### Client Compatibility:");
        prompt.AppendLine("- Maintain existing client contracts");
        prompt.AppendLine("- Gradual migration strategies");
        prompt.AppendLine("- Version management and deprecation");
        prompt.AppendLine("- Rollback and contingency planning");
        prompt.AppendLine();

        switch (legacyType.ToLower())
        {
            case "rest-api":
                prompt.AppendLine("## REST API Migration:");
                prompt.AppendLine("### Endpoint Analysis:");
                prompt.AppendLine("- REST resource to GraphQL type mapping");
                prompt.AppendLine("- HTTP verb to GraphQL operation conversion");
                prompt.AppendLine("- Query parameter to GraphQL argument mapping");
                prompt.AppendLine("- Response payload optimization");
                prompt.AppendLine();
                prompt.AppendLine("### Data Transformation:");
                prompt.AppendLine("- JSON response to GraphQL schema alignment");
                prompt.AppendLine("- Error response standardization");
                prompt.AppendLine("- Pagination pattern conversion");
                break;

            case "wcf-service":
                prompt.AppendLine("## WCF Service Migration:");
                prompt.AppendLine("### Service Contract Analysis:");
                prompt.AppendLine("- WCF operation to GraphQL field mapping");
                prompt.AppendLine("- Data contract to GraphQL type conversion");
                prompt.AppendLine("- Service behavior preservation");
                prompt.AppendLine("- Transaction and session handling migration");
                prompt.AppendLine();
                prompt.AppendLine("### Protocol Translation:");
                prompt.AppendLine("- SOAP to HTTP/JSON transformation");
                prompt.AppendLine("- Security context migration");
                prompt.AppendLine("- Fault contract to GraphQL error mapping");
                break;

            case "soap":
                prompt.AppendLine("## SOAP Service Migration:");
                prompt.AppendLine("### WSDL Analysis:");
                prompt.AppendLine("- Operation definition extraction");
                prompt.AppendLine("- Complex type hierarchy flattening");
                prompt.AppendLine("- Namespace and schema consolidation");
                prompt.AppendLine();
                prompt.AppendLine("### Protocol Migration:");
                prompt.AppendLine("- XML to JSON transformation");
                prompt.AppendLine("- SOAP envelope removal");
                prompt.AppendLine("- Header and security token handling");
                break;
        }

        switch (approach.ToLower())
        {
            case "wrapper":
                prompt.AppendLine();
                prompt.AppendLine("## Wrapper Approach:");
                prompt.AppendLine("### GraphQL Facade:");
                prompt.AppendLine("- Create GraphQL layer over existing services");
                prompt.AppendLine("- Preserve original service contracts");
                prompt.AppendLine("- Minimize changes to backend systems");
                prompt.AppendLine("- Provide unified GraphQL API surface");
                prompt.AppendLine();
                prompt.AppendLine("### Benefits and Considerations:");
                prompt.AppendLine("- Rapid implementation timeline");
                prompt.AppendLine("- Lower risk migration strategy");
                prompt.AppendLine("- Potential performance overhead");
                prompt.AppendLine("- Limited optimization opportunities");
                break;

            case "replacement":
                prompt.AppendLine();
                prompt.AppendLine("## Replacement Approach:");
                prompt.AppendLine("### Complete Migration:");
                prompt.AppendLine("- Rebuild services with GraphQL-first design");
                prompt.AppendLine("- Optimize data access patterns");
                prompt.AppendLine("- Implement modern authentication");
                prompt.AppendLine("- Enhanced error handling and monitoring");
                prompt.AppendLine();
                prompt.AppendLine("### Migration Strategy:");
                prompt.AppendLine("- Phased rollout with feature flags");
                prompt.AppendLine("- Parallel system operation");
                prompt.AppendLine("- Comprehensive testing and validation");
                break;

            case "hybrid":
                prompt.AppendLine();
                prompt.AppendLine("## Hybrid Approach:");
                prompt.AppendLine("### Gradual Migration:");
                prompt.AppendLine("- Selective service modernization");
                prompt.AppendLine("- GraphQL for new features, legacy for existing");
                prompt.AppendLine("- Service-by-service migration timeline");
                prompt.AppendLine("- Cross-system data federation");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Legacy Migration Tools:");
        prompt.AppendLine("1. **ExtractGraphQLFromCSharpCode** - Analyze existing service contracts");
        prompt.AppendLine("2. **GenerateOptimizedGraphQLQueries** - Create GraphQL equivalents");
        prompt.AppendLine("3. **CompareSchemas** - Validate migration completeness");
        prompt.AppendLine("4. **CreateTestSuite** - Generate migration validation tests");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("C# GraphQL caching strategies and implementation")]
    public static string CSharpGraphQLCaching(
        [Description("Caching level: memory, redis, database, distributed")]
        string level = "memory",
        [Description("Cache provider: memorycache, redis, ncache, hazelcast")]
        string provider = "redis",
        [Description("Invalidation strategy: ttl, dependency, event-driven")]
        string invalidation = "dependency")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# C# GraphQL Caching Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a C# GraphQL caching expert. Provide guidance for implementing efficient caching in C# GraphQL applications.");
        prompt.AppendLine();
        prompt.AppendLine($"**Caching Level:** {level}");
        prompt.AppendLine($"**Cache Provider:** {provider}");
        prompt.AppendLine($"**Invalidation Strategy:** {invalidation}");
        prompt.AppendLine();

        switch (level.ToLower())
        {
            case "memory":
                prompt.AppendLine("## In-Memory Caching:");
                prompt.AppendLine("### IMemoryCache Implementation:");
                prompt.AppendLine("- ASP.NET Core IMemoryCache service registration");
                prompt.AppendLine("- Cache key generation strategies");
                prompt.AppendLine("- Memory pressure and eviction policies");
                prompt.AppendLine("- Thread-safe cache operations");
                prompt.AppendLine();
                prompt.AppendLine("### DataLoader Pattern:");
                prompt.AppendLine("- HotChocolate DataLoader implementation");
                prompt.AppendLine("- Batch loading for N+1 query prevention");
                prompt.AppendLine("- Request-scoped caching");
                prompt.AppendLine("- Custom DataLoader creation");
                break;

            case "redis":
                prompt.AppendLine("## Redis Caching:");
                prompt.AppendLine("### StackExchange.Redis Integration:");
                prompt.AppendLine("- Connection multiplexer configuration");
                prompt.AppendLine("- Distributed cache implementation");
                prompt.AppendLine("- JSON serialization for complex objects");
                prompt.AppendLine("- Redis cluster and failover support");
                prompt.AppendLine();
                prompt.AppendLine("### Cache Strategies:");
                prompt.AppendLine("- Cache-aside pattern implementation");
                prompt.AppendLine("- Write-through and write-behind patterns");
                prompt.AppendLine("- Cache warming strategies");
                break;

            case "database":
                prompt.AppendLine("## Database-Level Caching:");
                prompt.AppendLine("### Entity Framework Caching:");
                prompt.AppendLine("- Second-level cache configuration");
                prompt.AppendLine("- Query result caching");
                prompt.AppendLine("- DbContext pooling optimization");
                prompt.AppendLine("- Compiled query caching");
                prompt.AppendLine();
                prompt.AppendLine("### SQL Server Caching:");
                prompt.AppendLine("- Query plan cache optimization");
                prompt.AppendLine("- Table-level cache dependencies");
                prompt.AppendLine("- In-memory OLTP integration");
                break;

            case "distributed":
                prompt.AppendLine("## Distributed Caching:");
                prompt.AppendLine("### Multi-Tier Caching:");
                prompt.AppendLine("- L1 (local) and L2 (distributed) cache coordination");
                prompt.AppendLine("- Cache coherence strategies");
                prompt.AppendLine("- Network partition handling");
                prompt.AppendLine("- Cache topology optimization");
                break;
        }

        switch (provider.ToLower())
        {
            case "memorycache":
                prompt.AppendLine();
                prompt.AppendLine("## MemoryCache Provider:");
                prompt.AppendLine("### Configuration:");
                prompt.AppendLine("- services.AddMemoryCache() registration");
                prompt.AppendLine("- MemoryCacheOptions configuration");
                prompt.AppendLine("- Size limits and compaction");
                prompt.AppendLine("- Cache entry options and priorities");
                break;

            case "redis":
                prompt.AppendLine();
                prompt.AppendLine("## Redis Provider:");
                prompt.AppendLine("### Setup and Configuration:");
                prompt.AppendLine("- services.AddStackExchangeRedisCache() setup");
                prompt.AppendLine("- Connection string and options");
                prompt.AppendLine("- Redis instance modes (standalone, cluster, sentinel)");
                prompt.AppendLine("- SSL/TLS configuration for production");
                break;

            case "ncache":
                prompt.AppendLine();
                prompt.AppendLine("## NCache Provider:");
                prompt.AppendLine("### Enterprise Caching:");
                prompt.AppendLine("- NCache client configuration");
                prompt.AppendLine("- Clustered cache topology");
                prompt.AppendLine("- Cache synchronization strategies");
                prompt.AppendLine("- High availability and disaster recovery");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## C# Caching Tools:");
        prompt.AppendLine("1. **AnalyzeQuery** - Cache optimization analysis");
        prompt.AppendLine("2. **ExecuteQuery** - Cache performance testing");
        prompt.AppendLine("3. **CreateTestSuite** - Cache validation scenarios");
        prompt.AppendLine("4. **DebugGraphQL** - Cache debugging and troubleshooting");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("C# GraphQL real-time subscriptions implementation")]
    public static string CSharpGraphQLSubscriptions(
        [Description("Subscription focus: setup, signalr, websockets, streaming")]
        string focus = "setup",
        [Description("Transport: signalr, websockets, server-sent-events")]
        string transport = "signalr",
        [Description("Scale: small, medium, large")]
        string scale = "medium")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# C# GraphQL Subscriptions Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a C# GraphQL subscriptions expert. Provide guidance for implementing real-time GraphQL features in C# applications.");
        prompt.AppendLine();
        prompt.AppendLine($"**Subscription Focus:** {focus}");
        prompt.AppendLine($"**Transport:** {transport}");
        prompt.AppendLine($"**Scale:** {scale}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "setup":
                prompt.AppendLine("## C# Subscription Setup:");
                prompt.AppendLine("### HotChocolate Configuration:");
                prompt.AppendLine("- AddGraphQLServer() with subscription support");
                prompt.AppendLine("- Subscription type definition and registration");
                prompt.AppendLine("- WebSocket middleware configuration");
                prompt.AppendLine("- Subscription resolver implementation");
                prompt.AppendLine();
                prompt.AppendLine("### ASP.NET Core Integration:");
                prompt.AppendLine("- UseWebSockets() middleware setup");
                prompt.AppendLine("- CORS configuration for WebSocket connections");
                prompt.AppendLine("- Authentication in subscription context");
                prompt.AppendLine("- Dependency injection for subscription services");
                break;

            case "signalr":
                prompt.AppendLine("## SignalR Integration:");
                prompt.AppendLine("### Hub Configuration:");
                prompt.AppendLine("- SignalR hub implementation for GraphQL");
                prompt.AppendLine("- Group management for targeted updates");
                prompt.AppendLine("- Connection state tracking");
                prompt.AppendLine("- Message broadcasting patterns");
                prompt.AppendLine();
                prompt.AppendLine("### Client Integration:");
                prompt.AppendLine("- .NET client connection setup");
                prompt.AppendLine("- JavaScript client integration");
                prompt.AppendLine("- Automatic reconnection handling");
                break;

            case "websockets":
                prompt.AppendLine("## Native WebSocket Implementation:");
                prompt.AppendLine("### WebSocket Handler:");
                prompt.AppendLine("- Custom WebSocket middleware");
                prompt.AppendLine("- GraphQL over WebSocket protocol");
                prompt.AppendLine("- Message framing and parsing");
                prompt.AppendLine("- Connection lifecycle management");
                break;

            case "streaming":
                prompt.AppendLine("## IAsyncEnumerable Streaming:");
                prompt.AppendLine("### Streaming Patterns:");
                prompt.AppendLine("- IAsyncEnumerable<T> subscription resolvers");
                prompt.AppendLine("- Channel-based data streaming");
                prompt.AppendLine("- Backpressure handling");
                prompt.AppendLine("- Memory-efficient streaming");
                break;
        }

        switch (transport.ToLower())
        {
            case "signalr":
                prompt.AppendLine();
                prompt.AppendLine("## SignalR Transport:");
                prompt.AppendLine("### Configuration:");
                prompt.AppendLine("- services.AddSignalR() registration");
                prompt.AppendLine("- Hub endpoint mapping");
                prompt.AppendLine("- Client proxy generation");
                prompt.AppendLine("- Transport fallback strategies");
                break;

            case "websockets":
                prompt.AppendLine();
                prompt.AppendLine("## WebSocket Transport:");
                prompt.AppendLine("### Implementation:");
                prompt.AppendLine("- GraphQL WebSocket subprotocol");
                prompt.AppendLine("- Message routing and handling");
                prompt.AppendLine("- Connection heartbeat and timeout");
                prompt.AppendLine("- Error handling and cleanup");
                break;

            case "server-sent-events":
                prompt.AppendLine();
                prompt.AppendLine("## Server-Sent Events:");
                prompt.AppendLine("### SSE Implementation:");
                prompt.AppendLine("- EventSource compatible responses");
                prompt.AppendLine("- Streaming response formatting");
                prompt.AppendLine("- Client reconnection handling");
                prompt.AppendLine("- Event ID and retry management");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## C# Subscription Tools:");
        prompt.AppendLine("1. **SchemaTools** - Analyze subscription schema definitions");
        prompt.AppendLine("2. **DevelopmentTools** - Generate subscription resolver code");
        prompt.AppendLine("3. **CreateTestSuite** - Real-time testing scenarios");
        prompt.AppendLine("4. **DebugGraphQL** - Subscription debugging and troubleshooting");

        return prompt.ToString();
    }
}