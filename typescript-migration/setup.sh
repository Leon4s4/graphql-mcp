#!/bin/bash

# GraphQL MCP Server - TypeScript Setup Script
set -e

echo "🚀 Setting up GraphQL MCP Server (TypeScript)..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js 18+ first."
    exit 1
fi

# Check Node.js version
NODE_VERSION=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
if [ "$NODE_VERSION" -lt "18" ]; then
    echo "❌ Node.js version 18+ is required. Current version: $(node -v)"
    exit 1
fi

echo "✅ Node.js $(node -v) detected"

# Install dependencies
echo "📦 Installing dependencies..."
npm install

# Copy environment file if it doesn't exist
if [ ! -f .env ]; then
    echo "📝 Creating .env file..."
    cp .env.example .env
    echo "✅ .env file created. Please update with your GraphQL endpoint settings."
fi

# Build the project
echo "🔨 Building TypeScript project..."
npm run build

# Run tests
echo "🧪 Running tests..."
npm test

echo ""
echo "🎉 Setup complete!"
echo ""
echo "📋 Next steps:"
echo "   1. Update your .env file with your GraphQL endpoint"
echo "   2. Run 'npm run dev' to start development server"
echo "   3. Run 'npm start' to start production server"
echo ""
echo "🔧 Available commands:"
echo "   npm run dev    - Start development server with hot reload"
echo "   npm run build  - Build for production"
echo "   npm start      - Start production server"
echo "   npm test       - Run tests"
echo "   npm run lint   - Lint code"
echo ""
echo "📖 See README.md for more information"
