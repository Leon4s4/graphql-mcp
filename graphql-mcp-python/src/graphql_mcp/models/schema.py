"""Schema-related types and models."""

from datetime import datetime
from typing import Any, Dict, List, Optional, Union
from pydantic import Field

from .base import BaseModel, TypeKind


class TypeReference(BaseModel):
    """GraphQL type reference with recursive structure."""
    kind: TypeKind
    name: Optional[str] = None
    description: Optional[str] = None
    of_type: Optional['TypeReference'] = None
    
    def __str__(self) -> str:
        """String representation of the type."""
        if self.kind == TypeKind.NON_NULL:
            return f"{self.of_type}!" if self.of_type else "Unknown!"
        elif self.kind == TypeKind.LIST:
            return f"[{self.of_type}]" if self.of_type else "[Unknown]"
        else:
            return self.name or "Unknown"


class ArgumentInfo(BaseModel):
    """GraphQL argument information."""
    name: str
    description: Optional[str] = None
    type: Optional[TypeReference] = None
    default_value: Optional[Any] = None
    is_required: bool = False


class EnumValueInfo(BaseModel):
    """GraphQL enum value information."""
    name: str
    description: Optional[str] = None
    is_deprecated: bool = False
    deprecation_reason: Optional[str] = None


class InputFieldInfo(BaseModel):
    """GraphQL input field information."""
    name: str
    description: Optional[str] = None
    type: Optional[TypeReference] = None
    default_value: Optional[Any] = None
    is_required: bool = False


class FieldInfo(BaseModel):
    """GraphQL field information."""
    name: str
    description: Optional[str] = None
    type: Optional[TypeReference] = None
    args: List[ArgumentInfo] = Field(default_factory=list)
    is_deprecated: bool = False
    deprecation_reason: Optional[str] = None


class DirectiveInfo(BaseModel):
    """GraphQL directive information."""
    name: str
    description: Optional[str] = None
    locations: List[str] = Field(default_factory=list)
    args: List[ArgumentInfo] = Field(default_factory=list)
    is_repeatable: bool = False


class GraphQlTypeInfo(BaseModel):
    """Complete GraphQL type information."""
    kind: TypeKind
    name: str
    description: Optional[str] = None
    fields: List[FieldInfo] = Field(default_factory=list)
    input_fields: List[InputFieldInfo] = Field(default_factory=list)
    interfaces: List[TypeReference] = Field(default_factory=list)
    enum_values: List[EnumValueInfo] = Field(default_factory=list)
    possible_types: List[TypeReference] = Field(default_factory=list)


class SchemaInfo(BaseModel):
    """GraphQL schema information."""
    query_type: Optional[TypeReference] = None
    mutation_type: Optional[TypeReference] = None
    subscription_type: Optional[TypeReference] = None
    types: List[GraphQlTypeInfo] = Field(default_factory=list)
    directives: List[DirectiveInfo] = Field(default_factory=list)
    last_modified: datetime = Field(default_factory=datetime.utcnow)
    version: str = ""


class SchemaContext(BaseModel):
    """Schema context information."""
    endpoint_name: str = ""
    schema_version: str = ""
    available_types: List[str] = Field(default_factory=list)
    available_operations: List[str] = Field(default_factory=list)
    supports_mutations: bool = False
    supports_subscriptions: bool = False


class SchemaField(BaseModel):
    """Schema field with usage information."""
    name: str
    type: str
    description: Optional[str] = None
    is_deprecated: bool = False
    usage_count: int = 0
    last_used: Optional[datetime] = None


class SchemaMetadata(BaseModel):
    """Schema metadata and statistics."""
    total_types: int = 0
    total_fields: int = 0
    total_operations: int = 0
    complexity_score: int = 0
    last_introspected: datetime = Field(default_factory=datetime.utcnow)
    cache_key: str = ""


class SchemaIntrospectionData(BaseModel):
    """Complete schema introspection data."""
    schema: SchemaInfo
    metadata: SchemaMetadata = Field(default_factory=SchemaMetadata)
    raw_sdl: Optional[str] = None
    introspection_query: str = ""
    timestamp: datetime = Field(default_factory=datetime.utcnow)


class SchemaAnalysis(BaseModel):
    """Schema analysis results."""
    endpoint_name: str = ""
    schema_quality: str = "Unknown"
    issues: List[str] = Field(default_factory=list)
    recommendations: List[str] = Field(default_factory=list)
    complexity_metrics: Dict[str, int] = Field(default_factory=dict)
    type_distribution: Dict[str, int] = Field(default_factory=dict)
    deprecated_fields: List[str] = Field(default_factory=list)


class SchemaComparisonResult(BaseModel):
    """Schema comparison results."""
    endpoint_a: str = ""
    endpoint_b: str = ""
    are_compatible: bool = False
    breaking_changes: List[str] = Field(default_factory=list)
    new_features: List[str] = Field(default_factory=list)
    deprecated_features: List[str] = Field(default_factory=list)
    similarity_score: float = 0.0
    migration_complexity: str = "Unknown"


class TypeRelationships(BaseModel):
    """Type relationships analysis."""
    type_name: str
    direct_dependencies: List[str] = Field(default_factory=list)
    reverse_dependencies: List[str] = Field(default_factory=list)
    circular_dependencies: List[str] = Field(default_factory=list)
    depth_level: int = 0


class TypeRelationshipsResult(BaseModel):
    """Complete type relationships analysis."""
    endpoint_name: str = ""
    relationships: List[TypeRelationships] = Field(default_factory=list)
    circular_dependency_count: int = 0
    max_depth: int = 0
    complexity_score: int = 0


# Resolve forward references
TypeReference.model_rebuild()