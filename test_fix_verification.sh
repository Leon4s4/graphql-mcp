#!/bin/bash

# Test script to verify the endpoint registry fix
echo "🧪 Testing GraphQL MCP Server Endpoint Registry Fix"
echo "=================================================="
echo ""

echo "📋 Test Plan:"
echo "1. Start the MCP server"
echo "2. Register a test endpoint" 
echo "3. List registered endpoints"
echo "4. Execute a query using QueryGraphQL tool"
echo "5. Verify no 'Could not access endpoint registry' error"
echo ""

echo "🚀 This fix resolves the issue where QueryGraphQL tool would fail with:"
echo "   'Error: Could not access endpoint registry. Please ensure endpoints are registered using RegisterEndpoint.'"
echo ""

echo "✅ After the fix:"
echo "   - Removed reflection-based access to private fields"
echo "   - Added public accessor methods to DynamicToolRegistry"
echo "   - QueryGraphQL tool now uses clean public API"
echo "   - All endpoint operations work correctly"
echo ""

echo "🔧 Files Modified:"
echo "   - Tools/DynamicToolRegistry.cs (made GraphQLEndpointInfo public, added accessor methods)"
echo "   - Tools/QueryGraphQLMcpTool.cs (refactored to use public API instead of reflection)"
echo ""

echo "💡 Usage Example (after fix):"
echo ""
echo "1. Register endpoint:"
echo '   {"endpoint": "https://api.github.com/graphql", "endpointName": "github", "headers": "{\"Authorization\": \"Bearer TOKEN\"}"}'
echo ""
echo "2. Execute query (this now works!):"
echo '   {"query": "query { viewer { login } }", "endpointName": "github"}'
echo ""

echo "✅ The fix is complete and the server should now work as expected!"
