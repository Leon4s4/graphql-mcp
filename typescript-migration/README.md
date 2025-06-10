# GraphQL MCP Server - TypeScript Implementation

A comprehensive Model Context Protocol (MCP) server that provides essential GraphQL development tools for debugging, testing, performance monitoring, and optimization. This is the TypeScript implementation of the original C# GraphQL MCP server.

## 🚀 Features

This MCP server provides **25+ essential GraphQL development tools** across 8 categories:

- **Real-time Query Testing & Validation**
- **Performance Profiling & Monitoring**
- **Schema Evolution & Breaking Changes Detection**
- **Query Optimization Suggestions**
- **Field Usage Analytics**
- **Error Context & Debugging**
- **Security Analysis & DoS Protection**
- **Mock Data Generation & Testing**

## 📦 Installation

```bash
# Install dependencies
npm install

# Copy environment file
cp .env.example .env

# Build the project
npm run build
```

## 🔧 Configuration

Set the following environment variables in your `.env` file:

```bash
ENDPOINT=http://localhost:4000/graphql
HEADERS={}
ALLOW_MUTATIONS=false
NAME=mcp-graphql
SCHEMA=
NODE_ENV=development
```

## 🚀 Development

```bash
# Start in development mode with hot reload
npm run dev

# Build for production
npm run build

# Start production server
npm start

# Run tests
npm test

# Lint code
npm run lint
```

## 🛠️ Available Tools

### Core GraphQL Operations
- `query_queryGraphql` - Execute GraphQL queries and mutations
- `schema_introspectSchema` - Get complete schema information
- `schema_getTypeInfo` - Get specific type information

### Security & Analysis
- `security_analyzeQuerySecurity` - Comprehensive security analysis
- `security_detectDoSPatterns` - DoS attack pattern detection

### Utilities
- `utility_formatQuery` - Format and prettify queries
- `utility_minifyQuery` - Minify queries for production
- `utility_extractVariables` - Extract hardcoded values to variables
- `utility_generateAliases` - Generate field aliases

### Dynamic Tool Registry
- `dynamic_registerEndpoint` - Register GraphQL endpoints for auto-tool generation
- `dynamic_listDynamicTools` - List all generated tools
- `dynamic_executeDynamicOperation` - Execute generated operations
- `dynamic_refreshEndpointTools` - Refresh tools when schema changes
- `dynamic_unregisterEndpoint` - Remove endpoint and tools

## 🎯 Usage with MCP Clients

### Claude Desktop Configuration

Add to your Claude Desktop MCP settings:

```json
{
  "mcpServers": {
    "graphql-mcp": {
      "command": "node",
      "args": ["/path/to/graphql-mcp/dist/index.js"],
      "env": {
        "ENDPOINT": "http://localhost:4000/graphql",
        "HEADERS": "{}",
        "ALLOW_MUTATIONS": "false"
      }
    }
  }
}
```

### VS Code MCP Configuration

Update your VS Code settings.json:

```json
{
  "mcp": {
    "servers": {
      "graphql-mcp": {
        "type": "stdio",
        "command": "node",
        "args": ["/path/to/graphql-mcp/dist/index.js"],
        "env": {
          "ENDPOINT": "http://localhost:4000/graphql",
          "HEADERS": "{}",
          "ALLOW_MUTATIONS": "false"
        }
      }
    }
  }
}
```

## 🔄 Migration from C#

This TypeScript implementation maintains feature parity with the original C# version:

### Architecture Changes
- **MCP SDK**: Uses `@modelcontextprotocol/sdk` instead of `ModelContextProtocol`
- **HTTP Client**: Uses `axios` instead of `HttpClient`
- **JSON Processing**: Uses native JSON instead of `System.Text.Json`
- **Module System**: Uses ES modules instead of C# namespaces

### Tool Compatibility
- All 25+ tools from the C# version are implemented
- Same tool names and parameters
- Compatible responses and error handling
- Preserved dynamic tool generation functionality

### Performance Improvements
- Faster startup time with Node.js
- Better memory usage with V8 engine
- Native JSON processing
- Asynchronous I/O by default

## 🧪 Testing

```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with coverage
npm test -- --coverage
```

## 🚢 Deployment

### Local Development
```bash
npm run dev
```

### Production Build
```bash
npm run build
npm start
```

### Docker Deployment
```bash
# Build Docker image
docker build -t graphql-mcp-ts .

# Run container
docker run -p 3000:3000 \
  -e ENDPOINT=http://localhost:4000/graphql \
  -e ALLOW_MUTATIONS=false \
  graphql-mcp-ts
```

## 📝 Dependencies

### Core Dependencies
- `@modelcontextprotocol/sdk` - MCP protocol implementation
- `graphql` - GraphQL core library
- `graphql-request` - GraphQL client
- `axios` - HTTP client
- `typescript` - TypeScript compiler

### GraphQL Analysis
- `graphql-depth-limit` - Query depth analysis
- `graphql-query-complexity` - Complexity analysis
- `graphql-tools` - Schema utilities
- `graphql-tag` - Query parsing

### Development
- `ts-node` - TypeScript execution
- `nodemon` - Hot reload
- `jest` - Testing framework
- `eslint` - Code linting

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Run `npm run lint` and `npm test`
6. Submit a pull request

## 📄 License

MIT License - see LICENSE file for details.

## 🔗 Related Projects

- [Original C# Implementation](../README.md)
- [MCP SDK Documentation](https://modelcontextprotocol.io/)
- [GraphQL Specification](https://graphql.org/)
