"""Endpoint registry service for managing GraphQL endpoints."""

import asyncio
import logging
from typing import Dict, List, Optional, Set
from collections import defaultdict
from datetime import datetime

from ..models.core import GraphQlEndpointInfo
from ..models.schema import SchemaInfo


logger = logging.getLogger(__name__)


class EndpointRegistryService:
    """
    Singleton service for managing GraphQL endpoint registration and tool lifecycle.
    Thread-safe endpoint and tool management with statistics and monitoring.
    """
    
    _instance: Optional['EndpointRegistryService'] = None
    _lock = asyncio.Lock()
    
    def __new__(cls) -> 'EndpointRegistryService':
        if cls._instance is None:
            cls._instance = super().__new__(cls)
            cls._instance._initialized = False
        return cls._instance
    
    def __init__(self):
        if not getattr(self, '_initialized', False):
            # Thread-safe collections for endpoint and tool management
            self._endpoints: Dict[str, GraphQlEndpointInfo] = {}
            self._endpoint_schemas: Dict[str, SchemaInfo] = {}
            self._tool_to_endpoint: Dict[str, str] = {}
            self._endpoint_to_tools: Dict[str, Set[str]] = defaultdict(set)
            self._endpoint_stats: Dict[str, Dict[str, any]] = defaultdict(dict)
            self._last_updated: Dict[str, datetime] = {}
            self._initialized = True
            logger.info("EndpointRegistryService initialized")
    
    async def register_endpoint(
        self, 
        endpoint_info: GraphQlEndpointInfo,
        schema_info: Optional[SchemaInfo] = None
    ) -> bool:
        """
        Register a GraphQL endpoint with optional schema information.
        
        Args:
            endpoint_info: Endpoint configuration
            schema_info: Optional schema information
            
        Returns:
            True if registration successful, False otherwise
        """
        async with self._lock:
            try:
                endpoint_name = endpoint_info.name
                
                # Store endpoint info
                self._endpoints[endpoint_name] = endpoint_info
                
                # Store schema info if provided
                if schema_info:
                    self._endpoint_schemas[endpoint_name] = schema_info
                
                # Initialize stats
                self._endpoint_stats[endpoint_name] = {
                    'registered_at': datetime.utcnow(),
                    'last_accessed': None,
                    'access_count': 0,
                    'tool_count': 0,
                    'schema_version': schema_info.version if schema_info else '',
                    'last_introspection': None,
                    'error_count': 0,
                    'success_count': 0,
                }
                
                self._last_updated[endpoint_name] = datetime.utcnow()
                
                logger.info(f"Registered endpoint: {endpoint_name} -> {endpoint_info.url}")
                return True
                
            except Exception as e:
                logger.error(f"Failed to register endpoint {endpoint_info.name}: {e}")
                return False
    
    async def unregister_endpoint(self, endpoint_name: str) -> bool:
        """
        Unregister an endpoint and clean up associated tools.
        
        Args:
            endpoint_name: Name of the endpoint to unregister
            
        Returns:
            True if unregistration successful, False otherwise
        """
        async with self._lock:
            try:
                if endpoint_name not in self._endpoints:
                    logger.warning(f"Endpoint not found: {endpoint_name}")
                    return False
                
                # Remove associated tools
                tools_to_remove = list(self._endpoint_to_tools.get(endpoint_name, set()))
                for tool_name in tools_to_remove:
                    await self._remove_tool(tool_name)
                
                # Remove endpoint data
                del self._endpoints[endpoint_name]
                self._endpoint_schemas.pop(endpoint_name, None)
                self._endpoint_stats.pop(endpoint_name, None)
                self._last_updated.pop(endpoint_name, None)
                self._endpoint_to_tools.pop(endpoint_name, None)
                
                logger.info(f"Unregistered endpoint: {endpoint_name}")
                return True
                
            except Exception as e:
                logger.error(f"Failed to unregister endpoint {endpoint_name}: {e}")
                return False
    
    async def get_endpoint(self, endpoint_name: str) -> Optional[GraphQlEndpointInfo]:
        """Get endpoint information by name."""
        async with self._lock:
            return self._endpoints.get(endpoint_name)
    
    async def get_all_endpoints(self) -> Dict[str, GraphQlEndpointInfo]:
        """Get all registered endpoints."""
        async with self._lock:
            return self._endpoints.copy()
    
    async def get_endpoint_schema(self, endpoint_name: str) -> Optional[SchemaInfo]:
        """Get cached schema information for an endpoint."""
        async with self._lock:
            return self._endpoint_schemas.get(endpoint_name)
    
    async def update_endpoint_schema(
        self, 
        endpoint_name: str, 
        schema_info: SchemaInfo
    ) -> bool:
        """Update cached schema information for an endpoint."""
        async with self._lock:
            try:
                if endpoint_name not in self._endpoints:
                    logger.warning(f"Endpoint not found: {endpoint_name}")
                    return False
                
                self._endpoint_schemas[endpoint_name] = schema_info
                self._last_updated[endpoint_name] = datetime.utcnow()
                
                # Update stats
                if endpoint_name in self._endpoint_stats:
                    self._endpoint_stats[endpoint_name]['schema_version'] = schema_info.version
                    self._endpoint_stats[endpoint_name]['last_introspection'] = datetime.utcnow()
                
                logger.info(f"Updated schema for endpoint: {endpoint_name}")
                return True
                
            except Exception as e:
                logger.error(f"Failed to update schema for endpoint {endpoint_name}: {e}")
                return False
    
    async def register_tool(self, tool_name: str, endpoint_name: str) -> bool:
        """Register a tool as associated with an endpoint."""
        async with self._lock:
            try:
                if endpoint_name not in self._endpoints:
                    logger.warning(f"Endpoint not found: {endpoint_name}")
                    return False
                
                # Store bidirectional mapping
                self._tool_to_endpoint[tool_name] = endpoint_name
                self._endpoint_to_tools[endpoint_name].add(tool_name)
                
                # Update stats
                if endpoint_name in self._endpoint_stats:
                    self._endpoint_stats[endpoint_name]['tool_count'] = len(
                        self._endpoint_to_tools[endpoint_name]
                    )
                
                logger.debug(f"Registered tool: {tool_name} -> {endpoint_name}")
                return True
                
            except Exception as e:
                logger.error(f"Failed to register tool {tool_name}: {e}")
                return False
    
    async def _remove_tool(self, tool_name: str) -> bool:
        """Remove a tool from the registry."""
        try:
            endpoint_name = self._tool_to_endpoint.get(tool_name)
            if endpoint_name:
                self._endpoint_to_tools[endpoint_name].discard(tool_name)
                
                # Update stats
                if endpoint_name in self._endpoint_stats:
                    self._endpoint_stats[endpoint_name]['tool_count'] = len(
                        self._endpoint_to_tools[endpoint_name]
                    )
            
            self._tool_to_endpoint.pop(tool_name, None)
            logger.debug(f"Removed tool: {tool_name}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to remove tool {tool_name}: {e}")
            return False
    
    async def get_endpoint_for_tool(self, tool_name: str) -> Optional[str]:
        """Get the endpoint name associated with a tool."""
        async with self._lock:
            return self._tool_to_endpoint.get(tool_name)
    
    async def get_tools_for_endpoint(self, endpoint_name: str) -> Set[str]:
        """Get all tools associated with an endpoint."""
        async with self._lock:
            return self._endpoint_to_tools.get(endpoint_name, set()).copy()
    
    async def get_endpoint_stats(self, endpoint_name: str) -> Optional[Dict[str, any]]:
        """Get statistics for a specific endpoint."""
        async with self._lock:
            return self._endpoint_stats.get(endpoint_name, {}).copy()
    
    async def get_all_stats(self) -> Dict[str, Dict[str, any]]:
        """Get statistics for all endpoints."""
        async with self._lock:
            return {
                name: stats.copy() 
                for name, stats in self._endpoint_stats.items()
            }
    
    async def record_endpoint_access(
        self, 
        endpoint_name: str, 
        success: bool = True
    ) -> None:
        """Record an access to an endpoint for statistics."""
        async with self._lock:
            if endpoint_name in self._endpoint_stats:
                stats = self._endpoint_stats[endpoint_name]
                stats['last_accessed'] = datetime.utcnow()
                stats['access_count'] = stats.get('access_count', 0) + 1
                
                if success:
                    stats['success_count'] = stats.get('success_count', 0) + 1
                else:
                    stats['error_count'] = stats.get('error_count', 0) + 1
    
    async def is_endpoint_registered(self, endpoint_name: str) -> bool:
        """Check if an endpoint is registered."""
        async with self._lock:
            return endpoint_name in self._endpoints
    
    async def get_endpoint_count(self) -> int:
        """Get the total number of registered endpoints."""
        async with self._lock:
            return len(self._endpoints)
    
    async def get_total_tool_count(self) -> int:
        """Get the total number of registered tools across all endpoints."""
        async with self._lock:
            return len(self._tool_to_endpoint)
    
    async def cleanup_stale_endpoints(self, max_age_hours: int = 24) -> int:
        """Remove endpoints that haven't been accessed recently."""
        async with self._lock:
            current_time = datetime.utcnow()
            stale_endpoints = []
            
            for endpoint_name, stats in self._endpoint_stats.items():
                last_accessed = stats.get('last_accessed')
                if last_accessed is None:
                    # Use registration time if never accessed
                    last_accessed = stats.get('registered_at', current_time)
                
                age_hours = (current_time - last_accessed).total_seconds() / 3600
                if age_hours > max_age_hours:
                    stale_endpoints.append(endpoint_name)
            
            # Remove stale endpoints
            for endpoint_name in stale_endpoints:
                await self.unregister_endpoint(endpoint_name)
            
            logger.info(f"Cleaned up {len(stale_endpoints)} stale endpoints")
            return len(stale_endpoints)
    
    def get_registry_summary(self) -> Dict[str, any]:
        """Get a summary of the registry state."""
        return {
            'total_endpoints': len(self._endpoints),
            'total_tools': len(self._tool_to_endpoint),
            'endpoints_with_schema': len(self._endpoint_schemas),
            'last_registry_update': max(self._last_updated.values()) if self._last_updated else None,
        }