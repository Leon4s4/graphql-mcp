"""Performance-related types and models."""

from datetime import datetime, timedelta
from typing import Any, Dict, List, Optional
from pydantic import Field

from .base import BaseModel


class MemoryUsage(BaseModel):
    """Memory usage information."""
    used_bytes: int = 0
    allocated_bytes: int = 0
    efficiency_ratio: float = 0.0
    peak_usage: int = 0
    gc_collections: int = 0


class DataFreshness(BaseModel):
    """Data freshness information."""
    as_of: datetime = Field(default_factory=datetime.utcnow)
    is_stale: bool = False
    age: timedelta = Field(default_factory=timedelta)
    cache_key: str = ""
    ttl_seconds: int = 0


class ComplexityMetrics(BaseModel):
    """Query complexity metrics."""
    depth_score: int = 0
    field_count: int = 0
    alias_count: int = 0
    fragment_count: int = 0
    directive_count: int = 0
    estimated_cost: int = 0
    max_allowed_depth: int = 10
    max_allowed_complexity: int = 1000


class ExecutionMetadata(BaseModel):
    """Execution metadata and performance information."""
    execution_time_ms: int = 0
    complexity_score: int = 0
    depth_score: int = 0
    field_count: int = 0
    cache_hit: bool = False
    endpoint_name: str = ""
    operation_name: Optional[str] = None
    variables_count: int = 0
    fragment_count: int = 0
    start_time: datetime = Field(default_factory=datetime.utcnow)
    end_time: Optional[datetime] = None
    server_time_ms: Optional[int] = None
    network_time_ms: Optional[int] = None


class PerformanceRecommendation(BaseModel):
    """Individual performance recommendation."""
    category: str = ""
    priority: str = "Medium"
    description: str = ""
    impact: str = ""
    effort: str = ""
    example: Optional[str] = None
    related_fields: List[str] = Field(default_factory=list)


class PerformanceRecommendations(BaseModel):
    """Collection of performance recommendations."""
    query_optimization: List[PerformanceRecommendation] = Field(default_factory=list)
    schema_improvements: List[PerformanceRecommendation] = Field(default_factory=list)
    caching_strategies: List[PerformanceRecommendation] = Field(default_factory=list)
    general_tips: List[PerformanceRecommendation] = Field(default_factory=list)
    estimated_improvement: str = ""
    priority_score: int = 0


class PerformanceProfile(BaseModel):
    """Performance profile for operations."""
    operation_name: str = ""
    average_time_ms: float = 0.0
    min_time_ms: int = 0
    max_time_ms: int = 0
    execution_count: int = 0
    error_rate: float = 0.0
    cache_hit_rate: float = 0.0
    complexity_trend: str = "Stable"
    last_executed: datetime = Field(default_factory=datetime.utcnow)


class PerformanceMetadata(BaseModel):
    """Performance metadata and statistics."""
    schema_size: int = 0
    processing_time_ms: int = 0
    cache_hit: bool = False
    last_updated: datetime = Field(default_factory=datetime.utcnow)
    memory_usage: Optional[MemoryUsage] = None
    recommendations: List[PerformanceRecommendation] = Field(default_factory=list)
    data_freshness: Optional[DataFreshness] = None
    complexity_metrics: Optional[ComplexityMetrics] = None


class PerformanceAnalysisResult(BaseModel):
    """Results of performance analysis."""
    endpoint_name: str = ""
    overall_score: int = 0
    bottlenecks: List[str] = Field(default_factory=list)
    optimization_opportunities: List[str] = Field(default_factory=list)
    performance_trends: Dict[str, Any] = Field(default_factory=dict)
    recommendations: PerformanceRecommendations = Field(default_factory=PerformanceRecommendations)
    benchmark_data: Dict[str, float] = Field(default_factory=dict)


class PerformanceComparisonResult(BaseModel):
    """Performance comparison between operations or endpoints."""
    baseline_name: str = ""
    comparison_name: str = ""
    performance_delta: Dict[str, float] = Field(default_factory=dict)
    winner: str = ""
    improvement_suggestions: List[str] = Field(default_factory=list)
    detailed_metrics: Dict[str, Any] = Field(default_factory=dict)


class PerformanceProfilingResult(BaseModel):
    """Results of performance profiling."""
    operation_profiles: List[PerformanceProfile] = Field(default_factory=list)
    hotspots: List[str] = Field(default_factory=list)
    optimization_targets: List[str] = Field(default_factory=list)
    overall_health: str = "Unknown"
    profiling_duration: timedelta = Field(default_factory=timedelta)


class PerformanceCorrelationResult(BaseModel):
    """Performance correlation analysis."""
    primary_metric: str = ""
    correlations: Dict[str, float] = Field(default_factory=dict)
    significant_factors: List[str] = Field(default_factory=list)
    recommendations: List[str] = Field(default_factory=list)
    r_squared: float = 0.0


class PredictiveAnalyticsResult(BaseModel):
    """Predictive analytics for performance."""
    predicted_metrics: Dict[str, float] = Field(default_factory=dict)
    confidence_intervals: Dict[str, tuple] = Field(default_factory=dict)
    trend_direction: str = "Stable"
    risk_factors: List[str] = Field(default_factory=list)
    recommendations: List[str] = Field(default_factory=list)
    forecast_horizon: str = "1 week"