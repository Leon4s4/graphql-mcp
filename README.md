# GraphQL MCP Server

A comprehensive Model Context Protocol (MCP) server that provides essential GraphQL development tools for debugging, testing, performance monitoring, and optimization. This server implements all the critical tools needed for professional GraphQL development workflows.

## 🚀 Features Overview

This MCP server provides **8 essential GraphQL development tools** that cover every aspect of GraphQL development:

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
# Clone and build the project
git clone <repository-url>
cd graphql-mcp
dotnet build
```

You can also build a Docker image using the provided Dockerfile:

```bash
docker build -t graphql-mcp .
docker run --rm -it graphql-mcp
```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ENDPOINT` | GraphQL endpoint URL | `http://localhost:4000/graphql` |
| `HEADERS` | JSON string of HTTP headers | `{}` |
| `ALLOW_MUTATIONS` | Allow mutation operations | `false` |
| `NAME` | Server name identifier | `mcp-graphql` |
| `SCHEMA` | Path to GraphQL schema file | (optional) |

### Example Usage

```bash
# Basic usage
ENDPOINT=http://localhost:4000/graphql dotnet run

# With authentication headers
HEADERS='{"Authorization":"Bearer your-token"}' ENDPOINT=https://api.example.com/graphql dotnet run

# Enable mutations and provide schema file
ALLOW_MUTATIONS=true SCHEMA=./schema.graphql ENDPOINT=http://localhost:4000/graphql dotnet run
```

## 🛠️ Available Tools

### Core GraphQL Operations
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

1. **Start the server** with your GraphQL endpoint
2. **Connect your MCP client** (Claude Desktop, etc.)
3. **Begin using tools** for immediate GraphQL development enhancement
4. **Monitor and optimize** your GraphQL API with comprehensive analytics

## Development

Tools live in the `Tools` directory and are automatically registered at startup. Contributions are welcome!

This server transforms GraphQL development by providing professional-grade tools for every aspect of the development lifecycle, from initial query testing to production monitoring and optimization.
