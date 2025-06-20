# GraphQL MCP Server

A comprehensive Model Context Protocol (MCP) server that provides powerful GraphQL introspection, querying, and development tools. This server enables LLMs to work effectively with GraphQL APIs through a consolidated set of intelligent tools.

## Overview

The GraphQL MCP Server offers a complete GraphQL development experience with consolidated tools that follow MCP best practices. The server has been optimized to reduce the number of exposed tools while maintaining all essential functionality, providing unified interfaces that combine multiple operations into single tool calls.

## Key Features

- **Unified Tool Interface**: Consolidated tools that combine multiple operations to reduce round trips
- **Dynamic Tool Generation**: Automatically generates MCP tools from GraphQL schema introspection
- **Multi-Endpoint Support**: Manage and query multiple GraphQL endpoints simultaneously
- **Comprehensive Analysis**: Query analysis, performance monitoring, and optimization recommendations
- **Development Support**: Debugging, testing, mock data generation, and code generation
- **Schema Management**: Schema introspection, comparison, and evolution tracking
- **Workflow Orchestration**: Complete workflows for exploration, querying, development, and optimization

## Installation

```bash
# Clone the repository
git clone <repository-url>
cd graphql-mcp

# Install dependencies
dotnet restore

# Build the project
dotnet build

# Run the server
dotnet run
```

## Core Tools

### 1. CombinedOperationsTools

The **primary interface** for GraphQL operations, designed to minimize round trips and provide comprehensive functionality.

#### `GraphqlServiceManager`
**The main tool for all GraphQL operations** - combines multiple operations in a single call.

**Actions Available:**
- `get_all_info` (default): Complete service information including schema, queries, and capabilities
- `get_schema`: Detailed schema information with types, fields, and descriptions
- `list_queries`: Available operations with signatures and examples
- `execute_query`: Execute GraphQL queries with optional variables
- `get_capabilities`: Service capabilities, limits, and supported features

**Parameters:**
- `endpoint`: GraphQL endpoint name or URL
- `action`: Action to perform (default: "get_all_info")
- `query`: GraphQL query string (required for execute_query)
- `variables`: Query variables as JSON object (optional)
- `includeMutations`: Include mutation operations (default: false)
- `maxDepth`: Maximum depth for schema introspection (default: 3)

**Example Usage:**
```json
{
  "endpoint": "github-api",
  "action": "get_all_info",
  "includeMutations": false,
  "maxDepth": 3
}
```

#### `ExecuteMultipleOperations`
Execute multiple GraphQL operations in sequence or parallel within a single tool call.

**Features:**
- Sequential execution with result chaining
- Parallel execution for better performance
- Error handling and continuation options
- Support for multiple endpoints

**Parameters:**
- `operations`: Array of operations as JSON (endpoint, query, variables, name)
- `executionMode`: "sequential" or "parallel" (default: "sequential")
- `continueOnError`: Continue on failure (default: true)
- `timeoutSeconds`: Per-operation timeout (default: 30)

#### `CompareAndAnalyzeSchemas`
Comprehensive schema comparison and analysis between different endpoints.

**Features:**
- Schema differences detection
- Breaking changes identification
- Evolution tracking
- Compatibility analysis

#### `CompleteGraphQLWorkflow`
**The PRIMARY tool for GraphQL discovery and execution** - combines multiple workflow steps.

**Workflow Types:**
- `explore`: Discover schema, types, and available operations
- `query`: Execute queries with intelligent analysis
- `develop`: Full development workflow with debugging and testing
- `optimize`: Performance analysis and optimization recommendations

**Parameters:**
- `endpoint`: GraphQL endpoint name or URL
- `workflow`: Workflow type (default: "explore")
- `query`: GraphQL query (required for 'query' and 'optimize' workflows)
- `variables`: Query variables as JSON (optional)
- `includeAnalysis`: Include comprehensive analysis (default: true)
- `includeExamples`: Include examples and documentation (default: true)

#### `ExecuteMultiEndpointWorkflow`
Advanced multi-endpoint orchestration for distributed GraphQL systems.

**Workflow Types:**
- `parallel`: Execute operations simultaneously across all endpoints
- `sequential`: Execute with dependency management and data passing
- `aggregate`: Collect, correlate, and merge data from multiple sources
- `compare`: Compare responses and schemas across endpoints

### 2. SchemaTools

Consolidated schema operations providing unified schema introspection and analysis.

#### `IntrospectSchema`
Comprehensive GraphQL schema introspection with multiple output formats.

**Features:**
- Detailed schema information
- Type definitions and relationships
- Operation discovery
- Multiple output formats

**Parameters:**
- `endpointName`: Name of registered endpoint
- `format`: Output format - "detailed", "operations", "types", "sdl" (default: "detailed")
- `includeMutations`: Include mutations (default: true)
- `includeSubscriptions`: Include subscriptions (default: false)
- `typeName`: Specific type to introspect (optional)
- `maxDepth`: Maximum introspection depth (default: 3)

#### `CompareSchemas`
Compare schemas between different endpoints for evolution analysis.

#### `GetTypeDefinition`
Get detailed information about specific GraphQL types.

#### `ListOperations`
List all available queries and mutations with their signatures.

#### `SearchSchema`
Search schema types and fields using patterns.

### 3. QueryTools

Consolidated query analysis, validation, and performance monitoring.

#### `AnalyzeQuery`
Comprehensive query analysis including validation, performance, and security assessment.

**Analysis Levels:**
- `basic`: Syntax validation and basic checks
- `standard`: Includes performance analysis
- `comprehensive`: Full analysis with security and optimization

**Features:**
- Syntax validation
- Performance assessment
- Security analysis
- Optimization suggestions

#### `ExecuteQuery`
Execute GraphQL queries with detailed metrics and analysis.

#### `BuildQuery`
Intelligent query construction from schema information.

#### `OptimizeQuery`
Query formatting and optimization with improvement suggestions.

### 4. DevelopmentTools

Consolidated development tools for comprehensive GraphQL development support.

#### `DebugGraphQL`
Comprehensive debugging and troubleshooting tool.

**Debug Types:**
- `query`: Debug query syntax, structure, and execution
- `schema`: Debug schema issues and compatibility
- `response`: Debug response errors and validation
- `performance`: Debug performance and optimization issues

**Features:**
- Query debugging and error analysis
- Response validation and troubleshooting
- Schema validation and compatibility checking
- Performance debugging and optimization

#### `GenerateTestData`
Generate comprehensive test data and mock responses.

**Features:**
- Schema-based mock data generation
- Realistic test scenarios
- Edge case generation
- Multiple output formats (json, query, variables)

#### `ExplainError`
Explain GraphQL errors with detailed analysis and resolution guidance.

**Features:**
- Error categorization and severity assessment
- Root cause analysis
- Step-by-step resolution guidance
- Prevention strategies

#### `CreateTestSuite`
Create comprehensive testing suites and validation scenarios.

**Test Types:**
- `functional`: Functional testing scenarios
- `performance`: Performance testing patterns
- `security`: Security testing scenarios
- `integration`: Integration testing helpers
- `regression`: Regression testing data

### 5. UtilityTools

Enhanced utility tools providing comprehensive GraphQL utilities and code generation.

#### `FormatQuery`
Format GraphQL queries with proper indentation and structure.

#### `MinifyQuery`
Compress GraphQL queries for production use.

#### `ExtractVariables`
Convert hardcoded values in queries to variables for reusability.

#### `GenerateAliases`
Create field aliases to prevent naming conflicts.

#### `GenerateCode`
Generate client code from GraphQL schemas for multiple programming languages.

**Supported Languages:**
- TypeScript: Full type definitions and client code
- JavaScript: Client code with JSDoc annotations
- Python: Client classes and type hints
- C#: Client classes and DTOs
- Java: Client interfaces and POJOs

#### `UtilityOperationsComprehensive`
Advanced utility suite with intelligent formatting, optimization, and transformation.

### 6. EndpointManagementTools

Tools for managing GraphQL endpoint registration and configuration.

### 7. CodeMigrationTools

**NEW**: Tools for migrating from REST API code to GraphQL queries.

#### `ExtractGraphQLFromCSharpCode`
**Analyze C# code that makes REST API calls and generate equivalent GraphQL queries.**

This powerful migration tool helps transition from REST to GraphQL by:
- Extracting REST API calls from C# code
- Analyzing data aggregation patterns  
- Identifying entity relationships and dependencies
- Generating equivalent GraphQL queries that combine multiple REST calls
- Providing migration recommendations and optimizations

**Supported C# Patterns:**
- HttpClient REST calls (GET, POST, PUT, DELETE)
- Multiple API calls with data aggregation
- Async/await patterns
- LINQ operations on API responses
- Entity mapping and transformation
- Conditional API calls based on previous responses

**Parameters:**
- `csharpCode`: C# code containing REST API calls to analyze
- `graphqlEndpoint`: Target GraphQL endpoint name (must be registered)
- `includeDataFlowAnalysis`: Include detailed analysis of data flows (default: true)
- `includeOptimizations`: Generate optimized GraphQL queries (default: true)
- `includeMigrationGuide`: Include migration recommendations (default: true)
- `analysisMode`: Analysis depth - "basic", "detailed", "comprehensive" (default: "detailed")

**Example Usage:**
```json
{
  "csharpCode": "var user = await httpClient.GetAsync(\"/api/users/{id}\");\nvar posts = await httpClient.GetAsync(\"/api/users/{id}/posts\");",
  "graphqlEndpoint": "my-api",
  "analysisMode": "comprehensive"
}
```

#### `GenerateOptimizedGraphQLQueries`
**Generate optimized GraphQL queries to replace multiple REST API calls.**

Creates equivalent GraphQL queries that:
- Combine multiple REST calls into single GraphQL operations
- Optimize data fetching with precise field selection
- Implement proper pagination and filtering
- Use fragments for reusable field sets
- Include variables for dynamic queries

**Parameters:**
- `restEndpoints`: JSON array of REST endpoints to replace
- `graphqlEndpoint`: Target GraphQL endpoint name
- `entityRelationships`: Entity relationships as JSON (optional)
- `includeOptimizations`: Include optimization techniques (default: true)
- `includeVariations`: Generate queries for different use cases (default: true)

**Example REST Endpoints Format:**
```json
[
  {
    "method": "GET",
    "endpoint": "/api/users/{id}",
    "purpose": "Get user details"
  },
  {
    "method": "GET", 
    "endpoint": "/api/users/{id}/posts",
    "purpose": "Get user posts"
  }
]
```

### 6. EndpointManagementTools

Tools for managing GraphQL endpoint registration and configuration.

#### `RegisterEndpoint`
Register a GraphQL endpoint and automatically generate MCP tools.

**Features:**
- Schema discovery and tool generation
- Authentication support (custom headers)
- Mutation support configuration
- Tool prefix customization

#### `GetAllEndpoints`
View all registered endpoints with their configuration and tool counts.

#### `RefreshEndpointTools`
Update dynamic tools by re-introspecting GraphQL schemas.

#### `UnregisterEndpoint`
Remove endpoints and clean up associated tools.

#### `RegisterEndpointWithAnalysis`
Register endpoints with optional analysis and monitoring.

## Workflow Examples

### Complete GraphQL Exploration
```json
{
  "tool": "CompleteGraphQLWorkflow",
  "endpoint": "https://api.github.com/graphql",
  "workflow": "explore",
  "includeAnalysis": true,
  "includeExamples": true
}
```

### Query Execution with Analysis
```json
{
  "tool": "CompleteGraphQLWorkflow", 
  "endpoint": "github-api",
  "workflow": "query",
  "query": "query { viewer { login name } }",
  "includeAnalysis": true
}
```

### Multi-Endpoint Data Aggregation
```json
{
  "tool": "ExecuteMultiEndpointWorkflow",
  "workflowType": "aggregate",
  "endpoints": "[\"api1\", \"api2\", \"api3\"]",
  "baseQuery": "query { users { id name } }",
  "includeDataAnalysis": true
}
```

### Development Workflow
```json
{
  "tool": "CompleteGraphQLWorkflow",
  "endpoint": "local-api",
  "workflow": "develop",
  "query": "query { getUser(id: \"123\") { name email } }",
  "includeExamples": true
}
```

### REST to GraphQL Migration
```json
{
  "tool": "ExtractGraphQLFromCSharpCode",
  "csharpCode": "var user = await httpClient.GetAsync(\"/api/users/{id}\");\nvar posts = await httpClient.GetAsync(\"/api/users/{id}/posts\");\nvar comments = await httpClient.GetAsync(\"/api/posts/{postId}/comments\");",
  "graphqlEndpoint": "my-api",
  "analysisMode": "comprehensive",
  "includeMigrationGuide": true
}
```

### Optimized Query Generation
```json
{
  "tool": "GenerateOptimizedGraphQLQueries",
  "restEndpoints": "[{\"method\": \"GET\", \"endpoint\": \"/api/users/{id}\", \"purpose\": \"Get user\"}, {\"method\": \"GET\", \"endpoint\": \"/api/users/{id}/posts\", \"purpose\": \"Get posts\"}]",
  "graphqlEndpoint": "my-api",
  "entityRelationships": "{\"User\": [\"posts\", \"comments\"], \"Post\": [\"author\"]}",
  "includeOptimizations": true,
  "includeVariations": true
}
```

## Predefined MCP Prompts

The GraphQL MCP Server includes predefined prompts that provide structured guidance for common GraphQL tasks. These prompts automatically include the correct tool parameters and best practices.

### Available Predefined Prompts

## Migration and REST-to-GraphQL Prompts

#### `MigrateCSharpToGraphQL`
**Comprehensive C# to GraphQL migration with analysis**
- **Parameters**: `csharpCode`, `graphqlEndpoint`, `analysisLevel`
- Analyzes C# REST API code patterns (HttpClient, async/await, LINQ)
- Generates equivalent GraphQL queries that combine multiple REST calls
- Provides migration guidance with performance benefits analysis
- Includes before/after code comparisons and step-by-step migration guide

#### `ConvertRestToGraphQL`
**Convert REST API endpoints to optimized GraphQL queries**
- **Parameters**: `restEndpoints`, `graphqlEndpoint`, `entityRelationships`
- Takes JSON array of REST endpoints with method, endpoint, and purpose
- Creates optimized GraphQL queries with fragments and variables
- Implements pagination, field selection, and query variations
- Generates list, search, and minimal query variations

#### `PlanGraphQLMigration`
**Strategic migration planning from REST to GraphQL**
- **Parameters**: `currentArchitecture`, `migrationGoals`, `constraints`, `riskTolerance`
- Creates comprehensive phased migration roadmaps
- Provides risk assessment and mitigation strategies
- Includes team training recommendations and resource planning
- Supports conservative, moderate, and aggressive migration approaches

#### `SetupGraphQLWorkflow`
**Comprehensive GraphQL development workflow setup**
- **Parameters**: `endpoint`, `developmentFocus`, `experienceLevel`
- Configures complete development environment
- Sets up testing, debugging, and performance monitoring procedures
- Provides team-specific guidance (beginner, intermediate, advanced)
- Includes tool configuration and best practices implementation

## Core GraphQL Development Prompts

#### `GenerateQuery`
**Optimized GraphQL query generation based on requirements**
- **Parameters**: `dataRequirement`, `endpointName`, `performance`, `includeRelated`
- Creates efficient queries with proper structure and optimization
- Includes fragments for reusable field sets and proper variable definitions
- Optimizes for performance levels (fast, normal, comprehensive)
- Provides usage examples and query explanation

#### `AnalyzeSchema`
**Comprehensive schema analysis with multiple focus areas**
- **Parameters**: `focus`, `endpointName`, `specificType`
- **Focus Areas**: structure, performance, security, evolution
- Provides actionable recommendations and identifies potential issues
- Includes implementation guidance and priority ranking
- Covers type relationships, naming conventions, and optimization opportunities

#### `DebuggingAssistant`
**Context-aware GraphQL troubleshooting guidance**
- **Parameters**: `issueType`, `severity`, `environment`
- **Issue Types**: query-error, performance, schema, resolver
- Provides systematic debugging approach with root cause analysis
- Environment-specific recommendations (development, staging, production)
- Includes resolution steps and prevention strategies

#### `TestingStrategy`
**Comprehensive testing strategies for GraphQL APIs**
- **Parameters**: `testingType`, `operationType`, `framework`
- **Testing Types**: unit, integration, performance, security
- Framework-specific implementations (jest, vitest, xunit, pytest)
- Generates test cases with examples and CI/CD integration guidance
- Includes mocking strategies and assertion recommendations

#### `SchemaDesign`
**GraphQL schema architecture and design guidance**
- **Parameters**: `designFocus`, `domain`, `scale`
- **Design Focus**: new-schema, refactoring, migration, optimization
- **Domains**: e-commerce, social, api-gateway, microservices
- Scale-appropriate recommendations (small, medium, large, enterprise)
- Includes best practices, patterns, and future considerations

## Workflow and Development Process Prompts

#### `DevelopmentWorkflow`
**GraphQL best practices for development workflows**
- **Parameters**: `phase`, `teamSize`, `techStack`
- **Phases**: planning, implementation, testing, deployment
- **Tech Stacks**: node, dotnet, python, java with specific recommendations
- Team size considerations (solo, small, medium, large)
- Includes quality gates, tools, and success metrics

#### `DocumentationGuide`
**Generate comprehensive GraphQL API documentation**
- **Parameters**: `docType`, `audience`, `format`
- **Doc Types**: api-reference, getting-started, best-practices, migration
- **Audiences**: developers, qa, devops, business
- **Formats**: markdown, interactive, video-script
- Audience-specific content and interactive features

#### `TrainingMaterial`
**Create GraphQL learning and training materials**
- **Parameters**: `level`, `format`, `focus`, `duration`
- **Levels**: beginner, intermediate, advanced, expert
- **Formats**: workshop, course, bootcamp, self-paced
- **Focus Areas**: concepts, implementation, optimization, architecture
- Includes learning objectives, hands-on exercises, and assessment strategies

## MCP Server Management Prompts

#### `EndpointManagementGuide`
**Guidance for managing GraphQL endpoints in the MCP server**
- **Parameters**: `detailLevel`
- Explains dynamic endpoint registration and tool generation
- Covers typical workflow for multi-endpoint management
- Includes best practices for endpoint lifecycle management

#### `ToolsOverview`
**Overview of available MCP server tools and capabilities**
- **Parameters**: `focus`
- **Focus Areas**: all, query, schema, performance, security
- Comprehensive overview of available tools by category
- Helps discover the right tool for specific tasks

### Using Predefined Prompts

Predefined prompts automatically structure your requests with the right parameters and guidance:

```
Use the MigrateCSharpToGraphQL prompt with:
- C# code: [your REST API code]
- GraphQL endpoint: "my-api"
- Analysis level: "comprehensive"
```

The prompt will automatically:
- Include all necessary analysis parameters
- Provide structured guidance for the migration
- Ensure you get comprehensive results
- Include tool-specific recommendations

## Migration Tool Prompts

### Using ExtractGraphQLFromCSharpCode

Here are practical prompts to get the most out of the C# to GraphQL migration tool:

#### **Basic Migration Analysis**
```
I have C# code that makes multiple REST API calls to fetch user data. Can you analyze this code and suggest equivalent GraphQL queries?

[Paste your C# code here]

Target GraphQL endpoint: "my-api"
```

#### **Comprehensive Migration with Performance Analysis**
```
Please perform a comprehensive analysis of my C# REST API code. I want to:
1. Extract all REST calls and understand the data flow
2. Generate optimized GraphQL queries that combine multiple calls
3. Get a detailed migration guide with code examples
4. Understand the performance benefits of switching to GraphQL

C# Code:
[Your code here]

GraphQL endpoint: "production-api"
Analysis mode: "comprehensive"
```

#### **Data Aggregation Pattern Analysis**
```
I have C# code that fetches data from multiple endpoints and aggregates it into objects. Can you:
- Identify the aggregation patterns
- Show how GraphQL can replace multiple REST calls with single queries
- Provide migration recommendations

Focus on: data flow analysis and optimization opportunities

[Your C# code]
```

#### **Entity Relationship Migration**
```
My C# application makes REST calls to get related entities (users → posts → comments). Can you:
1. Analyze the entity relationships in my code
2. Generate GraphQL queries that efficiently fetch related data
3. Show the performance improvements from reducing N+1 queries

C# Code: [paste code]
GraphQL endpoint: "content-api"
```

### Using GenerateOptimizedGraphQLQueries

#### **REST Endpoint Replacement**
```
I want to replace these REST endpoints with optimized GraphQL queries:

REST Endpoints:
- GET /api/users/{id} - Get user details
- GET /api/users/{id}/posts - Get user's posts  
- GET /api/posts/{id}/comments - Get post comments
- GET /api/users/{id}/followers - Get user followers

Please generate GraphQL queries that:
1. Combine these calls efficiently
2. Use fragments for reusable field sets
3. Include pagination and filtering
4. Provide query variations for different use cases

Target endpoint: "social-api"
```

#### **E-commerce API Migration**
```
Help me migrate our e-commerce REST API to GraphQL. Generate optimized queries for:

REST Endpoints:
- GET /api/products/{id} - Product details
- GET /api/products/{id}/reviews - Product reviews
- GET /api/products/{id}/related - Related products
- GET /api/categories/{id}/products - Category products
- GET /api/users/{id}/cart - User shopping cart
- GET /api/users/{id}/orders - User order history

Entity Relationships:
- Product: [reviews, related, category]
- User: [cart, orders, reviews]
- Category: [products, subcategories]

Include optimization techniques and performance comparisons.
```

#### **Microservices Consolidation**
```
I have multiple microservices with REST APIs that I want to consolidate through GraphQL:

Service 1 - User Service:
- GET /users/{id}
- GET /users/{id}/profile
- GET /users/{id}/preferences

Service 2 - Content Service:  
- GET /posts/{userId}
- GET /posts/{id}/details
- GET /categories

Service 3 - Analytics Service:
- GET /users/{id}/stats
- GET /posts/{id}/metrics

Generate unified GraphQL queries that can replace these distributed REST calls. Include data aggregation patterns and cross-service query optimization.
```

### Advanced Migration Scenarios

#### **Legacy System Migration**
```
I'm migrating a legacy .NET application with complex REST API patterns. Can you:

1. Analyze this C# code for REST API usage patterns
2. Identify opportunities for GraphQL optimization
3. Generate a phased migration plan
4. Provide performance impact analysis

Focus on: reducing network overhead, improving data fetching efficiency, and maintaining backwards compatibility during migration.

[Large C# codebase excerpt]
```

#### **Performance Optimization Focus**
```
My application has performance issues due to multiple REST API calls. Analyze this code and:

1. Calculate current network overhead
2. Show how GraphQL can reduce round trips
3. Generate optimized queries with field selection
4. Provide performance benchmarking data

Specific concerns:
- N+1 query problems
- Over-fetching data
- Multiple sequential API calls
- Mobile app performance

[C# code with performance issues]
```

#### **API Documentation Migration**
```
I have REST API documentation and want to understand how it would translate to GraphQL:

REST API Endpoints: [JSON array of endpoints]

Please generate:
1. Equivalent GraphQL schema suggestions
2. Optimized query patterns
3. Migration complexity assessment
4. Query variations for different client needs

Include fragments, variables, and best practices for the generated queries.
```

### Migration Planning Prompts

#### **Assessment and Planning**
```
Before starting my REST to GraphQL migration, I need to understand:

1. What REST patterns in my C# code are good candidates for GraphQL?
2. Which parts of my API would benefit most from migration?
3. What's the estimated effort and complexity?
4. How should I phase the migration?

Please analyze my codebase and provide a migration assessment:
[C# code or REST endpoint list]
```

#### **Team Training and Documentation**
```
I need to train my development team on GraphQL migration. Can you:

1. Analyze our current REST patterns
2. Show before/after examples with our actual code
3. Generate training materials with practical examples
4. Provide best practices specific to our use cases

Our current C# patterns: [code examples]
Target GraphQL endpoint: "training-api"
```

## Tool Consolidation Benefits

The GraphQL MCP Server has been optimized through comprehensive tool consolidation:

- **Reduced Complexity**: From 19 tool files with 60+ tools to 7 core files with 35-40 tools
- **Unified Interfaces**: Related operations grouped with optional parameters
- **Better Performance**: Reduced tool registration overhead and faster discovery
- **Improved UX**: Clear tool purposes and comprehensive functionality
- **MCP Best Practices**: Follows MCP guidelines for reducing round trips

## Configuration

### Environment Variables
- `GRAPHQL_MCP_PORT`: Server port (default: 8080)
- `GRAPHQL_MCP_HOST`: Server host (default: localhost)

### Authentication
Support for various authentication methods through custom headers:
- Bearer tokens
- API keys
- Custom authentication headers

## Error Handling

The server provides comprehensive error handling with:
- Detailed error messages and categorization
- Resolution guidance and suggestions
- Prevention strategies
- Debugging assistance

## Performance Optimization

Built-in performance features:
- Query complexity analysis
- Performance monitoring and metrics
- Optimization recommendations
- Caching strategies

## Security

Security features include:
- Query complexity limits
- Authentication header support
- Security analysis and recommendations
- Error sanitization

## Contributing

Contributions are welcome! Please ensure:
- Follow existing code patterns
- Add comprehensive documentation
- Include tests for new functionality
- Follow MCP best practices

## License

[License information here]

## Support

For issues and feature requests, please use the GitHub issue tracker.