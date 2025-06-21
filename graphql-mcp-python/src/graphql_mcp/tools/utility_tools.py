"""Utility tools for GraphQL MCP server."""

import logging
from mcp.server import Server

from .base_tool import BaseTool
from ..models.core import ComprehensiveResponse

logger = logging.getLogger(__name__)


async def register_tools(server: Server) -> None:
    """Register utility tools."""
    try:
        @server.call_tool()
        async def format_query(
            query: str,
            indent_size: int = 2,
            sort_fields: bool = False
        ) -> ComprehensiveResponse:
            """
            Format GraphQL queries with proper indentation and structure.
            
            Args:
                query: GraphQL query to format
                indent_size: Indentation size
                sort_fields: Whether to sort fields
            """
            tool = BaseTool()
            return tool.create_response(
                success=True,
                data={"message": "Query formatting placeholder"},
                suggestions=["Full implementation needed"]
            )
        
        logger.info("Utility tools registered")
        
    except Exception as e:
        logger.error(f"Failed to register utility tools: {e}")
        raise