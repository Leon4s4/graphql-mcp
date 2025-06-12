# GraphQL MCP Server - Prompts Demo

This document demonstrates how to use the comprehensive GraphQL expert prompts available in this MCP server.

## 🚀 What Are MCP Prompts?

MCP prompts are intelligent, parameterized templates that provide expert-level guidance and assistance. Unlike tools
that perform actions, prompts provide contextual expertise and generate guidance based on your specific situation.

## 💡 Available GraphQL Expert Prompts

### 1. **Query Generation & Optimization**

#### `GenerateQuery` - Expert Query Creation

Get optimized GraphQL queries tailored to your requirements:

```json
{
  "dataRequirement": "Get e-commerce product catalog with pricing, inventory, and customer reviews",
  "performance": "fast",
  "includeRelated": "yes"
}
```

**Result**: Receives expert guidance on:

- Optimized query structure with fragments
- Performance considerations and field selection
- Variable parameterization recommendations
- Usage examples and best practices

#### `AnalyzeSchema` - Schema Expert Analysis

Get comprehensive schema analysis:

```json
{
  "focus": "performance",
  "endpointName": "github",
  "specificType": "Repository"
}
```

**Analysis Types:**

- `structure` - Design patterns and organization
- `performance` - N+1 problems and optimization opportunities
- `security` - DoS vulnerabilities and auth patterns
- `evolution` - Breaking changes and migration planning

### 2. **Development Workflow Guidance**

#### `DevelopmentWorkflow` - Phase-Specific Best Practices

Get expert guidance for any development phase:

```json
{
  "phase": "implementation",
  "teamSize": "medium",
  "techStack": "dotnet"
}
```

**Phases Covered:**

- `planning` - Requirements analysis and schema design
- `implementation` - Development patterns and quality assurance
- `testing` - Testing strategies and automation
- `deployment` - Production readiness and monitoring

#### `DebuggingAssistant` - Expert Troubleshooting

Get systematic debugging guidance:

```json
{
  "issueType": "performance",
  "severity": "high",
  "environment": "production"
}
```

**Issue Types:**

- `query-error` - Syntax and validation problems
- `performance` - Slow queries and bottlenecks
- `schema` - Type definition and resolver issues
- `resolver` - Data fetching and error handling

### 3. **Architecture & Design Guidance**

#### `SchemaDesign` - Architecture Expert

Get comprehensive schema design guidance:

```json
{
  "designFocus": "new-schema",
  "domain": "e-commerce", 
  "scale": "enterprise"
}
```

**Design Focus Areas:**

- `new-schema` - Green field design principles
- `refactoring` - Safe improvement strategies
- `migration` - Version evolution and compatibility
- `optimization` - Performance and structure improvements

**Domain Expertise:**

- E-commerce, Social Media, API Gateway, Microservices

### 4. **Testing Strategy & Implementation**

#### `TestingStrategy` - Comprehensive Testing Guidance

Get expert testing strategies:

```json
{
  "testingType": "integration",
  "operationType": "mutation",
  "framework": "xunit"
}
```

**Testing Types:**

- `unit` - Resolver and schema testing
- `integration` - End-to-end query flows
- `performance` - Load and complexity testing
- `security` - Authorization and DoS protection

### 5. **Documentation & Learning**

#### `DocumentationGuide` - Professional Documentation

Create comprehensive API documentation:

```json
{
  "docType": "getting-started",
  "audience": "developers",
  "format": "interactive"
}
```

**Documentation Types:**

- `api-reference` - Complete API documentation
- `getting-started` - Onboarding guides
- `best-practices` - Implementation guidelines
- `migration` - Version migration guides

#### `TrainingMaterial` - Team Education

Generate training curricula:

```json
{
  "level": "intermediate",
  "format": "workshop",
  "focus": "implementation",
  "duration": 16
}
```

**Training Levels:**

- `beginner` - GraphQL fundamentals
- `intermediate` - Implementation and integration
- `advanced` - Architecture and optimization
- `expert` - Enterprise patterns and governance

## 🎯 Real-World Usage Examples

### Example 1: New Project Setup

```json
// Step 1: Get architecture guidance
{
  "prompt": "SchemaDesign",
  "designFocus": "new-schema",
  "domain": "social",
  "scale": "medium"
}

// Step 2: Get development workflow
{
  "prompt": "DevelopmentWorkflow", 
  "phase": "planning",
  "teamSize": "small",
  "techStack": "dotnet"
}

// Step 3: Create documentation plan
{
  "prompt": "DocumentationGuide",
  "docType": "getting-started",
  "audience": "developers",
  "format": "markdown"
}
```

### Example 2: Performance Optimization

```json
// Step 1: Analyze schema for performance issues
{
  "prompt": "AnalyzeSchema",
  "focus": "performance",
  "endpointName": "production_api"
}

// Step 2: Get debugging guidance
{
  "prompt": "DebuggingAssistant",
  "issueType": "performance", 
  "severity": "high",
  "environment": "production"
}

// Step 3: Generate optimized queries
{
  "prompt": "GenerateQuery",
  "dataRequirement": "User dashboard with minimal data",
  "performance": "fast"
}
```

### Example 3: Team Training

```json
// Step 1: Create training material
{
  "prompt": "TrainingMaterial",
  "level": "intermediate",
  "format": "course",
  "focus": "optimization",
  "duration": 20
}

// Step 2: Generate documentation
{
  "prompt": "DocumentationGuide",
  "docType": "best-practices",
  "audience": "developers",
  "format": "interactive"
}
```

## 🔧 Integration with Tools

The prompts work seamlessly with the server's tools:

1. **Use prompts for guidance** → Get expert recommendations
2. **Use tools for implementation** → Execute the recommendations
3. **Iterate and improve** → Refine based on results

For example:

1. Use `AnalyzeSchema` prompt to identify performance issues
2. Use `analyze_dataloader_patterns` tool to get specific metrics
3. Use `GenerateQuery` prompt to get optimized query patterns
4. Use `QueryGraphQL` tool to test the optimized queries

## 🚀 Getting Started

1. **Connect your MCP client** to the GraphQL MCP server
2. **Browse available prompts** using your client's interface
3. **Start with a simple prompt** like `GenerateQuery` to get familiar
4. **Combine prompts and tools** for comprehensive GraphQL development

The prompts transform your MCP client into a GraphQL expert consultant, providing exactly the guidance you need at every
stage of your GraphQL journey!
