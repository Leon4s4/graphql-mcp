"""Endpoint management tools for GraphQL MCP server."""

import logging
from typing import Dict, Optional
from mcp.server import Server

from .base_tool import BaseTool
from ..models.core import ComprehensiveResponse, GraphQlEndpointInfo

logger = logging.getLogger(__name__)


class EndpointManagementTool(BaseTool):
    """Endpoint management operations."""
    
    async def register_endpoint(
        self,
        name: str,
        url: str,
        headers: Optional[Dict[str, str]] = None,
        allow_mutations: bool = False,
        tool_prefix: str = ""
    ) -> ComprehensiveResponse:
        """Register a new GraphQL endpoint."""
        try:
            endpoint_info = GraphQlEndpointInfo(
                name=name,
                url=url,
                headers=headers or {},
                allow_mutations=allow_mutations,
                tool_prefix=tool_prefix
            )
            
            success = await self.registry.register_endpoint(endpoint_info)
            
            if success:
                return self.create_response(
                    success=True,
                    data={"endpoint_name": name, "status": "registered"},
                    suggestions=["Endpoint registered successfully"]
                )
            else:
                return self.create_response(
                    success=False,
                    errors=["Failed to register endpoint"],
                    suggestions=["Check endpoint configuration", "Verify URL accessibility"]
                )
                
        except Exception as e:
            return self.handle_error(e, "Endpoint registration")


async def register_tools(server: Server) -> None:
    """Register endpoint management tools."""
    try:
        @server.call_tool()
        async def register_endpoint(
            name: str,
            url: str,
            headers: Optional[Dict[str, str]] = None,
            allow_mutations: bool = False,
            tool_prefix: str = ""
        ) -> ComprehensiveResponse:
            """
            Register a GraphQL endpoint and automatically generate MCP tools.
            
            Args:
                name: Endpoint name
                url: GraphQL endpoint URL
                headers: Custom headers for authentication
                allow_mutations: Whether to support mutations
                tool_prefix: Prefix for generated tools
            """
            tool = EndpointManagementTool()
            return await tool.register_endpoint(
                name=name,
                url=url,
                headers=headers,
                allow_mutations=allow_mutations,
                tool_prefix=tool_prefix
            )
        
        @server.call_tool()
        async def get_all_endpoints() -> ComprehensiveResponse:
            """Get all registered endpoints with their configuration."""
            tool = EndpointManagementTool()
            endpoints = await tool.registry.get_all_endpoints()
            
            return tool.create_response(
                success=True,
                data={"endpoints": [ep.dict() for ep in endpoints.values()]},
                suggestions=["Use these endpoints with other tools"]
            )
        
        logger.info("Endpoint management tools registered")
        
    except Exception as e:
        logger.error(f"Failed to register endpoint management tools: {e}")
        raise