"""GraphQL MCP Prompts

Prompt generation for GraphQL development workflows.
"""

import logging
from mcp.server import Server

logger = logging.getLogger(__name__)


async def register_all_prompts(server: Server) -> None:
    """Register all prompts with the server."""
    try:
        # Import and register prompt modules
        from . import graphql_prompts
        from . import workflow_prompts
        
        # Register prompts from each module
        await graphql_prompts.register_prompts(server)
        await workflow_prompts.register_prompts(server)
        
        logger.info("All prompts registered successfully")
        
    except Exception as e:
        logger.error(f"Failed to register prompts: {e}")
        raise


__all__ = ["register_all_prompts"]