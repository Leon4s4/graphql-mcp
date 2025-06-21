"""Base models and common types for GraphQL MCP server."""

from datetime import datetime, timedelta
from enum import Enum
from typing import Any, Dict, List, Optional, Union
from pydantic import BaseModel as PydanticBaseModel, Field, ConfigDict


class BaseModel(PydanticBaseModel):
    """Base model with common configuration."""
    
    model_config = ConfigDict(
        # Convert camelCase to snake_case automatically
        alias_generator=lambda field_name: ''.join(
            ['_' + c.lower() if c.isupper() and i > 0 else c.lower() 
             for i, c in enumerate(field_name)]
        ),
        # Allow population by field name and alias
        populate_by_name=True,
        # Validate assignment
        validate_assignment=True,
        # Use enum values
        use_enum_values=True,
        # Serialize by alias (camelCase for JSON output)
        by_alias=True,
    )


class TypeKind(str, Enum):
    """GraphQL type kinds."""
    SCALAR = "SCALAR"
    OBJECT = "OBJECT"
    INTERFACE = "INTERFACE"
    UNION = "UNION"
    ENUM = "ENUM"
    INPUT_OBJECT = "INPUT_OBJECT"
    LIST = "LIST"
    NON_NULL = "NON_NULL"


class QueryComplexityRating(str, Enum):
    """Query complexity ratings."""
    SIMPLE = "Simple"
    MODERATE = "Moderate"
    COMPLEX = "Complex"
    VERY_COMPLEX = "VeryComplex"


class ValidationSeverity(str, Enum):
    """Validation issue severity levels."""
    INFO = "Info"
    WARNING = "Warning"
    ERROR = "Error"
    CRITICAL = "Critical"


class AnalysisLevel(str, Enum):
    """Analysis depth levels."""
    BASIC = "basic"
    STANDARD = "standard"
    COMPREHENSIVE = "comprehensive"


class ExecutionMode(str, Enum):
    """Batch execution modes."""
    SEQUENTIAL = "sequential"
    PARALLEL = "parallel"


class WorkflowType(str, Enum):
    """GraphQL workflow types."""
    EXPLORE = "explore"
    QUERY = "query"
    DEVELOP = "develop"
    OPTIMIZE = "optimize"