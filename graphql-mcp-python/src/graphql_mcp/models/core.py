"""Core GraphQL types and models."""

from datetime import datetime
from typing import Any, Dict, List, Optional, Union
from pydantic import Field

from .base import BaseModel


class ErrorLocation(BaseModel):
    """Location of a GraphQL error."""
    line: int
    column: int


class GraphQlError(BaseModel):
    """GraphQL error details."""
    message: str
    locations: Optional[List[ErrorLocation]] = None
    path: Optional[List[Union[str, int]]] = None
    extensions: Optional[Dict[str, Any]] = None


class ExecutionError(BaseModel):
    """Execution error with additional metadata."""
    message: str
    path: Optional[List[Union[str, int]]] = None
    locations: Optional[List[ErrorLocation]] = None
    extensions: Optional[Dict[str, Any]] = None
    suggestions: List[str] = Field(default_factory=list)
    category: str = ""
    severity: str = "Error"


class ErrorInfo(BaseModel):
    """Error information container."""
    is_graphql_error: bool = False
    errors: List[GraphQlError] = Field(default_factory=list)


class GraphQlEndpointInfo(BaseModel):
    """GraphQL endpoint configuration."""
    name: str = ""
    url: str = ""
    headers: Dict[str, str] = Field(default_factory=dict)
    allow_mutations: bool = False
    tool_prefix: str = ""
    schema_content: Optional[str] = ""


class ExecutionResult(BaseModel):
    """Basic GraphQL execution result."""
    data: Optional[Any] = None
    errors: Optional[List[ExecutionError]] = None


class GraphQlExecutionResponse(BaseModel):
    """Enhanced GraphQL execution response with metadata."""
    query_id: str = ""
    data: Optional[Any] = None
    errors: List[ExecutionError] = Field(default_factory=list)
    metadata: Optional['ExecutionMetadata'] = None
    suggestions: Optional['QuerySuggestions'] = None
    schema_context: Optional['SchemaContext'] = None
    performance: Optional['PerformanceRecommendations'] = None
    security: Optional['SecurityAnalysis'] = None


class ResponseMetadata(BaseModel):
    """Response metadata for all GraphQL responses."""
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    processing_time_ms: int = 0
    cache_hit: bool = False
    endpoint_name: str = ""
    operation_name: Optional[str] = None
    complexity_score: int = 0
    depth_score: int = 0


class ComprehensiveResponse(BaseModel):
    """Comprehensive response wrapper for complex operations."""
    success: bool = True
    data: Optional[Any] = None
    errors: List[str] = Field(default_factory=list)
    warnings: List[str] = Field(default_factory=list)
    metadata: ResponseMetadata = Field(default_factory=ResponseMetadata)
    suggestions: List[str] = Field(default_factory=list)
    related_info: Dict[str, Any] = Field(default_factory=dict)


# Forward reference resolution
from .performance import ExecutionMetadata, PerformanceRecommendations
from .query import QuerySuggestions
from .schema import SchemaContext
from .security import SecurityAnalysis

GraphQlExecutionResponse.model_rebuild()