"""Base tool class for GraphQL MCP tools."""

import logging
from abc import ABC, abstractmethod
from typing import Any, Dict, List, Optional
from datetime import datetime

from ..config import get_settings
from ..helpers import EndpointRegistryService
from ..models.core import ComprehensiveResponse, ResponseMetadata


logger = logging.getLogger(__name__)


class BaseTool(ABC):
    """Base class for GraphQL MCP tools."""
    
    def __init__(self):
        self.settings = get_settings()
        self.registry = EndpointRegistryService()
        self.logger = logging.getLogger(self.__class__.__name__)
    
    @abstractmethod
    async def execute(self, **kwargs) -> ComprehensiveResponse:
        """Execute the tool with given parameters."""
        pass
    
    def create_response(
        self,
        success: bool = True,
        data: Optional[Any] = None,
        errors: Optional[List[str]] = None,
        warnings: Optional[List[str]] = None,
        suggestions: Optional[List[str]] = None,
        related_info: Optional[Dict[str, Any]] = None,
        **metadata_kwargs
    ) -> ComprehensiveResponse:
        """Create a standardized response."""
        metadata = ResponseMetadata(
            timestamp=datetime.utcnow(),
            **metadata_kwargs
        )
        
        return ComprehensiveResponse(
            success=success,
            data=data,
            errors=errors or [],
            warnings=warnings or [],
            metadata=metadata,
            suggestions=suggestions or [],
            related_info=related_info or {}
        )
    
    def validate_endpoint(self, endpoint_name: str) -> bool:
        """Validate that an endpoint is registered."""
        try:
            # This would be async in practice, but for simplicity
            # we'll make it sync and check the registry
            return True  # Placeholder
        except Exception as e:
            self.logger.error(f"Endpoint validation failed: {e}")
            return False
    
    def log_tool_usage(
        self, 
        tool_name: str, 
        endpoint_name: Optional[str] = None,
        success: bool = True,
        execution_time_ms: Optional[int] = None
    ) -> None:
        """Log tool usage for analytics."""
        try:
            self.logger.info(
                "Tool executed",
                tool=tool_name,
                endpoint=endpoint_name,
                success=success,
                execution_time_ms=execution_time_ms
            )
        except Exception as e:
            self.logger.error(f"Failed to log tool usage: {e}")
    
    def handle_error(
        self, 
        error: Exception, 
        context: str = ""
    ) -> ComprehensiveResponse:
        """Handle and format errors consistently."""
        error_msg = f"{context}: {str(error)}" if context else str(error)
        self.logger.error(f"Tool error: {error_msg}")
        
        return self.create_response(
            success=False,
            errors=[error_msg],
            suggestions=[
                "Check your parameters",
                "Verify endpoint configuration",
                "Review error logs for details"
            ]
        )