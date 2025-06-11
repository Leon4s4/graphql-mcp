# GraphQL MCP Server

A comprehensive Model Context Protocol (MCP) server that provides essential GraphQL development tools for debugging, testing, performance monitoring, and optimization. This server implements all the critical tools needed for professional GraphQL development workflows with **dynamic multi-endpoint support**.

## 🚀 Features Overview

This MCP server provides **dynamic GraphQL endpoint management** and **25+ essential development tools** that cover every aspect of GraphQL development:

### 1. **Real-time Query Testing & Validation**
- **Immediate feedback** on syntax and logic errors
- **Smart validation** with context-aware suggestions
- **Query structure analysis** and recommendations

### 2. **Performance Profiling & Monitoring**
- **Execution time tracking** for queries and resolvers
- **N+1 query detection** and optimization suggestions
- **DataLoader pattern analysis** for efficient data fetching
- **Performance bottleneck identification**

### 3. **Schema Evolution & Breaking Changes Detection**
- **Schema comparison** between versions
- **Breaking change detection** with detailed reports
- **Field deprecation tracking** and migration guidance
- **Safe evolution recommendations**

### 4. **Query Optimization Suggestions**
- **Complexity analysis** and scoring
- **Query restructuring recommendations**
- **Field selection optimization**
- **Performance improvement suggestions**

### 5. **Field Usage Analytics**
- **Usage pattern tracking** across your API
- **Dead code identification** for cleanup opportunities
- **Popular field analysis** for optimization priorities
- **Query frequency statistics**

### 6. **Error Context & Debugging**
- **Enhanced error messages** with actionable suggestions
- **Root cause analysis** for common GraphQL issues
- **Debugging context** with field resolution paths
- **Solution recommendations** for error resolution

### 7. **Security Analysis & DoS Protection**
- **Query complexity analysis** to prevent DoS attacks
- **Depth limiting** and query cost analysis
- **Malicious pattern detection**
- **Security recommendation engine**

### 8. **Mock Data Generation & Testing**
- **Intelligent mock data** based on schema types
- **Test case generation** for comprehensive coverage
- **Realistic data patterns** for development and testing
- **Custom mock strategies** for different scenarios

## 📋 Installation

```bash
# Clone the repository
git clone <repository-url>
cd graphql-mcp

# Build and run
dotnet build
dotnet run
```

The server starts immediately without requiring endpoint configuration. Use the `RegisterEndpoint` tool to add GraphQL endpoints dynamically.

You can also build a Docker image using the provided Dockerfile:

```bash
docker build -t graphql-mcp .
docker run --rm -it graphql-mcp
```

## 🔧 Configuration

### **Dynamic Endpoint Registration (Recommended)**

No pre-configuration needed! Register endpoints at runtime:

1. Start the server: `dotnet run`
2. Use `RegisterEndpoint` tool to add GraphQL APIs
3. Begin using tools immediately

### **Optional Environment Variables**

| Variable | Description | Default |
|----------|-------------|---------|
| `NAME` | Server name identifier | `mcp-graphql` |
| `SCHEMA` | Path to GraphQL schema file | (optional) |

### **Legacy Single-Endpoint Mode**

For backward compatibility, you can still use environment variables for a single endpoint:

| Variable | Description | Default |
|----------|-------------|---------|
| `ENDPOINT` | GraphQL endpoint URL | `http://localhost:4000/graphql` |
| `HEADERS` | JSON string of HTTP headers | `{}` |
**Note**: The dynamic registration approach is preferred for new setups as it provides more flexibility.

### Example Usage

**Dynamic Registration (Recommended):**
```bash
# Start server
dotnet run

# Then use RegisterEndpoint tool to add endpoints
# No environment variables needed!
```

**Legacy Single Endpoint:**
```bash
# Basic usage
ENDPOINT=http://localhost:4000/graphql dotnet run

# With authentication headers
HEADERS='{"Authorization":"Bearer your-token"}' ENDPOINT=https://api.example.com/graphql dotnet run
```

## 🛠️ Available Tools

### **Dynamic Endpoint Management**
| Tool | Description |
|------|-------------|
| `RegisterEndpoint` | Register a GraphQL endpoint for automatic tool generation and queries |
| `ListDynamicTools` | View all registered endpoints and their generated tools |
| `RefreshEndpointTools` | Update tools when endpoint schemas change |
| `UnregisterEndpoint` | Remove an endpoint and cleanup its generated tools |

### **Core GraphQL Operations**
| Tool | Description |
|------|-------------|
| `QueryGraphQL` | Execute GraphQL queries and mutations against registered endpoints |
| `ExecuteDynamicOperation` | Execute auto-generated operation-specific tools |
| `introspect_schema` | Get complete schema information including types, fields, and directives |
| `validate_query` | Validate GraphQL query syntax, structure, and schema compliance |
| Tool | Description |
|------|-------------|
| `query_graphql` | Execute GraphQL queries and mutations with full error handling |
| `introspect_schema` | Get complete schema information including types, fields, and directives |
| `validate_query` | Validate GraphQL query syntax, structure, and schema compliance |

### Real-time Testing & Validation
| Tool | Description |
|------|-------------|
| `test_query` | Real-time query testing with immediate feedback and validation |
| `validate_query_syntax` | Advanced syntax validation with error context and suggestions |

### Performance Monitoring & Profiling
| Tool | Description |
|------|-------------|
| `measure_query_performance` | Comprehensive performance profiling with execution time tracking |
| `analyze_dataloader_patterns` | N+1 query detection and DataLoader optimization analysis |
| `get_performance_metrics` | Detailed performance metrics and bottleneck identification |

### Schema Evolution & Management
| Tool | Description |
|------|-------------|
| `compare_schemas` | Schema version comparison with breaking change detection |
| `detect_breaking_changes` | Automated breaking change analysis with migration guidance |
| `track_schema_evolution` | Schema evolution tracking and version management |

### Query Optimization & Analysis
| Tool | Description |
|------|-------------|
| `optimize_query` | Query optimization with complexity analysis and restructuring suggestions |
| `analyze_query_complexity` | Query complexity scoring and optimization recommendations |
| `suggest_query_improvements` | Intelligent suggestions for query performance improvements |

### Field Usage Analytics
| Tool | Description |
|------|-------------|
| `get_field_usage_analytics` | Comprehensive field usage tracking and analytics |
| `identify_unused_fields` | Dead code identification for schema cleanup |
| `analyze_usage_patterns` | Usage pattern analysis for optimization priorities |

### Error Context & Debugging
| Tool | Description |
|------|-------------|
| `explain_graphql_error` | Enhanced error explanation with context and solutions |
| `debug_query_execution` | Step-by-step query execution debugging |
| `get_error_context` | Detailed error context with resolution suggestions |

### Security Analysis
| Tool | Description |
|------|-------------|
| `analyze_query_security` | Comprehensive security analysis and threat detection |
| `detect_dos_patterns` | DoS attack pattern detection and prevention |
| `validate_query_safety` | Query safety validation with security recommendations |

### Testing & Mock Data
| Tool | Description |
|------|-------------|
| `generate_mock_data` | Intelligent mock data generation based on schema types |
| `create_test_cases` | Comprehensive test case generation for GraphQL operations |
| `generate_test_queries` | Automated test query generation for schema coverage |

## 🎯 Development Workflow Integration

This MCP server is designed to integrate seamlessly into your GraphQL development workflow:

1. **During Development**: Use real-time validation and testing tools
2. **Code Review**: Leverage schema evolution and breaking change detection
3. **Performance Optimization**: Utilize profiling and analytics tools
4. **Security Audits**: Apply security analysis and DoS protection tools
5. **Testing**: Generate comprehensive test cases and mock data
6. **Production Monitoring**: Track field usage and performance metrics

## 💡 GraphQL Expert Prompts

In addition to comprehensive tools, this MCP server provides **expert-level prompts** that act as GraphQL consultants and guides:

### **Query & Schema Assistance**
| Prompt | Description |
|--------|-------------|
| `GenerateQuery` | Expert GraphQL query generation with optimization and best practices |
| `AnalyzeSchema` | Comprehensive schema analysis for structure, performance, security, or evolution |
| `SchemaDesign` | Architecture guidance for designing, refactoring, or optimizing GraphQL schemas |

### **Development & Workflow Guidance**
| Prompt | Description |
|--------|-------------|
| `DevelopmentWorkflow` | Best practices for GraphQL development across all phases (planning to deployment) |
| `DebuggingAssistant` | Context-aware troubleshooting for GraphQL errors and performance issues |
| `TestingStrategy` | Comprehensive testing strategies for unit, integration, performance, and security testing |

### **Server & Endpoint Management**
| Prompt | Description |
|--------|-------------|
| `EndpointManagementGuide` | How to register endpoints and manage dynamic tools |
| `ToolsOverview` | Overview of available MCP server tools and capabilities |

### **Learning & Documentation**
| Prompt | Description |
|--------|-------------|
| `DocumentationGuide` | Create API documentation, guides, and migration materials |
| `TrainingMaterial` | Generate training curricula and learning materials for teams |

### **Prompt Features**
- **Parameterized**: Customize guidance based on your specific context (team size, tech stack, domain)
- **Expert-Level**: Advanced GraphQL knowledge and industry best practices
- **Practical**: Actionable advice with implementation examples
- **Comprehensive**: Cover all aspects from basic concepts to enterprise architecture

### **Example Prompt Usage**

```json
// Generate an optimized query
{
  "dataRequirement": "Get user profiles with their recent posts and follower counts",
  "performance": "fast",
  "includeRelated": "yes"
}

// Get development workflow guidance
{
  "phase": "implementation",
  "teamSize": "medium",
  "techStack": "dotnet"
}

// Create testing strategy
{
  "testingType": "performance",
  "operationType": "query",
  "framework": "xunit"
}
```

These prompts transform your MCP client into a GraphQL expert consultant, providing contextual guidance exactly when you need it.

## 🔒 Security Features

- **Query complexity analysis** to prevent resource exhaustion
- **Depth limiting** to avoid deeply nested query attacks
- **Rate limiting recommendations** based on query patterns
- **Malicious pattern detection** for common GraphQL vulnerabilities
- **Security best practices** enforcement and recommendations

## 📊 Analytics & Monitoring

- **Real-time performance metrics** collection and analysis
- **Field usage statistics** for optimization insights
- **Query pattern analysis** for performance improvements
- **Error tracking and categorization** for debugging efficiency
- **Schema evolution metrics** for change impact assessment

## 🚦 Getting Started

### **Quick Start (Dynamic Endpoints)**

1. **Start the server**
   ```bash
   dotnet run
   ```

2. **Connect your MCP client** (Claude Desktop, etc.)

3. **Register your first GraphQL endpoint**
   Use the `RegisterEndpoint` tool:
   ```json
   {
     "endpoint": "https://api.github.com/graphql",
     "endpointName": "github",
     "headers": "{\"Authorization\": \"Bearer YOUR_TOKEN\"}",
     "allowMutations": false,
     "toolPrefix": "gh"
   }
   ```

4. **Start using tools**
   - Use `QueryGraphQL` for custom queries
   - Use auto-generated tools (e.g., `gh_query_viewer`)
   - Use `ListDynamicTools` to see all available tools

### **Example Workflow**

```json
// 1. Register an endpoint
{
  "endpoint": "http://localhost:4000/graphql",
  "endpointName": "local_api",
  "allowMutations": true,
  "toolPrefix": "local"
}

// 2. Execute a query
{
  "query": "query { users { id name email } }",
  "endpointName": "local_api"
}

// 3. Use generated tools
// After registration, tools like local_query_users are automatically available
```

See [MULTI_ENDPOINT_GUIDE.md](MULTI_ENDPOINT_GUIDE.md) for detailed multi-endpoint usage examples.

## Development

Tools live in the `Tools` directory and are automatically registered at startup. Contributions are welcome!

This server transforms GraphQL development by providing professional-grade tools for every aspect of the development lifecycle, from initial query testing to production monitoring and optimization.
