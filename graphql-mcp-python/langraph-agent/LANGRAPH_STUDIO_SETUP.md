# LangGraph Studio Setup

## Prerequisites

1. **Install LangGraph Studio**: Download from [LangGraph Studio](https://smith.langchain.com/studio)
2. **API Keys**: Set up your environment variables (see below)
3. **Local LLM** (optional): Set up LM Studio if using offline models

## Environment Setup

Create or update your `.env` file in this directory:

```bash
# LangSmith (required for LangGraph Studio)
LANGCHAIN_API_KEY=your_langsmith_api_key_here
LANGCHAIN_TRACING_V2=true
LANGSMITH_PROJECT=persona-migration-agent

# OpenAI (if using online models)
OPENAI_API_KEY=your_openai_api_key_here

# Anthropic (if using Claude for problem solver)
ANTHROPIC_API_KEY=your_anthropic_api_key_here
```

## Running in LangGraph Studio

### Step 1: Open LangGraph Studio
1. Launch LangGraph Studio
2. Click "Open" and select this directory: `/Users/Git/graphql-mcp/graphql-mcp-python/langraph-agent`

### Step 2: Available Graphs
The studio will detect two graphs from `langgraph.json`:

1. **`csharp_migration`** - Original single-purpose C# to GraphQL agent
2. **`persona_migration`** - New persona-based migration agent ⭐

### Step 3: Select Persona Migration Graph
1. Choose `persona_migration` from the graph dropdown
2. The studio will show the persona-based workflow with nodes:
   - Coordinator (routing logic)
   - Product Manager
   - Software Architect  
   - Implementer
   - Problem Solver

### Step 4: Test the Agent

#### Example Input:
```json
{
  "user_request": "I need to migrate our REST API to GraphQL. We have performance issues with multiple API calls and over-fetching data. The team wants better developer experience and faster loading times.",
  "current_persona": "coordinator",
  "messages": [],
  "requirements": null,
  "technical_design": null,
  "implementation": null,
  "problems": [],
  "docs_directory": "docs",
  "next_action": null
}
```

#### Expected Flow:
1. **Coordinator** → Determines to start with Product Manager
2. **Product Manager** → Gathers requirements, creates PRD
3. **Coordinator** → Routes to Software Architect  
4. **Software Architect** → Creates technical specification
5. **Coordinator** → Routes to Implementer
6. **Implementer** → Generates implementation code
7. **Coordinator** → Completes or routes to Problem Solver if issues

## Model Configuration

### Using Online Models (Default)
The agent is configured to use:
- **Product Manager**: GPT-4o-mini
- **Architect**: o1-mini (reasoning model)
- **Implementer**: GPT-4o-mini  
- **Problem Solver**: Claude 3.5 Sonnet

### Using Local Models
To use local LLMs, modify the agent initialization:

```python
# In persona_migration_agent.py, line ~549
agent = create_persona_agent(use_online_models=False)
```

Requires LM Studio running with these models:
- **Product Manager**: llama-3.1-8b-instruct
- **Architect**: deepseek-coder-6.7b-instruct
- **Implementer**: codellama-13b-instruct
- **Problem Solver**: mistral-7b-instruct

## Troubleshooting

### Common Issues:

1. **"Graph not found"**: Ensure `langgraph.json` is in the project root
2. **"Module not found"**: Run `pip install -e .` in the langraph-agent directory
3. **API key errors**: The agent now gracefully falls back to local models if API keys are missing
4. **Local LLM issues**: Ensure LM Studio is running on `http://localhost:1234`
5. **"OpenAIError: api_key must be set"**: This is now handled automatically - the agent will use local models

### Debug Steps:
```bash
# Test the agent locally first
cd /Users/Git/graphql-mcp/graphql-mcp-python/langraph-agent
python example_persona_usage.py

# Check if the graph exports correctly
python -c "from persona_migration_agent import graph; print('Graph loaded successfully')"
```

## Studio Features

### Visualization
- See the persona workflow as a visual graph
- Watch messages flow between personas
- Monitor state changes in real-time

### Debugging
- Step through each persona's execution
- Inspect intermediate states
- Modify inputs and re-run

### Export
- Export conversation threads
- Save successful workflows as templates
- Share configurations with team

## Next Steps

1. Open LangGraph Studio
2. Load this project directory
3. Select the `persona_migration` graph
4. Start with the example input above
5. Watch the personas collaborate on your migration task!

For more complex scenarios, see `example_persona_usage.py` for additional test cases.