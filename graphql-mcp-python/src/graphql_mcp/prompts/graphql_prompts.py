"""GraphQL prompts for development workflows."""

import logging
from mcp.server import Server

logger = logging.getLogger(__name__)


async def register_prompts(server: Server) -> None:
    """Register GraphQL prompts."""
    try:
        @server.list_prompts()
        async def get_graphql_prompts():
            """Get available GraphQL prompts."""
            return [
                {
                    "name": "generate_query",
                    "description": "Generate optimized GraphQL queries based on requirements",
                    "arguments": [
                        {"name": "data_requirement", "description": "Data requirements"},
                        {"name": "endpoint_name", "description": "GraphQL endpoint name"},
                        {"name": "performance", "description": "Performance level"},
                        {"name": "include_related", "description": "Include related data"}
                    ]
                },
                {
                    "name": "analyze_schema", 
                    "description": "Comprehensive schema analysis",
                    "arguments": [
                        {"name": "focus", "description": "Analysis focus area"},
                        {"name": "endpoint_name", "description": "GraphQL endpoint name"},
                        {"name": "specific_type", "description": "Specific type to analyze"}
                    ]
                }
            ]
        
        @server.get_prompt()
        async def generate_query_prompt(
            data_requirement: str,
            endpoint_name: str,
            performance: str = "normal",
            include_related: bool = True
        ):
            """Generate a prompt for GraphQL query generation."""
            return f"""
# GraphQL Query Generation

Generate an optimized GraphQL query for the following requirements:

**Data Requirement:** {data_requirement}
**Endpoint:** {endpoint_name}
**Performance Level:** {performance}
**Include Related Data:** {include_related}

Please create:
1. An efficient GraphQL query with proper structure
2. Fragments for reusable field sets if applicable
3. Variables for dynamic values
4. Usage examples
5. Performance considerations

Focus on {performance} performance optimization and follow GraphQL best practices.
"""
        
        logger.info("GraphQL prompts registered")
        
    except Exception as e:
        logger.error(f"Failed to register GraphQL prompts: {e}")
        raise