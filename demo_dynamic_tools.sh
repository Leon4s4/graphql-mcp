#!/bin/bash

# Demo script for testing Dynamic Tool Registry
# This script demonstrates the automatic API-to-tools mapping functionality

echo "🚀 GraphQL MCP Server - Dynamic Tool Registry Demo"
echo "=================================================="
echo ""

echo "📝 This demo will show you how to:"
echo "   1. Register a GraphQL endpoint"
echo "   2. Generate tools automatically"
echo "   3. Execute generated operations"
echo ""

echo "🎯 Demo Steps:"
echo ""

echo "1️⃣  Start the MCP server:"
echo "   dotnet run"
echo ""

echo "2️⃣  Register the Countries GraphQL API:"
echo "   Tool: registerEndpoint"
echo "   Parameters:"
echo '   {
     "endpoint": "https://countries.trevorblades.com/",
     "endpointName": "countries", 
     "allowMutations": false,
     "toolPrefix": "country"
   }'
echo ""

echo "3️⃣  List generated tools:"
echo "   Tool: listDynamicTools"
echo "   (No parameters needed)"
echo ""

echo "4️⃣  Execute a generated operation:"
echo "   Tool: executeDynamicOperation"
echo "   Parameters:"
echo '   {
     "toolName": "country_query_countries",
     "variables": "{}"
   }'
echo ""

echo "5️⃣  Execute with parameters:"
echo "   Tool: executeDynamicOperation" 
echo "   Parameters:"
echo '   {
     "toolName": "country_query_country",
     "variables": "{\"code\": \"US\"}"
   }'
echo ""

echo "🔧 Advanced Examples:"
echo ""

echo "GitHub API (requires token):"
echo '   {
     "endpoint": "https://api.github.com/graphql",
     "endpointName": "github",
     "headers": "{\"Authorization\": \"Bearer YOUR_TOKEN\"}",
     "allowMutations": false,
     "toolPrefix": "gh"
   }'
echo ""

echo "Local Development API:"
echo '   {
     "endpoint": "http://localhost:4000/graphql", 
     "endpointName": "local_dev",
     "allowMutations": true,
     "toolPrefix": "dev"
   }'
echo ""

echo "✨ Ready to test! Start the server with: dotnet run"
