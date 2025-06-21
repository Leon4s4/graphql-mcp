"""Workflow prompts for GraphQL development."""

import logging
from mcp.server import Server

logger = logging.getLogger(__name__)


async def register_prompts(server: Server) -> None:
    """Register workflow prompts."""
    try:
        @server.get_prompt()
        async def development_workflow_prompt(
            phase: str,
            team_size: str = "small",
            tech_stack: str = "node"
        ):
            """Generate development workflow guidance."""
            return f"""
# GraphQL Development Workflow

**Phase:** {phase}
**Team Size:** {team_size}
**Tech Stack:** {tech_stack}

Provide guidance for the {phase} phase of GraphQL development:

1. Key activities and deliverables
2. Best practices for {team_size} teams
3. {tech_stack}-specific recommendations
4. Quality gates and checkpoints
5. Tools and resources needed

Include practical examples and actionable steps.
"""
        
        logger.info("Workflow prompts registered")
        
    except Exception as e:
        logger.error(f"Failed to register workflow prompts: {e}")
        raise