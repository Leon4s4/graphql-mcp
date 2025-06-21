"""Configuration for the C# to GraphQL migration agent."""

import os
from dataclasses import dataclass
from typing import Optional


@dataclass
class AgentConfig:
    """Configuration class for the LangGraph agent."""
    
    # LM Studio configuration
    llm_base_url: str = "http://localhost:1234/v1"
    model_name: str = "gemma-3"  # Default to Gemma 3
    api_key: str = "lm-studio"  # Any string works for local models
    temperature: float = 0.1
    max_tokens: Optional[int] = None
    
    # Agent behavior configuration
    enable_detailed_analysis: bool = True
    enable_optimization_suggestions: bool = True  
    enable_migration_planning: bool = True
    enable_performance_analysis: bool = True
    
    # GraphQL specific settings
    default_query_depth_limit: int = 5
    enable_fragments: bool = True
    enable_variables: bool = True
    
    @classmethod
    def from_env(cls) -> "AgentConfig":
        """Create configuration from environment variables."""
        return cls(
            llm_base_url=os.getenv("LLM_BASE_URL", "http://localhost:1234/v1"),
            model_name=os.getenv("MODEL_NAME", "gemma-3"),
            api_key=os.getenv("LLM_API_KEY", "lm-studio"),
            temperature=float(os.getenv("LLM_TEMPERATURE", "0.1")),
            max_tokens=int(os.getenv("LLM_MAX_TOKENS")) if os.getenv("LLM_MAX_TOKENS") else None,
            enable_detailed_analysis=os.getenv("ENABLE_DETAILED_ANALYSIS", "true").lower() == "true",
            enable_optimization_suggestions=os.getenv("ENABLE_OPTIMIZATION_SUGGESTIONS", "true").lower() == "true",
            enable_migration_planning=os.getenv("ENABLE_MIGRATION_PLANNING", "true").lower() == "true",
            enable_performance_analysis=os.getenv("ENABLE_PERFORMANCE_ANALYSIS", "true").lower() == "true",
            default_query_depth_limit=int(os.getenv("QUERY_DEPTH_LIMIT", "5")),
            enable_fragments=os.getenv("ENABLE_FRAGMENTS", "true").lower() == "true",
            enable_variables=os.getenv("ENABLE_VARIABLES", "true").lower() == "true"
        )
    
    def validate(self) -> None:
        """Validate the configuration."""
        if not self.llm_base_url:
            raise ValueError("LLM base URL must be provided")
        
        if not self.model_name:
            raise ValueError("Model name must be provided")
        
        if self.temperature < 0 or self.temperature > 2:
            raise ValueError("Temperature must be between 0 and 2")
        
        if self.max_tokens is not None and self.max_tokens <= 0:
            raise ValueError("Max tokens must be positive")
        
        if self.default_query_depth_limit <= 0:
            raise ValueError("Query depth limit must be positive")


# Default configuration instance
DEFAULT_CONFIG = AgentConfig()

# Environment-based configuration
ENV_CONFIG = AgentConfig.from_env()