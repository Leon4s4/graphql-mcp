"""Main entry point for GraphQL MCP server - Python implementation."""

import asyncio
import logging
import signal
import sys
from typing import Optional

import structlog
from mcp.server import Server
from mcp.server.stdio import stdio_server

from .config import get_settings
from .helpers import EndpointRegistryService
from .tools import register_all_tools
from .prompts import register_all_prompts


# Configure structured logging
def configure_logging(level: str = "INFO", format_type: str = "json") -> None:
    """Configure structured logging for the application."""
    log_level = getattr(logging, level.upper(), logging.INFO)
    
    if format_type.lower() == "json":
        structlog.configure(
            processors=[
                structlog.stdlib.filter_by_level,
                structlog.stdlib.add_logger_name,
                structlog.stdlib.add_log_level,
                structlog.stdlib.PositionalArgumentsFormatter(),
                structlog.processors.TimeStamper(fmt="iso"),
                structlog.processors.StackInfoRenderer(),
                structlog.processors.format_exc_info,
                structlog.processors.UnicodeDecoder(),
                structlog.processors.JSONRenderer()
            ],
            context_class=dict,
            logger_factory=structlog.stdlib.LoggerFactory(),
            wrapper_class=structlog.stdlib.BoundLogger,
            cache_logger_on_first_use=True,
        )
    else:
        structlog.configure(
            processors=[
                structlog.stdlib.filter_by_level,
                structlog.stdlib.add_logger_name,
                structlog.stdlib.add_log_level,
                structlog.stdlib.PositionalArgumentsFormatter(),
                structlog.processors.TimeStamper(fmt="%Y-%m-%d %H:%M:%S"),
                structlog.processors.StackInfoRenderer(),
                structlog.processors.format_exc_info,
                structlog.processors.UnicodeDecoder(),
                structlog.dev.ConsoleRenderer()
            ],
            context_class=dict,
            logger_factory=structlog.stdlib.LoggerFactory(),
            wrapper_class=structlog.stdlib.BoundLogger,
            cache_logger_on_first_use=True,
        )
    
    logging.basicConfig(level=log_level)


class GraphQLMcpServer:
    """GraphQL MCP Server implementation."""
    
    def __init__(self):
        self.settings = get_settings()
        self.server: Optional[Server] = None
        self.registry: Optional[EndpointRegistryService] = None
        self.logger = structlog.get_logger(__name__)
        
        # Configure logging
        configure_logging(
            level=self.settings.log_level,
            format_type=self.settings.log_format
        )
        
        self.logger.info(
            "GraphQL MCP Server initializing",
            version="1.0.0",
            debug=self.settings.debug
        )
    
    async def initialize(self) -> None:
        """Initialize the server and its components."""
        try:
            # Initialize the MCP server
            self.server = Server("graphql-mcp-python")
            
            # Initialize endpoint registry
            self.registry = EndpointRegistryService()
            
            # Register all tools
            await register_all_tools(self.server)
            
            # Register all prompts
            await register_all_prompts(self.server)
            
            self.logger.info(
                "Server initialization complete",
                tools_registered=len(self.server.list_tools()),
                prompts_registered=len(self.server.list_prompts())
            )
            
        except Exception as e:
            self.logger.error("Failed to initialize server", error=str(e))
            raise
    
    async def run(self) -> None:
        """Run the MCP server."""
        try:
            if not self.server:
                await self.initialize()
            
            self.logger.info("Starting GraphQL MCP server")
            
            # Run the server with stdio transport
            async with stdio_server(self.server) as (read_stream, write_stream):
                await self.server.run(
                    read_stream, 
                    write_stream,
                    self.settings.dict()
                )
                
        except KeyboardInterrupt:
            self.logger.info("Server shutdown requested")
        except Exception as e:
            self.logger.error("Server error", error=str(e))
            raise
        finally:
            await self.cleanup()
    
    async def cleanup(self) -> None:
        """Cleanup server resources."""
        try:
            self.logger.info("Cleaning up server resources")
            
            # Cleanup endpoint registry if needed
            if self.registry:
                # Could add cleanup methods to registry
                pass
            
            self.logger.info("Server cleanup complete")
            
        except Exception as e:
            self.logger.error("Error during cleanup", error=str(e))


async def main() -> None:
    """Main entry point for the GraphQL MCP server."""
    server = GraphQLMcpServer()
    
    # Setup signal handlers for graceful shutdown
    def signal_handler(signum, frame):
        server.logger.info(f"Received signal {signum}, initiating shutdown")
        sys.exit(0)
    
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)
    
    try:
        await server.run()
    except Exception as e:
        server.logger.error("Server failed", error=str(e))
        sys.exit(1)


def sync_main() -> None:
    """Synchronous entry point for the server."""
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\nServer shutdown complete")
    except Exception as e:
        print(f"Server error: {e}")
        sys.exit(1)


if __name__ == "__main__":
    sync_main()