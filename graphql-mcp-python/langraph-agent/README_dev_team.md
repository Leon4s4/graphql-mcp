# Software Development Team Agent

A comprehensive LangGraph agent that simulates a complete software development team using specialized AI personas to handle any software development project from conception to deployment.

## Overview

This agent implements a full software development team with four specialized personas working together on any software project:

- **Product Manager**: Requirements gathering, user stories, acceptance criteria
- **Software Architect**: Technical design, architecture decisions, implementation planning  
- **Software Engineer**: Code implementation, testing, documentation
- **DevOps/Problem Solver**: Issue resolution, debugging, system optimization

## Key Features

- **Complete SDLC Coverage**: From requirements to deployment
- **Flexible Project Types**: Features, bug fixes, refactoring, new projects
- **Intelligent Workflow**: Coordinator routes work to appropriate team members
- **Issue Management**: Automatic problem detection and resolution
- **Production-Ready Output**: Code, tests, documentation, deployment scripts
- **Scalable Architecture**: Works with online models or local LLMs

## Supported Project Types

### 1. New Features (`feature`)
- Feature development from requirements to implementation
- User story creation and technical specification
- Complete code implementation with tests
- Documentation and deployment preparation

### 2. Bug Fixes (`bugfix`)
- Rapid issue analysis and root cause identification
- Problem-solving approach with multiple solution options
- Implementation of fixes with testing verification
- Prevention strategies for similar issues

### 3. Code Refactoring (`refactor`)
- Legacy code modernization and optimization
- Architecture improvements and pattern implementation
- Performance optimization and maintainability enhancement
- Comprehensive testing and documentation updates

### 4. New Projects (`new_project`)
- Complete project planning and architecture design
- Technology stack selection and justification
- Implementation roadmap and team coordination
- Infrastructure and deployment strategy

## Usage

### Basic Usage

```python
from software_dev_team_agent import create_dev_team_agent

# Create the development team
team = create_dev_team_agent(use_online_models=True)

# Define your project
project_request = """
Build a REST API for a task management system.
- User authentication with JWT
- CRUD operations for tasks and projects
- Role-based permissions
- PostgreSQL database
- Comprehensive testing
"""

# Let the team handle it
result = team.develop_project(project_request, project_type="feature")

if result["success"]:
    print("✅ Project completed successfully!")
    for message in result["messages"]:
        print(message)
```

### Advanced Example - Bug Fix

```python
team = create_dev_team_agent()

bug_report = """
URGENT: Payment processing failing for credit cards.
- 30% of transactions being declined
- Started after yesterday's deployment  
- Error: "Invalid card number" for valid cards
- Revenue impact: $5,000/hour
- Must fix within 4 hours
"""

result = team.develop_project(bug_report, project_type="bugfix")
```

### Example Scenarios

Run comprehensive examples:

```bash
python example_dev_team_usage.py
```

## Team Member Responsibilities

### Product Manager
**Primary Role**: Requirements analysis and user story creation

**Responsibilities**:
- Analyze project requests and gather detailed requirements
- Create user stories with acceptance criteria  
- Prioritize features and estimate story points
- Define success metrics and project scope
- Handle stakeholder communication and clarification

**Output**: User stories, acceptance criteria, priority matrix, requirements documentation

### Software Architect  
**Primary Role**: Technical design and system architecture

**Responsibilities**:
- Design scalable, maintainable system architecture
- Make technology stack and framework decisions
- Create detailed implementation plans and phases
- Identify technical risks and mitigation strategies
- Define testing strategies and deployment approaches

**Output**: Technical specifications, architecture diagrams, implementation roadmap, risk assessment

### Software Engineer
**Primary Role**: Code implementation and testing

**Responsibilities**:
- Implement features according to technical specifications
- Write comprehensive test suites (unit, integration, e2e)
- Create and update technical documentation
- Follow coding standards and best practices
- Ensure all acceptance criteria are met

**Output**: Production-ready code, test suites, documentation, deployment scripts

### DevOps/Problem Solver
**Primary Role**: Issue resolution and system optimization

**Responsibilities**:
- Debug complex technical issues and performance problems
- Perform root cause analysis using systematic approaches
- Implement monitoring, alerting, and observability solutions
- Optimize system performance and resource usage
- Develop prevention strategies for recurring issues

**Output**: Issue resolution plans, system optimizations, monitoring setup, troubleshooting guides

## Workflow Architecture

The team follows an intelligent workflow managed by a coordinator:

```
Start → Coordinator → Product Manager → Architect → Engineer → Problem Solver → End
                 ↑                                                      ↓
                 └──────────── Routing Logic ←──────────────────────────┘
```

### Workflow Logic

1. **Initial Assessment**: Coordinator analyzes project type and requirements
2. **Requirements Phase**: Product Manager creates user stories and acceptance criteria
3. **Design Phase**: Software Architect creates technical specifications
4. **Implementation Phase**: Software Engineer implements the solution
5. **Issue Resolution**: Problem Solver handles any issues that arise
6. **Completion**: Coordinator generates final project summary

### Dynamic Routing

The coordinator intelligently routes work based on:
- Current project state and completeness
- Presence of active issues requiring immediate attention
- Project type and complexity
- Team member availability and specialization

## Configuration

### Environment Variables

```bash
# Online Models (Recommended)
OPENAI_API_KEY=your_openai_key_here
ANTHROPIC_API_KEY=your_anthropic_key_here  # For enhanced problem solving

# LangSmith Tracing (Optional)
LANGCHAIN_API_KEY=your_langsmith_key_here
LANGCHAIN_TRACING_V2=true
LANGSMITH_PROJECT=dev-team-agent
```

### Model Selection

**Online Models (Default)**:
- **Product Manager**: GPT-4o-mini (focused, instruction-following)
- **Architect**: o1-mini (reasoning model for complex design)
- **Engineer**: GPT-4o-mini (reliable code generation)
- **Problem Solver**: Claude 3.5 Sonnet (superior debugging abilities)

**Local Models** (when `use_online_models=False`):
- **Product Manager**: llama-3.1-8b-instruct
- **Architect**: deepseek-coder-6.7b-instruct  
- **Engineer**: codellama-13b-instruct
- **Problem Solver**: mistral-7b-instruct

## LangGraph Studio Integration

The agent is fully compatible with LangGraph Studio for visual workflow management.

### Setup for LangGraph Studio

1. **Open LangGraph Studio**
2. **Load Project**: Select `/Users/Git/graphql-mcp/graphql-mcp-python/langraph-agent`
3. **Choose Graph**: Select `dev_team` from the dropdown
4. **Input Format**:

```json
{
  "project_request": "Your project description here",
  "project_type": "feature",
  "current_persona": "coordinator",
  "messages": [],
  "user_stories": null,
  "technical_spec": null,
  "code_deliverables": null,
  "issues": [],
  "next_action": null,
  "iteration_count": 0
}
```

### Studio Visualization

- **Node Visualization**: See each team member as a distinct node
- **Flow Tracking**: Watch work flow between team members
- **State Inspection**: Monitor project state and deliverables
- **Message History**: Review complete team collaboration

## Real-World Applications

### Startup Development
- **MVP Planning**: Product Manager defines core features
- **Architecture**: Architect designs scalable foundation
- **Implementation**: Engineer builds with testing and documentation
- **Launch Support**: Problem Solver handles deployment issues

### Enterprise Projects
- **Feature Development**: Complete feature lifecycle management
- **Legacy Modernization**: Systematic refactoring and migration
- **Bug Triage**: Rapid issue resolution with root cause analysis
- **Performance Optimization**: System-wide improvements

### Consulting Scenarios
- **Client Requirements**: Product Manager translates business needs
- **Technical Proposals**: Architect creates detailed implementation plans
- **Delivery**: Engineer provides production-ready solutions
- **Support**: Problem Solver handles post-deployment issues

## Benefits

### Comprehensive Coverage
- **End-to-End Development**: From concept to deployment
- **Professional Quality**: Production-ready deliverables
- **Best Practices**: Industry-standard approaches and patterns
- **Documentation**: Complete technical and user documentation

### Intelligent Collaboration
- **Specialized Expertise**: Each persona excels in their domain
- **Context Awareness**: Team members understand project history
- **Adaptive Workflow**: Dynamic routing based on project needs
- **Quality Assurance**: Multiple review points and validation

### Scalability
- **Flexible Deployment**: Online models or local LLMs
- **Project Variety**: Handles any software development scenario
- **Team Efficiency**: Parallel processing where possible
- **Resource Optimization**: Right expert for the right task

## Troubleshooting

### Common Issues

1. **Missing API Keys**: Agent gracefully falls back to local models
2. **Local LLM Setup**: Ensure LM Studio is running with appropriate models
3. **Graph Loading**: Verify `langgraph.json` includes the `dev_team` entry
4. **Memory Issues**: Large projects may require streaming or chunking

### Performance Optimization

- **Model Selection**: Use online models for best results
- **Context Management**: Agent automatically manages conversation history
- **Parallel Processing**: Team members can work independently when possible
- **Caching**: Results are cached to avoid redundant work

### Debug Commands

```bash
# Test agent creation
python -c "from software_dev_team_agent import create_dev_team_agent; create_dev_team_agent()"

# Run example scenarios
python example_dev_team_usage.py

# Check graph export
python -c "from software_dev_team_agent import graph; print('Graph ready:', graph is not None)"
```

## Files

- `software_dev_team_agent.py`: Main team agent implementation
- `example_dev_team_usage.py`: Comprehensive usage examples  
- `README_dev_team.md`: This documentation
- `langgraph.json`: LangGraph Studio configuration

## Future Enhancements

- **Code Review Agent**: Automated code quality and security analysis
- **Testing Specialist**: Dedicated testing strategy and automation
- **UI/UX Designer**: Frontend design and user experience optimization
- **Security Analyst**: Security assessment and vulnerability analysis
- **Performance Engineer**: Specialized performance optimization and monitoring

## Getting Started

1. **Install Dependencies**: `pip install -r requirements.txt`
2. **Set API Keys**: Configure environment variables
3. **Run Examples**: `python example_dev_team_usage.py`
4. **Try LangGraph Studio**: Load project and select `dev_team` graph
5. **Start Building**: Use the team for your own software projects!

The Software Development Team Agent transforms how you approach software development by providing expert collaboration at every stage of the project lifecycle.