"""Settings and configuration for GraphQL MCP server."""

import os
from typing import Dict, Optional
from pydantic import Field
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    """Application settings with environment variable support."""
    
    # Server settings
    host: str = Field(default="localhost", description="Server host")
    port: int = Field(default=8080, description="Server port")
    debug: bool = Field(default=False, description="Debug mode")
    
    # HTTP client settings
    http_timeout: int = Field(default=30, description="HTTP request timeout in seconds")
    max_connections: int = Field(default=100, description="Maximum HTTP connections")
    
    # Cache settings
    cache_enabled: bool = Field(default=True, description="Enable caching")
    cache_type: str = Field(default="memory", description="Cache type: memory, redis, disk")
    cache_ttl: int = Field(default=3600, description="Cache TTL in seconds")
    redis_url: Optional[str] = Field(default=None, description="Redis URL for caching")
    cache_size_limit: int = Field(default=1000, description="Memory cache size limit")
    
    # Performance settings
    max_query_depth: int = Field(default=10, description="Maximum GraphQL query depth")
    max_query_complexity: int = Field(default=1000, description="Maximum query complexity")
    default_page_size: int = Field(default=10, description="Default pagination size")
    max_page_size: int = Field(default=100, description="Maximum pagination size")
    
    # Security settings
    rate_limit_enabled: bool = Field(default=True, description="Enable rate limiting")
    rate_limit_requests: int = Field(default=100, description="Rate limit requests per minute")
    allowed_origins: str = Field(default="*", description="CORS allowed origins")
    
    # Logging settings
    log_level: str = Field(default="INFO", description="Logging level")
    log_format: str = Field(default="json", description="Log format: json, text")
    
    # Tool settings
    enable_dynamic_tools: bool = Field(default=True, description="Enable dynamic tool generation")
    tool_cleanup_interval: int = Field(default=3600, description="Tool cleanup interval in seconds")
    max_endpoints: int = Field(default=50, description="Maximum registered endpoints")
    
    # Batch operation settings
    max_batch_size: int = Field(default=10, description="Maximum batch operation size")
    batch_timeout: int = Field(default=120, description="Batch operation timeout in seconds")
    parallel_batch_enabled: bool = Field(default=True, description="Enable parallel batch execution")
    
    class Config:
        env_prefix = "GRAPHQL_MCP_"
        env_file = ".env"
        case_sensitive = False


# Global settings instance
_settings: Optional[Settings] = None


def get_settings() -> Settings:
    """Get the global settings instance."""
    global _settings
    if _settings is None:
        _settings = Settings()
    return _settings


def update_settings(**kwargs) -> Settings:
    """Update settings with new values."""
    global _settings
    current_settings = get_settings()
    new_values = current_settings.dict()
    new_values.update(kwargs)
    _settings = Settings(**new_values)
    return _settings