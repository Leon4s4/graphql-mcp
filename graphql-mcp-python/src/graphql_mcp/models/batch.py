"""Batch operation types and models."""

from datetime import datetime, timedelta
from typing import Any, Dict, List, Optional
from pydantic import Field

from .base import BaseModel, ExecutionMode


class BatchQueryRequest(BaseModel):
    """Individual batch query request."""
    name: str = ""
    query: str = ""
    variables: Optional[Dict[str, Any]] = None
    operation_name: Optional[str] = None
    endpoint: str = ""
    timeout_seconds: int = 30


class BatchOperationResult(BaseModel):
    """Result of a single batch operation."""
    name: str = ""
    success: bool = False
    data: Optional[Any] = None
    error: Optional[str] = None
    execution_time: timedelta = Field(default_factory=timedelta)
    index: int = 0
    endpoint: str = ""
    query: Optional[str] = None
    variables: Optional[Dict[str, Any]] = None
    was_retried: bool = False
    retry_attempts: int = 0
    status_code: Optional[int] = None
    headers: Dict[str, str] = Field(default_factory=dict)


class BatchSummary(BaseModel):
    """Summary of batch operation execution."""
    total_operations: int = 0
    successful_operations: int = 0
    failed_operations: int = 0
    total_execution_time: timedelta = Field(default_factory=timedelta)
    average_execution_time: float = 0.0
    execution_mode: ExecutionMode = ExecutionMode.SEQUENTIAL
    started_at: datetime = Field(default_factory=datetime.utcnow)
    completed_at: Optional[datetime] = None
    continue_on_error: bool = True


class BatchExecutionResponse(BaseModel):
    """Complete batch execution response."""
    results: List[BatchOperationResult] = Field(default_factory=list)
    summary: BatchSummary = Field(default_factory=BatchSummary)
    errors: List[str] = Field(default_factory=list)
    warnings: List[str] = Field(default_factory=list)
    metadata: Dict[str, Any] = Field(default_factory=dict)


class BatchQueryResult(BaseModel):
    """Result of batch query execution."""
    batch_id: str = ""
    operations: List[BatchOperationResult] = Field(default_factory=list)
    summary: BatchSummary = Field(default_factory=BatchSummary) 
    aggregated_data: Optional[Dict[str, Any]] = None
    cross_operation_analysis: Optional[Dict[str, Any]] = None