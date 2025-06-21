"""Query-related types and models."""

from datetime import datetime
from typing import Any, Dict, List, Optional
from pydantic import Field

from .base import BaseModel, QueryComplexityRating


class QueryExample(BaseModel):
    """GraphQL query example with metadata."""
    name: str = ""
    description: str = ""
    query: str = ""
    variables: Optional[Dict[str, Any]] = None
    expected_result: Optional[Dict[str, Any]] = None
    complexity_score: int = 0
    category: str = ""
    tags: List[str] = Field(default_factory=list)


class PaginationInfo(BaseModel):
    """Pagination information."""
    has_next_page: bool = False
    has_previous_page: bool = False
    start_cursor: Optional[str] = None
    end_cursor: Optional[str] = None
    total_count: Optional[int] = None
    page_size: int = 10


class PaginationHints(BaseModel):
    """Pagination hints and recommendations."""
    recommended_page_size: int = 10
    supports_cursor_pagination: bool = False
    supports_offset_pagination: bool = False
    pagination_fields: List[str] = Field(default_factory=list)
    best_practices: List[str] = Field(default_factory=list)


class PaginationRecommendation(BaseModel):
    """Pagination implementation recommendations."""
    strategy: str = ""
    page_size: int = 10
    sort_fields: List[str] = Field(default_factory=list)
    filter_options: List[str] = Field(default_factory=list)
    example_query: str = ""
    performance_notes: List[str] = Field(default_factory=list)


class QuerySuggestions(BaseModel):
    """Query optimization suggestions."""
    optimization_hints: List[str] = Field(default_factory=list)
    related_queries: List[QueryExample] = Field(default_factory=list)
    field_suggestions: List[str] = Field(default_factory=list)
    pagination_hints: Optional[PaginationHints] = None
    alternative_approaches: List[str] = Field(default_factory=list)


class QueryComplexityInfo(BaseModel):
    """Query complexity analysis."""
    rating: QueryComplexityRating = QueryComplexityRating.SIMPLE
    score: int = 0
    depth: int = 0
    field_count: int = 0
    estimated_execution_time: int = 0
    bottlenecks: List[str] = Field(default_factory=list)
    suggestions: List[str] = Field(default_factory=list)


class QueryStatistics(BaseModel):
    """Query usage statistics."""
    total_executions: int = 0
    successful_executions: int = 0
    failed_executions: int = 0
    average_execution_time: float = 0.0
    first_seen: datetime = Field(default_factory=datetime.utcnow)
    last_seen: datetime = Field(default_factory=datetime.utcnow)
    popularity_score: float = 0.0


class QueryValidationResult(BaseModel):
    """Query validation results."""
    is_valid: bool = False
    errors: List[str] = Field(default_factory=list)
    warnings: List[str] = Field(default_factory=list)
    suggestions: List[str] = Field(default_factory=list)
    complexity_info: Optional[QueryComplexityInfo] = None
    estimated_cost: int = 0


class QueryDebuggingResult(BaseModel):
    """Query debugging information."""
    query: str = ""
    issues_found: List[str] = Field(default_factory=list)
    resolution_steps: List[str] = Field(default_factory=list)
    related_documentation: List[str] = Field(default_factory=list)
    example_fixes: List[str] = Field(default_factory=list)
    debugging_tips: List[str] = Field(default_factory=list)


class MutationExample(BaseModel):
    """GraphQL mutation example."""
    name: str = ""
    description: str = ""
    mutation: str = ""
    variables: Optional[Dict[str, Any]] = None
    expected_result: Optional[Dict[str, Any]] = None
    required_permissions: List[str] = Field(default_factory=list)
    side_effects: List[str] = Field(default_factory=list)


class FieldUsage(BaseModel):
    """Field usage information."""
    field_path: str = ""
    usage_count: int = 0
    last_used: datetime = Field(default_factory=datetime.utcnow)
    performance_impact: str = "Low"
    deprecation_warnings: List[str] = Field(default_factory=list)


class FieldUsageStats(BaseModel):
    """Field usage statistics."""
    total_fields: int = 0
    used_fields: int = 0
    unused_fields: int = 0
    frequently_used_fields: List[FieldUsage] = Field(default_factory=list)
    rarely_used_fields: List[FieldUsage] = Field(default_factory=list)


class FieldUsageAnalysisResult(BaseModel):
    """Field usage analysis results."""
    endpoint_name: str = ""
    analysis_period: str = ""
    field_stats: FieldUsageStats = Field(default_factory=FieldUsageStats)
    optimization_opportunities: List[str] = Field(default_factory=list)
    deprecation_candidates: List[str] = Field(default_factory=list)
    performance_insights: List[str] = Field(default_factory=list)


class UsageExamples(BaseModel):
    """Usage examples for GraphQL operations."""
    queries: List[QueryExample] = Field(default_factory=list)
    mutations: List[MutationExample] = Field(default_factory=list)
    fragments: List[str] = Field(default_factory=list)
    best_practices: List[str] = Field(default_factory=list)
    common_patterns: List[str] = Field(default_factory=list)


class UsageStatistics(BaseModel):
    """Usage statistics for GraphQL operations."""
    total_operations: int = 0
    successful_operations: int = 0
    failed_operations: int = 0
    average_response_time: float = 0.0
    peak_usage_time: Optional[datetime] = None
    most_used_fields: List[str] = Field(default_factory=list)
    error_patterns: List[str] = Field(default_factory=list)


class UsageTrendsResult(BaseModel):
    """Usage trends analysis."""
    endpoint_name: str = ""
    time_period: str = ""
    growth_rate: float = 0.0
    trending_operations: List[str] = Field(default_factory=list)
    declining_operations: List[str] = Field(default_factory=list)
    usage_patterns: Dict[str, Any] = Field(default_factory=dict)
    predictions: Dict[str, float] = Field(default_factory=dict)