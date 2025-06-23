# GraphQL MCP Server

A Model Context Protocol (MCP) server providing GraphQL introspection, querying, and development tools.

## Overview

The GraphQL MCP Server offers consolidated tools for GraphQL operations with 70% fewer tool files while maintaining full functionality.

**✅ Consolidation Status**: **COMPLETED** - Tool consolidation implemented and verified.

## Tools

### SchemaTools

**IntrospectSchema** - Schema introspection with multiple output formats
- `endpointName`: Registered endpoint name
- `format`: "detailed", "operations", "types", "summary" (default: "detailed")
- `includeMutations`: Include mutations (default: true)
- `includeSubscriptions`: Include subscriptions (default: false)

**CompareSchemas** - Compare schemas between endpoints
- `endpoint1`: First endpoint name
- `endpoint2`: Second endpoint name
- `includeFieldAnalysis`: Include field-level analysis (default: true)
- `detectBreakingChanges`: Detect breaking changes (default: true)

**GetTypeDefinition** - Get detailed type information
- `endpointName`: Registered endpoint name
- `typeName`: Type name to analyze
- `includeRelated`: Include related types (default: false)
- `format`: "sdl", "detailed", "summary" (default: "detailed")

**ListOperations** - List available operations
- `endpointName`: Registered endpoint name
- `operationType`: "query", "mutation", "subscription", "all" (default: "all")
- `includeParameters`: Include parameter details (default: true)
- `includeExamples`: Include examples (default: false)

**SearchSchema** - Search schema with patterns
- `endpointName`: Registered endpoint name
- `searchPattern`: Search pattern (regex supported)
- `searchTarget`: "types", "fields", "operations", "all" (default: "all")
- `caseSensitive`: Case-sensitive search (default: false)

### QueryTools

**AnalyzeQuery** - Query analysis and validation
- `query`: GraphQL query string
- `analysisLevel`: "quick", "standard", "comprehensive" (default: "standard")
- `endpointName`: Endpoint for validation (optional)
- `includePerformance`: Include performance analysis (default: true)
- `includeSecurity`: Include security analysis (default: true)

**ExecuteQuery** - Execute queries with metrics
- `endpointName`: Registered endpoint name
- `query`: GraphQL query to execute
- `variables`: Query variables as JSON (optional)
- `includeMetrics`: Include execution metrics (default: true)
- `responseFormat`: "json", "formatted", "summary" (default: "formatted")

**BuildQuery** - Build queries from schema
- `endpointName`: Registered endpoint name
- `operationType`: "query", "mutation", "subscription" (default: "query")
- `target`: Target type or operation name (optional)
- `fields`: Fields to include (comma-separated, optional)
- `includeRelated`: Include related fields (default: false)

**OptimizeQuery** - Query formatting and optimization
- `query`: GraphQL query to optimize
- `optimizationType`: "format", "performance", "structure", "all" (default: "all")
- `extractVariables`: Extract variables (default: false)
- `suggestFragments`: Suggest fragments (default: false)

### DevelopmentTools

**DebugGraphQL** - Debug GraphQL operations
- `debugType`: "query", "schema", "response", "performance"
- `input`: Content to debug
- `endpointName`: Endpoint for validation (optional)
- `includeDetailedAnalysis`: Include detailed analysis (default: true)
- `includeResolution`: Include resolution suggestions (default: true)

**GenerateTestData** - Generate test data and mocks
- `endpointName`: Registered endpoint name
- `typeName`: Type for test data (optional)
- `recordCount`: Number of records (default: 5)
- `includeEdgeCases`: Include edge cases (default: true)
- `style`: "realistic", "minimal", "comprehensive" (default: "realistic")

**ExplainError** - Explain GraphQL errors
- `errorInput`: Error message or response
- `includeResolution`: Include resolution steps (default: true)
- `includePrevention`: Include prevention strategies (default: true)
- `includeExamples`: Include code examples (default: false)

**CreateTestSuite** - Create testing suites
- `endpointName`: Registered endpoint name
- `testType`: "functional", "performance", "security", "integration" (default: "functional")
- `includePositiveCases`: Include positive tests (default: true)
- `includeNegativeCases`: Include negative tests (default: true)

### UtilityTools

**FormatQuery** - Format GraphQL queries
- `query`: GraphQL query to format
- `indentSize`: Indentation size (default: 2)
- `removeComments`: Remove comments (default: false)

**MinifyQuery** - Compress queries for production
- `query`: GraphQL query to minify
- `preserveVariables`: Preserve variable names (default: true)
- `removeWhitespace`: Remove unnecessary whitespace (default: true)

**ExtractVariables** - Extract hardcoded values to variables
- `query`: GraphQL query to process
- `includeStringValues`: Extract string values (default: true)
- `includeNumericValues`: Extract numeric values (default: true)

**GenerateAliases** - Generate field aliases
- `query`: GraphQL query
- `aliasPattern`: Alias naming pattern (default: "auto")
- `conflictResolution`: How to resolve conflicts (default: "suffix")

**GenerateCode** - Generate client code from schema
- `endpointName`: Registered endpoint name
- `language`: "typescript", "javascript", "python", "csharp", "java"
- `outputStyle`: "classes", "interfaces", "types" (default: "classes")
- `includeComments`: Include documentation (default: true)

### EndpointManagementTools

**RegisterEndpoint** - Register GraphQL endpoint
- `name`: Endpoint name
- `url`: GraphQL endpoint URL
- `headers`: Custom headers as JSON (optional)
- `includeMutations`: Include mutations (default: false)
- `toolPrefix`: Tool name prefix (optional)

**GetAllEndpoints** - List registered endpoints
- `includeToolCounts`: Include tool counts (default: true)
- `includeStatus`: Include endpoint status (default: true)

**RefreshEndpointTools** - Refresh endpoint tools
- `endpointName`: Endpoint to refresh (optional, refreshes all if not specified)
- `forceRefresh`: Force refresh even if schema unchanged (default: false)

**UnregisterEndpoint** - Remove endpoint
- `endpointName`: Endpoint name to remove
- `cleanupTools`: Remove associated tools (default: true)

### CombinedOperationsTools

**GraphqlServiceManager** - Primary GraphQL operations interface
- `endpoint`: Endpoint name or URL
- `action`: "get_all_info", "get_schema", "list_queries", "execute_query", "get_capabilities" (default: "get_all_info")
- `query`: GraphQL query (required for execute_query)
- `variables`: Query variables as JSON (optional)
- `includeMutations`: Include mutations (default: false)

**ExecuteMultipleOperations** - Execute multiple operations
- `operations`: Array of operations as JSON
- `executionMode`: "sequential", "parallel" (default: "sequential")
- `continueOnError`: Continue on failure (default: true)
- `timeoutSeconds`: Per-operation timeout (default: 30)

**CompareAndAnalyzeSchemas** - Schema comparison
- `endpoint1`: First endpoint name
- `endpoint2`: Second endpoint name
- `includeBreakingChanges`: Include breaking changes analysis (default: true)
- `detailLevel`: "basic", "detailed", "comprehensive" (default: "detailed")

### CodeMigrationTools

**ExtractGraphQLFromCSharpCode** - Analyze C# REST code for GraphQL migration
- `csharpCode`: C# code with REST API calls
- `graphqlEndpoint`: Target GraphQL endpoint name
- `includeDataFlowAnalysis`: Include data flow analysis (default: true)
- `analysisMode`: "basic", "detailed", "comprehensive" (default: "detailed")

**GenerateOptimizedGraphQLQueries** - Generate optimized queries from REST endpoints
- `restEndpoints`: JSON array of REST endpoints
- `graphqlEndpoint`: Target GraphQL endpoint name
- `entityRelationships`: Entity relationships as JSON (optional)
- `includeOptimizations`: Include optimization techniques (default: true)

### Additional Specialized Tools

**TestingMockingTools** - Generate mock data
- `endpointName`: Registered endpoint name
- `typeName`: Type name for mock data
- `count`: Number of instances (default: 1)

**BranchNamingTool** - Generate Git branch names
- `changeDescription`: Description of changes
- `changeType`: "feature", "bugfix", "hotfix", "refactor" (default: "feature")
- `includeTicket`: Include ticket reference (default: false)

**AutomaticQueryBuilderTool** - Automatic query building
- `endpointName`: Registered endpoint name
- `requirements`: Data requirements description
- `includeFragments`: Use fragments (default: true)

## Installation

```bash
# Clone and build
git clone <repository-url>
cd graphql-mcp
dotnet restore
dotnet build
dotnet run
```

## Configuration

### Environment Variables
- `GRAPHQL_MCP_PORT`: Server port (default: 8080)
- `GRAPHQL_MCP_HOST`: Server host (default: localhost)

### Authentication
Custom headers support for Bearer tokens, API keys, and authentication headers.

## Tool Consolidation

✅ **COMPLETED** - Reduced from 19+ tool files to 6 core consolidated files (70% reduction) while maintaining 100% functionality.

**Core Files**: SchemaTools, QueryTools, DevelopmentTools, UtilityTools, EndpointManagementTools, CombinedOperationsTools