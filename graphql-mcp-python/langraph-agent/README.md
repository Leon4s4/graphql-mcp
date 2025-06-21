# C# to GraphQL Migration Agent

A LangGraph-powered agent that specializes in migrating .NET C# code with REST API calls to GraphQL queries. This agent leverages local LLMs (like Gemma 3 running in LM Studio) and integrates with the existing GraphQL MCP tools.

## Features

- **Automated C# Code Analysis**: Identifies REST API calls, entities, and data patterns
- **GraphQL Query Generation**: Creates optimized GraphQL queries to replace multiple REST calls
- **Migration Planning**: Provides step-by-step migration guidance with code examples
- **Performance Analysis**: Estimates performance improvements from REST to GraphQL migration
- **Local LLM Support**: Works with LM Studio and other local model servers
- **MCP Integration**: Uses tools from the graphql-mcp-python project

## Installation

1. Install dependencies:
```bash
cd graphql-mcp-python/langraph-agent
pip install -r requirements.txt
```

2. Ensure LM Studio is running with Gemma 3 model:
   - Start LM Studio
   - Load the Gemma 3 model
   - Start the local server (usually on http://localhost:1234)

## Quick Start

```python
from csharp_to_graphql_agent import create_agent

# Create the agent
agent = create_agent(
    llm_base_url="http://localhost:1234/v1",
    model_name="gemma-3"
)

# Analyze C# code
csharp_code = '''
public async Task<User> GetUserAsync(int id)
{
    var user = await httpClient.GetAsync($"/api/users/{id}");
    var posts = await httpClient.GetAsync($"/api/users/{id}/posts");
    // ... more REST calls
}
'''

# Run migration analysis
result = agent.migrate_csharp_code(csharp_code)

if result["success"]:
    print(f"Found {len(result['rest_calls'])} REST calls")
    print(f"Generated {len(result['graphql_queries'])} GraphQL queries")
    # Access migration plan, performance analysis, etc.
```

## Configuration

The agent can be configured via environment variables or directly:

```python
# Environment variables
export LLM_BASE_URL="http://localhost:1234/v1"
export MODEL_NAME="gemma-3"
export LLM_TEMPERATURE="0.1"

# Or configure directly
from config import AgentConfig

config = AgentConfig(
    llm_base_url="http://localhost:1234/v1",
    model_name="gemma-3",
    temperature=0.1,
    enable_detailed_analysis=True
)
```

## Example Usage

Run the included example:

```bash
python example_usage.py
```

This will analyze a comprehensive C# service class and demonstrate:
- REST API call identification
- Entity extraction
- GraphQL query generation
- Migration planning
- Performance analysis

## Agent Workflow

The LangGraph agent follows this workflow:

1. **Code Analysis**: 
   - Extracts REST API calls using regex patterns
   - Identifies entities and data models
   - Analyzes data flow patterns

2. **Query Generation**:
   - Groups related REST calls
   - Generates optimized GraphQL queries
   - Adds variables, fragments, and field selection

3. **Migration Planning**:
   - Creates step-by-step migration guide
   - Provides code examples (before/after)
   - Estimates performance improvements

4. **Finalization**:
   - Compiles comprehensive results
   - Provides recommendations and best practices

## Integration with MCP Tools

The agent is designed to work alongside the existing GraphQL MCP tools:

```python
# Use with existing MCP endpoints
from graphql_mcp.helpers.endpoint_registry import EndpointRegistryService

# Register GraphQL endpoint first
EndpointRegistryService.Instance.register_endpoint(
    name="my-graphql-api",
    url="https://api.example.com/graphql"
)

# Then use agent to migrate code targeting that endpoint
result = agent.migrate_csharp_code(csharp_code)
```

## Supported C# Patterns

The agent recognizes these C# REST API patterns:

- `HttpClient.GetAsync()`, `PostAsync()`, `PutAsync()`, `DeleteAsync()`
- `await httpClient.SendAsync()`
- URL parameters with `{id}` placeholders
- LINQ operations on API responses
- Multiple chained API calls
- Entity classes and data models

## Output Format

The agent returns a comprehensive analysis including:

```json
{
  "success": true,
  "rest_calls": [...],           // Identified REST calls
  "entities": [...],             // Extracted entities  
  "graphql_queries": [...],      // Generated GraphQL queries
  "migration_plan": "...",       // Step-by-step migration guide
  "messages": [...]              // Agent conversation log
}
```

## Local LLM Requirements

- **LM Studio**: Version 0.2.15 or higher
- **Gemma 3**: Recommended model (2B or 7B parameter versions)
- **Alternative Models**: Any OpenAI-compatible local model server
- **Memory**: At least 8GB RAM for Gemma 3 2B, 16GB+ for 7B

## Troubleshooting

### LM Studio Connection Issues
- Ensure LM Studio server is running on the correct port
- Check that the model is properly loaded
- Verify the base URL in configuration

### Analysis Issues
- Ensure C# code contains actual REST API calls
- Check for proper HttpClient usage patterns
- Verify entity classes are properly defined

### Performance
- Use smaller models (2B) for faster analysis
- Adjust temperature for more focused responses
- Enable/disable features based on needs

## Advanced Usage

### Custom Analysis
```python
# Customize analysis behavior
agent = create_agent()
agent._analyze_csharp_code.invoke({
    "csharp_code": code,
    "analysis_mode": "comprehensive"
})
```

### Batch Processing
```python
# Process multiple files
results = []
for file_path in csharp_files:
    with open(file_path) as f:
        code = f.read()
    result = agent.migrate_csharp_code(code)
    results.append(result)
```

### Integration with Existing Tools
```python
# Use with existing MCP code migration tools
from graphql_mcp.tools.code_migration import register_tools

# Combine LangGraph agent with MCP tools
# for comprehensive migration workflow
```

## Contributing

To extend the agent:

1. Add new analysis patterns in `_analyze_csharp_code()`
2. Enhance GraphQL query generation in `_generate_graphql_queries()`
3. Improve migration planning in `_create_migration_plan()`
4. Add new LLM providers in the configuration

## Roadmap

- [ ] Support for additional .NET patterns (Entity Framework, etc.)
- [ ] Integration with more MCP tools
- [ ] Support for TypeScript/JavaScript migration
- [ ] GraphQL schema validation
- [ ] Automated testing of generated queries
- [ ] Visual migration reports

## License

This project follows the same license as the parent graphql-mcp-python project.