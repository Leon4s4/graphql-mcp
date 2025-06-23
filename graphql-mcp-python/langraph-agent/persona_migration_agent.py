"""Persona-based LangGraph agent for software migration tasks."""

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
class Requirement:
    """Represents a software requirement."""
    id: str
    description: str
    priority: str
    acceptance_criteria: List[str]
    user_story: str


@dataclass
class TechnicalDesign:
    """Represents a technical design specification."""
    title: str
    overview: str
    implementation_steps: List[str]
    assumptions: List[str]
    dependencies: List[str]
    risk_factors: List[str]


@dataclass
class Implementation:
    """Represents an implementation result."""
    code: str
    files_created: List[str]
    files_modified: List[str]
    tests_written: bool
    documentation_updated: bool


@dataclass
class Problem:
    """Represents a problem that needs solving."""
    description: str
    error_details: Optional[str]
    context: str
    attempted_solutions: List[str]


class PersonaAgentState(TypedDict):
    """State for the persona-based migration agent."""
    messages: Annotated[List[Any], add_messages]
    current_persona: str
    user_request: str
    requirements: Optional[List[Requirement]]
    technical_design: Optional[TechnicalDesign]
    implementation: Optional[Implementation]
    problems: List[Problem]
    docs_directory: str
    next_action: Optional[str]


class PersonaMigrationAgent:
    """Persona-based LangGraph agent for software migrations."""
    
    def __init__(self, use_online_models: bool = True):
        """Initialize the agent with OpenAI or local LLM configuration."""
        if use_online_models:
            # Check if API keys are available
            import os
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
                            model="gpt-4o-mini",  # Fallback to GPT-4o-mini
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
                    model="llama-3.1-8b-instruct",  # Good for structured tasks
                    temperature=0.1
                )
                
                # Architect: Reasoning-capable model for complex design
                self.architect_llm = ChatOpenAI(
                    **base_config,
                    model="deepseek-coder-6.7b-instruct",  # Strong reasoning for architecture
                    temperature=0.1
                )
                
                # Implementer: Code-focused model
                self.implementer_llm = ChatOpenAI(
                    **base_config,
                    model="codellama-13b-instruct",  # Specialized for code generation
                    temperature=0.1
                )
                
                # Problem Solver: Creative problem-solving model
                self.problem_solver_llm = ChatOpenAI(
                    **base_config,
                    model="mistral-7b-instruct",  # Good at creative problem solving
                    temperature=0.2
                )
            except Exception as e:
                print(f"Warning: Could not initialize specialized local models: {e}")
                print("Using single local model for all personas.")
                
                # Fallback to single local model for all personas
                fallback_llm = ChatOpenAI(
                    **base_config,
                    model="gemma-3",  # Generic fallback model
                    temperature=0.1
                )
                
                self.product_manager_llm = fallback_llm
                self.architect_llm = fallback_llm
                self.implementer_llm = fallback_llm
                self.problem_solver_llm = fallback_llm
        
        # Persona system prompts
        self.persona_prompts = {
            "product_manager": """You are a product manager for this application. Your task is to turn user requirements into product requirements documents (PRDs) that include user stories for new features. Add acceptance criteria. If you don't have enough information, ask me questions about the feature. 

When creating PRDs:
- Focus on functional rather than technical requirements
- Create clear user stories with "As a [user], I want [feature] so that [benefit]" format
- Define specific acceptance criteria for each requirement
- Prioritize requirements (High/Medium/Low)
- Avoid implementation details - focus on WHAT, not HOW

You should be focused and avoid going off track. Ask clarifying questions if needed.""",
            
            "architect": """You are a software architect for this application. Your product manager has provided requirements outlining the functional needs for a new feature. Your task is to design the technical implementation and ensure all acceptance criteria are met. 

Create a step-by-step technical specification that includes:
- High-level system architecture overview
- Implementation steps in logical order
- Dependencies and prerequisites
- Risk factors and mitigation strategies
- Performance and scalability considerations
- Security implications
- Testing strategy

DO NOT INCLUDE SOURCE CODE in your design. Focus on the technical approach and architecture. If you need to make assumptions, state them clearly. If anything is unclear, ask questions about the requirements or implementation constraints.""",
            
            "implementer": """You are a software engineer tasked with implementing features based on technical specifications. You must follow the architect's design precisely and complete all implementation steps.

Your responsibilities:
- Follow the technical specification exactly
- Write clean, maintainable code following project conventions
- Implement all required functionality
- Write appropriate tests
- Update documentation as needed
- Verify all acceptance criteria are met

After finishing implementation:
1. Review your work against the technical specification
2. Ensure all steps are complete
3. If any steps are missing, implement them
4. Repeat until all requirements are fully implemented

You should be focused and methodical. Ask questions if the specification is unclear before starting implementation.""",
            
            "problem_solver": """You are a problem-solving specialist focused on investigating and fixing issues in software systems. You excel at debugging, root cause analysis, and finding creative solutions to complex problems.

Your approach:
- Analyze the problem systematically
- Gather relevant context and error information
- Identify potential root causes
- Propose and test solutions
- Think creatively about alternative approaches
- Verify fixes actually resolve the issue

You are particularly good at:
- Debugging complex technical issues
- Finding edge cases and corner cases
- Understanding system interactions
- Proposing workarounds when direct fixes aren't possible
- Identifying underlying architectural problems

Focus on getting to the root cause rather than just addressing symptoms."""
        }
        
        # Create the agent graph
        self.graph = self._create_graph()
    
    def _create_graph(self) -> StateGraph:
        """Create the persona-based workflow graph."""
        workflow = StateGraph(PersonaAgentState)
        
        # Add persona nodes
        workflow.add_node("product_manager", self._product_manager_node)
        workflow.add_node("architect", self._architect_node)
        workflow.add_node("implementer", self._implementer_node)
        workflow.add_node("problem_solver", self._problem_solver_node)
        workflow.add_node("coordinator", self._coordinator_node)
        
        # Add routing logic
        workflow.add_edge(START, "coordinator")
        workflow.add_conditional_edges(
            "coordinator",
            self._route_to_persona,
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
    
    def _coordinator_node(self, state: PersonaAgentState) -> PersonaAgentState:
        """Coordinator node that determines next steps in the workflow."""
        logger.info("Coordinating workflow...")
        
        # Determine next action based on current state
        if not state.get("requirements"):
            state["next_action"] = "product_manager"
            state["current_persona"] = "product_manager"
        elif not state.get("technical_design"):
            state["next_action"] = "architect"
            state["current_persona"] = "architect"
        elif not state.get("implementation"):
            state["next_action"] = "implementer"
            state["current_persona"] = "implementer"
        elif state.get("problems"):
            state["next_action"] = "problem_solver"
            state["current_persona"] = "problem_solver"
        else:
            state["next_action"] = "end"
            state["current_persona"] = "coordinator"
            
            # Create final summary
            summary = """
# Migration Process Complete

All personas have completed their tasks:
✅ Product Manager: Requirements gathered and documented
✅ Software Architect: Technical design created
✅ Implementer: Code implementation completed
✅ Problem Solver: Issues resolved (if any)

The migration is ready for deployment and testing.
"""
            state["messages"].append(AIMessage(content=summary))
        
        return state
    
    def _route_to_persona(self, state: PersonaAgentState) -> Literal["product_manager", "architect", "implementer", "problem_solver", "end"]:
        """Route to the appropriate persona based on current state."""
        return state.get("next_action", "end")
    
    def _product_manager_node(self, state: PersonaAgentState) -> PersonaAgentState:
        """Product manager persona node for requirements gathering."""
        logger.info("Product Manager analyzing requirements...")
        
        state["current_persona"] = "product_manager"
        
        messages = [
            SystemMessage(content=self.persona_prompts["product_manager"]),
            HumanMessage(content=f"""
User Request: {state['user_request']}

Please analyze this request and create a Product Requirements Document (PRD). Focus on:
1. Understanding the user's needs
2. Creating clear user stories
3. Defining acceptance criteria
4. Prioritizing requirements

If you need more information, ask specific questions.
""")
        ]
        
        response = self.product_manager_llm.invoke(messages)
        
        # Extract and structure requirements (simplified)
        # In a real implementation, you'd parse the LLM response more thoroughly
        state["requirements"] = [
            Requirement(
                id="REQ-001",
                description="Migration from current system to new architecture",
                priority="High",
                acceptance_criteria=["System maintains current functionality", "Performance is maintained or improved"],
                user_story="As a developer, I want to migrate the system so that it's more maintainable"
            )
        ]
        
        state["messages"].append(AIMessage(content=response.content))
        
        return state
    
    def _architect_node(self, state: PersonaAgentState) -> PersonaAgentState:
        """Software architect persona node for technical design."""
        logger.info("Software Architect creating technical design...")
        
        state["current_persona"] = "architect"
        
        requirements_summary = "\n".join([
            f"- {req.description} (Priority: {req.priority})" 
            for req in state.get("requirements", [])
        ])
        
        messages = [
            SystemMessage(content=self.persona_prompts["architect"]),
            HumanMessage(content=f"""
Requirements from Product Manager:
{requirements_summary}

Original User Request: {state['user_request']}

Please create a comprehensive technical specification for implementing these requirements. Include:
1. High-level architecture overview
2. Step-by-step implementation plan
3. Dependencies and prerequisites
4. Risk assessment and mitigation
5. Testing strategy

Do NOT include actual code - focus on the technical approach and design.
""")
        ]
        
        response = self.architect_llm.invoke(messages)
        
        # Structure the technical design
        state["technical_design"] = TechnicalDesign(
            title="Migration Technical Specification",
            overview="Technical design for system migration",
            implementation_steps=[
                "Analyze current system architecture",
                "Design new system architecture", 
                "Create migration plan",
                "Implement migration incrementally",
                "Test and validate migration"
            ],
            assumptions=["Current system is well-documented", "Migration can be done incrementally"],
            dependencies=["Access to source code", "Test environment available"],
            risk_factors=["Data loss during migration", "Downtime during transition"]
        )
        
        state["messages"].append(AIMessage(content=response.content))
        
        return state
    
    def _implementer_node(self, state: PersonaAgentState) -> PersonaAgentState:
        """Implementer persona node for code implementation."""
        logger.info("Implementer executing technical design...")
        
        state["current_persona"] = "implementer"
        
        design_summary = ""
        if state.get("technical_design"):
            design = state["technical_design"]
            design_summary = f"""
Technical Design Overview: {design.overview}

Implementation Steps:
{chr(10).join(f"- {step}" for step in design.implementation_steps)}

Dependencies: {', '.join(design.dependencies)}
Risk Factors: {', '.join(design.risk_factors)}
"""
        
        messages = [
            SystemMessage(content=self.persona_prompts["implementer"]),
            HumanMessage(content=f"""
Technical Specification from Architect:
{design_summary}

Original User Request: {state['user_request']}

Please implement the technical specification. Follow the implementation steps precisely and:
1. Create/modify necessary files
2. Write appropriate tests
3. Update documentation
4. Verify all acceptance criteria are met

Provide the implementation code and describe what files were created/modified.
""")
        ]
        
        response = self.implementer_llm.invoke(messages)
        
        # Structure the implementation result
        state["implementation"] = Implementation(
            code=response.content,
            files_created=["migration_script.py", "config.json"],
            files_modified=["main.py", "requirements.txt"],
            tests_written=True,
            documentation_updated=True
        )
        
        state["messages"].append(AIMessage(content=response.content))
        
        return state
    
    def _problem_solver_node(self, state: PersonaAgentState) -> PersonaAgentState:
        """Problem solver persona node for debugging and issue resolution."""
        logger.info("Problem Solver analyzing issues...")
        
        state["current_persona"] = "problem_solver"
        
        problems_summary = "\n".join([
            f"Problem: {problem.description}\n"
            f"Error: {problem.error_details or 'No error details'}\n"
            f"Context: {problem.context}\n"
            f"Attempted Solutions: {', '.join(problem.attempted_solutions) if problem.attempted_solutions else 'None'}\n"
            for problem in state.get("problems", [])
        ])
        
        messages = [
            SystemMessage(content=self.persona_prompts["problem_solver"]),
            HumanMessage(content=f"""
Issues to Resolve:
{problems_summary}

Implementation Context:
{state.get('implementation', {}).get('code', 'No implementation available')}

Original User Request: {state['user_request']}

Please analyze these issues and provide solutions. Focus on:
1. Root cause analysis
2. Specific fixes or workarounds
3. Prevention strategies for similar issues
4. Verification steps to ensure the fix works
""")
        ]
        
        response = self.problem_solver_llm.invoke(messages)
        
        # Clear resolved problems
        state["problems"] = []
        
        state["messages"].append(AIMessage(content=response.content))
        
        return state
    
    @tool
    def _create_prd_file(self, content: str, filename: str, directory: str = "docs") -> Dict[str, Any]:
        """Create a PRD markdown file in the docs directory."""
        try:
            os.makedirs(directory, exist_ok=True)
            filepath = os.path.join(directory, filename)
            
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(content)
            
            return {
                "success": True,
                "filepath": filepath,
                "message": f"PRD created at {filepath}"
            }
        except Exception as e:
            return {
                "success": False,
                "error": str(e)
            }
    
    @tool
    def _create_techspec_file(self, content: str, filename: str, directory: str = "docs") -> Dict[str, Any]:
        """Create a technical specification markdown file in the docs directory."""
        try:
            os.makedirs(directory, exist_ok=True)
            filepath = os.path.join(directory, filename)
            
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(content)
            
            return {
                "success": True,
                "filepath": filepath,
                "message": f"Technical specification created at {filepath}"
            }
        except Exception as e:
            return {
                "success": False,
                "error": str(e)
            }
    
    def run_migration(self, user_request: str, docs_directory: str = "docs") -> Dict[str, Any]:
        """Main method to run the persona-based migration process."""
        try:
            # Initialize state
            initial_state = PersonaAgentState(
                messages=[],
                current_persona="coordinator",
                user_request=user_request,
                requirements=None,
                technical_design=None,
                implementation=None,
                problems=[],
                docs_directory=docs_directory,
                next_action=None
            )
            
            # Run the graph
            final_state = self.graph.invoke(initial_state)
            
            return {
                "success": True,
                "messages": [msg.content for msg in final_state["messages"] if hasattr(msg, 'content')],
                "requirements": final_state.get("requirements"),
                "technical_design": final_state.get("technical_design"),
                "implementation": final_state.get("implementation"),
                "final_persona": final_state.get("current_persona")
            }
            
        except Exception as e:
            logger.error(f"Error in migration process: {e}")
            return {
                "success": False,
                "error": str(e)
            }
    
    def add_problem(self, state: PersonaAgentState, description: str, error_details: str = None, context: str = "") -> PersonaAgentState:
        """Add a problem to be solved by the problem solver persona."""
        problem = Problem(
            description=description,
            error_details=error_details,
            context=context,
            attempted_solutions=[]
        )
        
        if "problems" not in state:
            state["problems"] = []
        
        state["problems"].append(problem)
        return state


def create_persona_agent(use_online_models: bool = True) -> PersonaMigrationAgent:
    """Factory function to create the persona-based migration agent."""
    return PersonaMigrationAgent(use_online_models=use_online_models)


# Export graph for LangGraph Studio
# Default to local models to avoid API key errors in LangGraph Studio
try:
    import os
    if os.getenv("OPENAI_API_KEY"):
        agent = create_persona_agent(use_online_models=True)
    else:
        agent = create_persona_agent(use_online_models=False)
except Exception as e:
    print(f"Warning: Error creating agent with online models: {e}")
    print("Creating agent with local models instead.")
    agent = create_persona_agent(use_online_models=False)

graph = agent.graph