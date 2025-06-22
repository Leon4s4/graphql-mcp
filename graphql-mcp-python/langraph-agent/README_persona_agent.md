# Persona-Based Migration Agent

A LangGraph agent that uses specialized AI personas to handle complex software migration tasks more efficiently and systematically.

## Overview

This agent implements a persona-based approach to AI-assisted software development, breaking complex migration tasks into specialized roles:

- **Product Manager**: Gathers requirements and creates Product Requirements Documents (PRDs)
- **Software Architect**: Designs technical implementation and creates specifications
- **Implementer**: Executes the technical design and writes code
- **Problem Solver**: Debugs issues and finds solutions to problems

## Key Features

- **Systematic Workflow**: Each persona handles their specialized area of expertise
- **Flexible LLM Support**: Can use OpenAI models or local LLMs via LM Studio
- **Stateful Processing**: Maintains context across all persona interactions
- **Problem Resolution**: Built-in problem-solving workflow for debugging issues
- **Documentation Generation**: Creates PRDs and technical specifications

## Architecture

The agent uses LangGraph to orchestrate a state machine with the following flow:

```
Start → Coordinator → Product Manager → Architect → Implementer → Problem Solver → End
```

The coordinator determines which persona should act next based on the current state and available information.

## Usage

### Basic Usage

```python
from persona_migration_agent import create_persona_agent

# Create agent (uses online models by default)
agent = create_persona_agent(use_online_models=True)

# Define migration request
user_request = """
I need to migrate our REST API to GraphQL.
Current issues: multiple API calls, over-fetching data, poor performance.
Goals: reduce network requests, improve performance, better developer experience.
"""

# Run migration process
result = agent.run_migration(user_request)

if result["success"]:
    print("Migration completed!")
    for message in result["messages"]:
        print(message)
```

### Using Local LLM

```python
# Use local LLM instead of online models
agent = create_persona_agent(use_online_models=False)
```

### Example Scenarios

Run the example script to see the agent in action:

```bash
python example_persona_usage.py
```

## Persona Responsibilities

### Product Manager
- Analyzes user requirements
- Creates user stories with acceptance criteria
- Prioritizes features and requirements
- Asks clarifying questions when needed
- **Best Model**: GPT-4o-mini (focused, less likely to go off-track)

### Software Architect
- Designs technical implementation
- Creates step-by-step specifications
- Identifies dependencies and risks
- Plans architecture and approach
- **Best Model**: o1-mini (reasoning model for complex design)

### Implementer
- Follows technical specifications precisely
- Writes production-ready code
- Creates tests and documentation
- Verifies all requirements are met
- **Best Model**: GPT-4o-mini (focused, follows instructions well)

### Problem Solver
- Investigates and debugs issues
- Performs root cause analysis
- Proposes creative solutions
- Handles complex technical problems
- **Best Model**: Claude 3.5 Sonnet (excellent problem-solving abilities)

## State Management

The agent maintains state through a `PersonaAgentState` object that includes:

- Current persona and workflow position
- Requirements gathered by the product manager
- Technical design from the architect
- Implementation results and code
- Problems that need resolution
- Message history and context

## Configuration

### Environment Variables

Set up your environment variables for the desired LLM providers:

```bash
# OpenAI
export OPENAI_API_KEY="your-key-here"

# Anthropic (for Claude)
export ANTHROPIC_API_KEY="your-key-here"

# LangSmith (optional, for tracing)
export LANGCHAIN_API_KEY="your-key-here"
export LANGCHAIN_TRACING_V2=true
```

### Local LLM Setup

For local LLM usage, ensure LM Studio is running with specialized models:

1. Install and run LM Studio
2. Load specialized models for each persona:
   - **Product Manager**: `llama-3.1-8b-instruct` (structured tasks)
   - **Architect**: `deepseek-coder-6.7b-instruct` (reasoning & design)
   - **Implementer**: `codellama-13b-instruct` (code generation)
   - **Problem Solver**: `mistral-7b-instruct` (creative problem solving)
3. Start the local server on `http://localhost:1234`

**Note**: You can use a single model for all personas, but specialized models will give better results for each role.

## Benefits

This persona-based approach provides several advantages:

1. **Focused Expertise**: Each AI persona specializes in their domain
2. **Reduced Hallucination**: Personas stay focused on their specific role
3. **Better Planning**: Systematic approach from requirements to implementation
4. **Problem Resolution**: Dedicated problem-solving capabilities
5. **Maintainable Results**: Well-documented process and outcomes

## Files

- `persona_migration_agent.py`: Main agent implementation
- `example_persona_usage.py`: Example usage scenarios
- `csharp_to_graphql_agent.py`: Original single-purpose agent (for comparison)

## Dependencies

```bash
pip install langchain-openai langchain-core langgraph
```

## Future Enhancements

- Integration with external tools (GitHub, Jira, etc.)
- Support for additional LLM providers
- Enhanced file generation and management capabilities
- Integration with CI/CD pipelines
- Real-time collaboration features