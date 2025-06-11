#!/bin/bash

# Demo script for the new multi-endpoint GraphQL MCP Server
# This script demonstrates the dynamic endpoint registration workflow

echo "🚀 GraphQL MCP Server - Multi-Endpoint Demo"
echo "============================================"
echo ""

echo "📝 New Workflow Overview:"
echo "1. Start server (no configuration needed)"
echo "2. Register GraphQL endpoints dynamically"
echo "3. Execute queries against any registered endpoint"
echo "4. Use auto-generated tools for specific operations"
echo ""

echo "🔧 Server Configuration Changes:"
echo "• Environment variables are now optional"
echo "• Multiple endpoints can be registered simultaneously"
echo "• Each endpoint has its own authentication and settings"
echo "• Auto-generated tools are created per endpoint"
echo ""

echo "🛠️ Available Tools for Endpoint Management:"
echo "• RegisterEndpoint - Add new GraphQL endpoints"
echo "• QueryGraphQL - Execute queries against registered endpoints" 
echo "• ListDynamicTools - View all endpoints and generated tools"
echo "• ExecuteDynamicOperation - Use auto-generated operation tools"
echo "• RefreshEndpointTools - Update tools when schemas change"
echo "• UnregisterEndpoint - Remove endpoints and cleanup tools"
echo ""

echo "📖 Example Usage:"
echo ""
echo "1. Register a GitHub API endpoint:"
echo '{
  "endpoint": "https://api.github.com/graphql",
  "endpointName": "github", 
  "headers": "{\"Authorization\": \"Bearer YOUR_TOKEN\"}",
  "allowMutations": false,
  "toolPrefix": "gh"
}'
echo ""

echo "2. Register a local development API:"
echo '{
  "endpoint": "http://localhost:4000/graphql",
  "endpointName": "local_api",
  "allowMutations": true,
  "toolPrefix": "dev"
}'
echo ""

echo "3. Execute a custom query:"
echo '{
  "query": "query { viewer { login name } }",
  "endpointName": "github"
}'
echo ""

echo "4. Use auto-generated tools:"
echo "After registration, tools like 'gh_query_viewer' and 'dev_mutation_createUser' are automatically available!"
echo ""

echo "📚 For detailed documentation, see:"
echo "• MULTI_ENDPOINT_GUIDE.md - Complete multi-endpoint usage guide"
echo "• DYNAMIC_TOOLS_README.md - Dynamic tool generation details"
echo "• README.md - Updated with new workflow information"
echo ""

echo "🎯 Benefits of the new approach:"
echo "• No pre-configuration required"
echo "• Support for multiple GraphQL APIs simultaneously"
echo "• Flexible authentication per endpoint"
echo "• Automatic tool generation for all operations"
echo "• Runtime endpoint management"
echo ""

echo "✅ Ready to start! Run: dotnet run"
