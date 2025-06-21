"""GraphQL MCP Tools

MCP tools for GraphQL operations and management.
"""

import logging
from typing import List
from mcp.server import Server

logger = logging.getLogger(__name__)


async def register_all_tools(server: Server) -> None:
    """Register all MCP tools with the server."""
    try:
        # Import and register tool modules
        from . import combined_operations
        from . import endpoint_management
        from . import schema_tools
        from . import query_tools
        from . import development_tools
        from . import utility_tools
        from . import code_migration
        
        # Register tools from each module
        await combined_operations.register_tools(server)
        await endpoint_management.register_tools(server)
        await schema_tools.register_tools(server)
        await query_tools.register_tools(server)
        await development_tools.register_tools(server)
        await utility_tools.register_tools(server)
        await code_migration.register_tools(server)
        
        logger.info("All tools registered successfully")
        
    except Exception as e:
        logger.error(f"Failed to register tools: {e}")
        raise


__all__ = ["register_all_tools"]