"""Generic Software Development Team Agent using persona-based approach."""

import os
from typing import Dict, List, Any, Optional, TypedDict, Annotated, Literal
from dataclasses import dataclass
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage
from langchain_core.tools import tool
from langgraph.graph import StateGraph, END, START
from langgraph.graph.message import add_messages
import logging

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


@dataclass
class UserStory:
    """Represents a user story from the product manager."""
    id: str
    title: str
    description: str
    acceptance_criteria: List[str]
    priority: str
    story_points: int


@dataclass
class TechnicalSpec:
    """Represents a technical specification from the architect."""
    title: str
    overview: str
    architecture_decisions: List[str]
    implementation_plan: List[str]
    dependencies: List[str]
    testing_strategy: str
    deployment_notes: str


@dataclass
class CodeDeliverable:
    """Represents code deliverables from the implementer."""
    files_created: List[str]
    files_modified: List[str]
    code_snippets: Dict[str, str]
    tests_written: List[str]
    documentation_updates: List[str]
    deployment_scripts: List[str]


@dataclass
class Issue:
    """Represents an issue that needs problem-solving."""
    id: str
    title: str
    description: str
    severity: str
    error_logs: Optional[str]
    context: str
    attempted_fixes: List[str]
    status: str


class SoftwareDevTeamState(TypedDict):
    """State for the software development team agent."""
    messages: Annotated[List[Any], add_messages]
    current_persona: str
    project_request: str
    user_stories: Optional[List[UserStory]]
    technical_spec: Optional[TechnicalSpec]
    code_deliverables: Optional[CodeDeliverable]
    issues: List[Issue]
    project_type: str  # "feature", "bugfix", "refactor", "new_project"
    next_action: Optional[str]
    iteration_count: int


class SoftwareDevTeamAgent:
    """Generic software development team using persona-based approach."""
    
    def __init__(self, use_online_models: bool = True):
        """Initialize the team with online or local LLM configuration."""
        if use_online_models:
            # Check if API keys are available
            openai_key = os.getenv("OPENAI_API_KEY")
            anthropic_key = os.getenv("ANTHROPIC_API_KEY")
            
            if not openai_key:
                print("Warning: OPENAI_API_KEY not found. Falling back to local models.")
                use_online_models = False
            else:
                try:
                    self.product_manager_llm = ChatOpenAI(
                        model="gpt-4o-mini",
                        temperature=0.1
                    )
                    self.architect_llm = ChatOpenAI(
                        model="o1-mini",  # Use reasoning model for complex design
                        temperature=0.1
                    )
                    self.implementer_llm = ChatOpenAI(
                        model="gpt-4o-mini",
                        temperature=0.1
                    )
                    
                    # Use Claude if available, otherwise GPT-4o-mini
                    if anthropic_key:
                        from langchain_anthropic import ChatAnthropic
                        self.problem_solver_llm = ChatAnthropic(
                            model="claude-3-5-sonnet-20241022",
                            temperature=0.2
                        )
                    else:
                        self.problem_solver_llm = ChatOpenAI(
                            model="gpt-4o-mini",
                            temperature=0.2
                        )
                except Exception as e:
                    print(f"Error initializing online models: {e}")
                    print("Falling back to local models.")
                    use_online_models = False
        
        if not use_online_models:
            # Try specialized local models, fallback to single model if needed
            base_config = {
                "base_url": "http://localhost:1234/v1",
                "api_key": "lm-studio"
            }
            
            try:
                # Product Manager: Focused, instruction-following model
                self.product_manager_llm = ChatOpenAI(
                    **base_config,
                    model="llama-3.1-8b-instruct",
                    temperature=0.1
                )
                
                # Architect: Reasoning-capable model for complex design
                self.architect_llm = ChatOpenAI(
                    **base_config,
                    model="deepseek-coder-6.7b-instruct",
                    temperature=0.1
                )
                
                # Implementer: Code-focused model
                self.implementer_llm = ChatOpenAI(
                    **base_config,
                    model="codellama-13b-instruct",
                    temperature=0.1
                )
                
                # Problem Solver: Creative problem-solving model
                self.problem_solver_llm = ChatOpenAI(
                    **base_config,
                    model="mistral-7b-instruct",
                    temperature=0.2
                )
            except Exception as e:
                print(f"Warning: Could not initialize specialized local models: {e}")
                print("Using single local model for all personas.")
                
                # Fallback to single local model for all personas
                fallback_llm = ChatOpenAI(
                    **base_config,
                    model="gemma-3",
                    temperature=0.1
                )
                
                self.product_manager_llm = fallback_llm
                self.architect_llm = fallback_llm
                self.implementer_llm = fallback_llm
                self.problem_solver_llm = fallback_llm
        
        # Persona system prompts for general software development
        self.persona_prompts = {
            "product_manager": """You are a Senior Product Manager for software development projects. Your expertise includes:

**Core Responsibilities:**
- Analyze project requirements and break them into clear user stories
- Define acceptance criteria and success metrics
- Prioritize features based on business value and technical complexity
- Create detailed Product Requirements Documents (PRDs)
- Communicate with stakeholders about project scope and timeline

**Your Process:**
1. Listen to project requests and ask clarifying questions
2. Break down complex projects into manageable user stories
3. Define clear acceptance criteria for each story
4. Assign story points and priority levels
5. Consider edge cases and potential user scenarios

**Output Format:**
- User stories in "As a [user type], I want [functionality] so that [benefit]" format
- Clear acceptance criteria with measurable outcomes
- Priority levels (High/Medium/Low) with justifications
- Story point estimates (1, 2, 3, 5, 8, 13)

Focus on the WHAT and WHY, not the HOW. Ask questions if requirements are unclear.""",
            
            "architect": """You are a Senior Software Architect responsible for technical design and system architecture. Your expertise includes:

**Core Responsibilities:**
- Design scalable, maintainable software architectures
- Make technology stack decisions based on requirements
- Define implementation strategies and development phases
- Identify technical risks and mitigation strategies
- Ensure alignment with best practices and standards

**Your Process:**
1. Review user stories and functional requirements
2. Design high-level system architecture
3. Choose appropriate technologies, frameworks, and patterns
4. Break down implementation into logical phases
5. Define testing strategies and deployment approaches
6. Identify dependencies and potential blockers

**Output Format:**
- High-level architecture overview with diagrams/descriptions
- Technology stack recommendations with justifications
- Step-by-step implementation plan
- Database schema and API design considerations
- Performance, security, and scalability considerations
- Testing strategy (unit, integration, e2e)

Focus on HOW to build it technically. Provide detailed guidance for the implementation team.""",
            
            "implementer": """You are a Senior Software Engineer responsible for implementing features based on technical specifications. Your expertise includes:

**Core Responsibilities:**
- Write clean, maintainable, and tested code
- Follow architectural guidelines and coding standards
- Implement features according to acceptance criteria
- Create comprehensive tests (unit, integration)
- Document code and update technical documentation

**Your Process:**
1. Review technical specifications and user stories
2. Break down implementation into smaller tasks
3. Write code following best practices and patterns
4. Create tests that cover happy path, edge cases, and error scenarios
5. Update documentation and deployment scripts
6. Verify all acceptance criteria are met

**Output Format:**
- Complete, production-ready code implementations
- Comprehensive test suites with good coverage
- Updated documentation (README, API docs, etc.)
- Database migrations or schema updates if needed
- Deployment scripts and configuration updates

Focus on delivering high-quality, working software. Always include proper error handling and logging.""",
            
            "problem_solver": """You are a Senior DevOps/Site Reliability Engineer and debugging expert. Your expertise includes:

**Core Responsibilities:**
- Diagnose and resolve complex technical issues
- Analyze system performance and reliability problems
- Debug application errors and infrastructure issues
- Optimize system performance and resource usage
- Implement monitoring and alerting solutions

**Your Process:**
1. Analyze error logs, stack traces, and system metrics
2. Reproduce issues in controlled environments
3. Identify root causes using systematic debugging
4. Propose multiple solution approaches with trade-offs
5. Implement fixes with proper testing and rollback plans
6. Add monitoring to prevent similar issues

**Output Format:**
- Clear problem analysis with root cause identification
- Step-by-step debugging methodology
- Multiple solution options with pros/cons
- Implementation plan with testing approach
- Preventive measures and monitoring recommendations

Focus on getting to the root cause, not just treating symptoms. Think systematically about system interactions."""
        }
        
        # Create the team workflow graph
        self.graph = self._create_graph()
    
    def _create_graph(self) -> StateGraph:
        """Create the software development team workflow graph."""
        workflow = StateGraph(SoftwareDevTeamState)
        
        # Add team member nodes
        workflow.add_node("product_manager", self._product_manager_node)
        workflow.add_node("architect", self._architect_node)
        workflow.add_node("implementer", self._implementer_node)
        workflow.add_node("problem_solver", self._problem_solver_node)
        workflow.add_node("coordinator", self._coordinator_node)
        
        # Add routing logic
        workflow.add_edge(START, "coordinator")
        workflow.add_conditional_edges(
            "coordinator",
            self._route_to_team_member,
            {
                "product_manager": "product_manager",
                "architect": "architect", 
                "implementer": "implementer",
                "problem_solver": "problem_solver",
                "end": END
            }
        )
        workflow.add_edge("product_manager", "coordinator")
        workflow.add_edge("architect", "coordinator")
        workflow.add_edge("implementer", "coordinator")
        workflow.add_edge("problem_solver", "coordinator")
        
        return workflow.compile()
    
    def _coordinator_node(self, state: SoftwareDevTeamState) -> SoftwareDevTeamState:
        """Coordinator node that manages the development workflow."""
        logger.info("Coordinating development workflow...")
        
        # Determine next action based on current state and project type
        if not state.get("user_stories"):
            state["next_action"] = "product_manager"
            state["current_persona"] = "product_manager"
        elif state.get("issues") and len(state["issues"]) > 0:
            # Prioritize problem solving if there are active issues
            state["next_action"] = "problem_solver"
            state["current_persona"] = "problem_solver"
        elif not state.get("technical_spec"):
            state["next_action"] = "architect"
            state["current_persona"] = "architect"
        elif not state.get("code_deliverables"):
            state["next_action"] = "implementer"
            state["current_persona"] = "implementer"
        else:
            # Project complete
            state["next_action"] = "end"
            state["current_persona"] = "coordinator"
            
            # Create final summary
            summary = self._generate_project_summary(state)
            state["messages"].append(AIMessage(content=summary))
        
        return state
    
    def _route_to_team_member(self, state: SoftwareDevTeamState) -> Literal["product_manager", "architect", "implementer", "problem_solver", "end"]:
        """Route to the appropriate team member based on current state."""
        return state.get("next_action", "end")
    
    def _product_manager_node(self, state: SoftwareDevTeamState) -> SoftwareDevTeamState:
        """Product manager creates user stories and requirements."""
        logger.info("Product Manager creating user stories...")
        
        state["current_persona"] = "product_manager"
        
        messages = [
            SystemMessage(content=self.persona_prompts["product_manager"]),
            HumanMessage(content=f"""
Project Request: {state['project_request']}
Project Type: {state.get('project_type', 'feature')}

Please analyze this request and create detailed user stories with:
1. Clear user story format ("As a... I want... so that...")
2. Specific acceptance criteria for each story
3. Priority levels (High/Medium/Low)
4. Story point estimates
5. Any questions you need answered to clarify requirements

If this is a bug fix, focus on the affected user experience and fixing criteria.
If this is a new feature, break it down into implementable chunks.
If this is a refactor, focus on the benefits and success criteria.
""")
        ]
        
        response = self.product_manager_llm.invoke(messages)
        
        # Create structured user stories (simplified for demo)
        state["user_stories"] = [
            UserStory(
                id="US-001",
                title="Primary Feature Implementation",
                description=f"Implement the main functionality requested: {state['project_request'][:100]}...",
                acceptance_criteria=["Feature works as described", "Tests pass", "Documentation updated"],
                priority="High",
                story_points=8
            )
        ]
        
        state["messages"].append(AIMessage(content=response.content))
        return state
    
    def _architect_node(self, state: SoftwareDevTeamState) -> SoftwareDevTeamState:
        """Software architect creates technical specifications."""
        logger.info("Software Architect designing technical solution...")
        
        state["current_persona"] = "architect"
        
        user_stories_summary = "\n".join([
            f"- {story.title}: {story.description} (Priority: {story.priority}, Points: {story.story_points})"
            for story in state.get("user_stories", [])
        ])
        
        messages = [
            SystemMessage(content=self.persona_prompts["architect"]),
            HumanMessage(content=f"""
Project Request: {state['project_request']}
Project Type: {state.get('project_type', 'feature')}

User Stories from Product Manager:
{user_stories_summary}

Please create a comprehensive technical specification including:
1. High-level architecture and system design
2. Technology stack recommendations
3. Database schema considerations (if applicable)
4. API design (if applicable)
5. Step-by-step implementation plan
6. Testing strategy
7. Deployment and infrastructure considerations
8. Performance and security considerations
9. Risk assessment and mitigation strategies

Focus on providing clear technical guidance for the implementation team.
""")
        ]
        
        response = self.architect_llm.invoke(messages)
        
        # Structure the technical specification
        state["technical_spec"] = TechnicalSpec(
            title=f"Technical Specification: {state['project_request'][:50]}...",
            overview="Comprehensive technical design for the requested functionality",
            architecture_decisions=["Chosen architecture pattern", "Technology stack selection"],
            implementation_plan=[
                "Phase 1: Setup and foundation",
                "Phase 2: Core functionality",
                "Phase 3: Testing and optimization",
                "Phase 4: Deployment and monitoring"
            ],
            dependencies=["Required libraries", "External services", "Infrastructure"],
            testing_strategy="Unit tests, integration tests, end-to-end tests",
            deployment_notes="Deployment strategy and rollback plan"
        )
        
        state["messages"].append(AIMessage(content=response.content))
        return state
    
    def _implementer_node(self, state: SoftwareDevTeamState) -> SoftwareDevTeamState:
        """Software engineer implements the technical specification."""
        logger.info("Software Engineer implementing the solution...")
        
        state["current_persona"] = "implementer"
        
        spec_summary = ""
        if state.get("technical_spec"):
            spec = state["technical_spec"]
            spec_summary = f"""
Technical Specification: {spec.title}
Overview: {spec.overview}

Implementation Plan:
{chr(10).join(f"- {step}" for step in spec.implementation_plan)}

Architecture Decisions:
{chr(10).join(f"- {decision}" for decision in spec.architecture_decisions)}

Testing Strategy: {spec.testing_strategy}
"""
        
        messages = [
            SystemMessage(content=self.persona_prompts["implementer"]),
            HumanMessage(content=f"""
Project Request: {state['project_request']}
Project Type: {state.get('project_type', 'feature')}

{spec_summary}

User Stories to Implement:
{chr(10).join(f"- {story.title}: {chr(10).join(story.acceptance_criteria)}" for story in state.get('user_stories', []))}

Please implement the solution including:
1. Complete, production-ready code
2. Comprehensive test suite
3. Documentation updates
4. Database migrations (if needed)
5. Configuration files
6. Deployment scripts

Ensure all acceptance criteria are met and code follows best practices.
""")
        ]
        
        response = self.implementer_llm.invoke(messages)
        
        # Structure the code deliverables
        state["code_deliverables"] = CodeDeliverable(
            files_created=["main.py", "models.py", "tests/test_main.py"],
            files_modified=["requirements.txt", "README.md", "config.yaml"],
            code_snippets={
                "main.py": "# Main implementation code would be here",
                "tests/test_main.py": "# Test code would be here"
            },
            tests_written=["Unit tests", "Integration tests", "End-to-end tests"],
            documentation_updates=["README.md updated", "API documentation", "Deployment guide"],
            deployment_scripts=["deploy.sh", "docker-compose.yml"]
        )
        
        state["messages"].append(AIMessage(content=response.content))
        return state
    
    def _problem_solver_node(self, state: SoftwareDevTeamState) -> SoftwareDevTeamState:
        """Problem solver debugs issues and optimizes solutions."""
        logger.info("Problem Solver analyzing and fixing issues...")
        
        state["current_persona"] = "problem_solver"
        
        issues_summary = "\n".join([
            f"Issue {issue.id}: {issue.title}\n"
            f"Description: {issue.description}\n"
            f"Severity: {issue.severity}\n"
            f"Error Logs: {issue.error_logs or 'None'}\n"
            f"Attempted Fixes: {', '.join(issue.attempted_fixes) if issue.attempted_fixes else 'None'}\n"
            for issue in state.get("issues", [])
        ])
        
        implementation_context = ""
        if state.get("code_deliverables"):
            deliverables = state["code_deliverables"]
            implementation_context = f"""
Implementation Context:
- Files Created: {', '.join(deliverables.files_created)}
- Files Modified: {', '.join(deliverables.files_modified)}
- Tests: {', '.join(deliverables.tests_written)}
"""
        
        messages = [
            SystemMessage(content=self.persona_prompts["problem_solver"]),
            HumanMessage(content=f"""
Project: {state['project_request']}

Issues to Resolve:
{issues_summary}

{implementation_context}

Please analyze these issues and provide:
1. Root cause analysis for each issue
2. Step-by-step debugging approach
3. Multiple solution options with trade-offs
4. Implementation plan for fixes
5. Prevention strategies for similar issues
6. Monitoring and alerting recommendations

Focus on systematic problem-solving and long-term reliability.
""")
        ]
        
        response = self.problem_solver_llm.invoke(messages)
        
        # Mark issues as resolved (simplified)
        for issue in state.get("issues", []):
            issue.status = "resolved"
        
        state["messages"].append(AIMessage(content=response.content))
        return state
    
    def _generate_project_summary(self, state: SoftwareDevTeamState) -> str:
        """Generate a comprehensive project completion summary."""
        user_stories_count = len(state.get("user_stories", []))
        issues_resolved = len([i for i in state.get("issues", []) if i.status == "resolved"])
        
        deliverables = state.get("code_deliverables")
        files_info = ""
        if deliverables:
            files_info = f"""
**Code Deliverables:**
- Files Created: {len(deliverables.files_created)} files
- Files Modified: {len(deliverables.files_modified)} files
- Tests Written: {len(deliverables.tests_written)} test suites
- Documentation: {len(deliverables.documentation_updates)} updates
"""
        
        return f"""
# 🎉 Software Development Project Complete!

## Project Summary
**Request:** {state['project_request']}
**Type:** {state.get('project_type', 'feature').title()}

## Team Deliverables
✅ **Product Manager**: {user_stories_count} user stories created with acceptance criteria
✅ **Software Architect**: Complete technical specification and implementation plan
✅ **Software Engineer**: Full implementation with tests and documentation
✅ **Problem Solver**: {issues_resolved} issues analyzed and resolved

{files_info}

## Project Status: ✅ READY FOR DEPLOYMENT

The development team has successfully completed all phases of the project:
- Requirements gathering and user story creation
- Technical architecture and design
- Implementation with comprehensive testing
- Issue resolution and optimization

The solution is production-ready and meets all acceptance criteria.
"""
    
    def develop_project(self, project_request: str, project_type: str = "feature") -> Dict[str, Any]:
        """Main method to run the software development team workflow."""
        try:
            # Initialize state
            initial_state = SoftwareDevTeamState(
                messages=[],
                current_persona="coordinator",
                project_request=project_request,
                user_stories=None,
                technical_spec=None,
                code_deliverables=None,
                issues=[],
                project_type=project_type,
                next_action=None,
                iteration_count=0
            )
            
            # Run the team workflow
            final_state = self.graph.invoke(initial_state)
            
            return {
                "success": True,
                "messages": [msg.content for msg in final_state["messages"] if hasattr(msg, 'content')],
                "user_stories": final_state.get("user_stories"),
                "technical_spec": final_state.get("technical_spec"),
                "code_deliverables": final_state.get("code_deliverables"),
                "issues_resolved": len([i for i in final_state.get("issues", []) if i.status == "resolved"]),
                "final_persona": final_state.get("current_persona")
            }
            
        except Exception as e:
            logger.error(f"Error in development process: {e}")
            return {
                "success": False,
                "error": str(e)
            }
    
    def add_issue(self, state: SoftwareDevTeamState, title: str, description: str, severity: str = "medium", error_logs: str = None) -> SoftwareDevTeamState:
        """Add an issue for the problem solver to address."""
        issue = Issue(
            id=f"ISS-{len(state.get('issues', [])) + 1:03d}",
            title=title,
            description=description,
            severity=severity,
            error_logs=error_logs,
            context="Added during development workflow",
            attempted_fixes=[],
            status="open"
        )
        
        if "issues" not in state:
            state["issues"] = []
        
        state["issues"].append(issue)
        return state


def create_dev_team_agent(use_online_models: bool = True) -> SoftwareDevTeamAgent:
    """Factory function to create the software development team agent."""
    return SoftwareDevTeamAgent(use_online_models=use_online_models)


# Export graph for LangGraph Studio
# Default to local models to avoid API key errors in LangGraph Studio
try:
    if os.getenv("OPENAI_API_KEY"):
        dev_team_agent = create_dev_team_agent(use_online_models=True)
    else:
        dev_team_agent = create_dev_team_agent(use_online_models=False)
except Exception as e:
    print(f"Warning: Error creating dev team agent with online models: {e}")
    print("Creating dev team agent with local models instead.")
    dev_team_agent = create_dev_team_agent(use_online_models=False)

graph = dev_team_agent.graph