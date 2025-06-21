"""Smart response service for enhanced GraphQL responses."""

import asyncio
import logging
from typing import Any, Dict, List, Optional
from datetime import datetime

from ..models.core import GraphQlExecutionResponse, ComprehensiveResponse
from ..models.schema import SchemaInfo
from ..models.performance import ExecutionMetadata, PerformanceRecommendations
from ..models.query import QuerySuggestions
from ..models.security import SecurityAnalysis
from .json_helpers import JsonHelpers

logger = logging.getLogger(__name__)


class SmartResponseService:
    """Service for creating enhanced GraphQL responses with metadata."""
    
    def __init__(self):
        self.logger = logging.getLogger(self.__class__.__name__)
    
    async def create_execution_response_async(
        self,
        query_id: str,
        data: Optional[Any] = None,
        errors: Optional[List[Any]] = None,
        endpoint_name: str = "",
        schema_info: Optional[SchemaInfo] = None,
        include_suggestions: bool = True,
        include_performance: bool = True,
        include_security: bool = True
    ) -> GraphQlExecutionResponse:
        """Create enhanced execution response with metadata."""
        try:
            # Create base metadata
            metadata = ExecutionMetadata(
                endpoint_name=endpoint_name,
                operation_name=query_id,
                cache_hit=False,
                execution_time_ms=0,
                start_time=datetime.utcnow()
            )
            
            # Create response components in parallel
            tasks = []
            
            if include_suggestions:
                tasks.append(self._generate_query_suggestions(data, errors, schema_info))
            
            if include_performance:
                tasks.append(self._generate_performance_recommendations(metadata))
            
            if include_security:
                tasks.append(self._generate_security_analysis(data, schema_info))
            
            # Wait for all tasks to complete
            results = await asyncio.gather(*tasks, return_exceptions=True)
            
            # Extract results
            suggestions = None
            performance = None
            security = None
            
            result_index = 0
            if include_suggestions:
                suggestions = results[result_index] if not isinstance(results[result_index], Exception) else None
                result_index += 1
            
            if include_performance:
                performance = results[result_index] if not isinstance(results[result_index], Exception) else None
                result_index += 1
            
            if include_security:
                security = results[result_index] if not isinstance(results[result_index], Exception) else None
                result_index += 1
            
            # Convert errors to ExecutionError objects
            execution_errors = []
            if errors:
                from ..models.core import ExecutionError
                for error in errors:
                    if isinstance(error, dict):
                        execution_errors.append(ExecutionError(
                            message=error.get("message", "Unknown error"),
                            path=error.get("path"),
                            extensions=error.get("extensions"),
                            category="GraphQL"
                        ))
                    else:
                        execution_errors.append(ExecutionError(
                            message=str(error),
                            category="General"
                        ))
            
            return GraphQlExecutionResponse(
                query_id=query_id,
                data=data,
                errors=execution_errors,
                metadata=metadata,
                suggestions=suggestions,
                performance=performance,
                security=security
            )
            
        except Exception as e:
            logger.error(f"Failed to create execution response: {e}")
            # Return basic response on error
            return GraphQlExecutionResponse(
                query_id=query_id,
                data=data,
                errors=[],
                metadata=ExecutionMetadata(endpoint_name=endpoint_name)
            )
    
    async def create_schema_introspection_response_async(
        self,
        schema_info: SchemaInfo,
        endpoint_name: str,
        include_analysis: bool = True,
        format_type: str = "detailed"
    ) -> ComprehensiveResponse:
        """Create enhanced schema introspection response."""
        try:
            from ..models.core import ResponseMetadata
            
            # Format schema data based on type
            formatted_data = await self._format_schema_data(schema_info, format_type)
            
            # Generate analysis if requested
            analysis_data = {}
            if include_analysis:
                analysis_data = await self._analyze_schema(schema_info, endpoint_name)
            
            # Create metadata
            metadata = ResponseMetadata(
                timestamp=datetime.utcnow(),
                endpoint_name=endpoint_name,
                processing_time_ms=0,
                cache_hit=False
            )
            
            return ComprehensiveResponse(
                success=True,
                data={
                    "schema": formatted_data,
                    "analysis": analysis_data,
                    "endpoint_name": endpoint_name,
                    "format": format_type
                },
                metadata=metadata,
                suggestions=[
                    "Use specific type queries for detailed information",
                    "Consider caching schema data for better performance",
                    "Review deprecated fields and plan migrations"
                ]
            )
            
        except Exception as e:
            logger.error(f"Failed to create schema response: {e}")
            return ComprehensiveResponse(
                success=False,
                errors=[str(e)]
            )
    
    async def _generate_query_suggestions(
        self,
        data: Optional[Any],
        errors: Optional[List[Any]],
        schema_info: Optional[SchemaInfo]
    ) -> Optional[QuerySuggestions]:
        """Generate query optimization suggestions."""
        try:
            suggestions = QuerySuggestions()
            
            # Add general optimization hints
            if data:
                suggestions.optimization_hints.extend([
                    "Consider using fragments for repeated field sets",
                    "Use field aliases to avoid naming conflicts",
                    "Implement pagination for large result sets"
                ])
            
            if errors:
                suggestions.optimization_hints.extend([
                    "Check field names against schema",
                    "Verify required arguments are provided",
                    "Review query syntax and structure"
                ])
            
            # Add schema-specific suggestions
            if schema_info:
                suggestions.field_suggestions.extend([
                    "id", "name", "description", "createdAt", "updatedAt"
                ])
            
            return suggestions
            
        except Exception as e:
            logger.error(f"Failed to generate query suggestions: {e}")
            return None
    
    async def _generate_performance_recommendations(
        self,
        metadata: ExecutionMetadata
    ) -> Optional[PerformanceRecommendations]:
        """Generate performance recommendations."""
        try:
            from ..models.performance import PerformanceRecommendation
            
            recommendations = PerformanceRecommendations()
            
            # Add general performance recommendations
            recommendations.query_optimization.append(
                PerformanceRecommendation(
                    category="Query Structure",
                    priority="Medium",
                    description="Use specific field selection to reduce payload size",
                    impact="Reduces network overhead and parsing time",
                    effort="Low"
                )
            )
            
            recommendations.caching_strategies.append(
                PerformanceRecommendation(
                    category="Caching",
                    priority="High",
                    description="Implement query result caching for frequently accessed data",
                    impact="Significantly reduces server load and response time",
                    effort="Medium"
                )
            )
            
            return recommendations
            
        except Exception as e:
            logger.error(f"Failed to generate performance recommendations: {e}")
            return None
    
    async def _generate_security_analysis(
        self,
        data: Optional[Any],
        schema_info: Optional[SchemaInfo]
    ) -> Optional[SecurityAnalysis]:
        """Generate security analysis."""
        try:
            analysis = SecurityAnalysis()
            
            # Basic security checks
            if data and isinstance(data, dict):
                # Check for potentially sensitive fields
                sensitive_fields = ["password", "token", "secret", "key", "email"]
                for field in sensitive_fields:
                    if any(field in str(key).lower() for key in data.keys()):
                        analysis.has_sensitive_data = True
                        analysis.security_warnings.append(
                            f"Response contains potentially sensitive field: {field}"
                        )
            
            # Add general security recommendations
            analysis.security_recommendations.extend([
                "Use HTTPS for all GraphQL endpoints",
                "Implement query depth limiting",
                "Add query complexity analysis",
                "Use authentication for sensitive operations"
            ])
            
            return analysis
            
        except Exception as e:
            logger.error(f"Failed to generate security analysis: {e}")
            return None
    
    async def _format_schema_data(
        self,
        schema_info: SchemaInfo,
        format_type: str
    ) -> Dict[str, Any]:
        """Format schema data based on requested type."""
        try:
            if format_type == "types":
                return {
                    "types": [
                        {
                            "name": type_info.name,
                            "kind": type_info.kind.value,
                            "description": type_info.description,
                            "fields_count": len(type_info.fields)
                        }
                        for type_info in schema_info.types
                    ]
                }
            elif format_type == "operations":
                operations = {"queries": [], "mutations": []}
                
                # Find query operations
                if schema_info.query_type:
                    query_type = next(
                        (t for t in schema_info.types if t.name == schema_info.query_type.name),
                        None
                    )
                    if query_type:
                        operations["queries"] = [
                            {
                                "name": field.name,
                                "description": field.description,
                                "args_count": len(field.args)
                            }
                            for field in query_type.fields
                        ]
                
                # Find mutation operations
                if schema_info.mutation_type:
                    mutation_type = next(
                        (t for t in schema_info.types if t.name == schema_info.mutation_type.name),
                        None
                    )
                    if mutation_type:
                        operations["mutations"] = [
                            {
                                "name": field.name,
                                "description": field.description,
                                "args_count": len(field.args)
                            }
                            for field in mutation_type.fields
                        ]
                
                return operations
            else:
                # Default detailed format
                return JsonHelpers.sanitize_for_json(schema_info.dict())
                
        except Exception as e:
            logger.error(f"Failed to format schema data: {e}")
            return {"error": str(e)}
    
    async def _analyze_schema(
        self,
        schema_info: SchemaInfo,
        endpoint_name: str
    ) -> Dict[str, Any]:
        """Analyze schema for insights."""
        try:
            analysis = {
                "endpoint_name": endpoint_name,
                "total_types": len(schema_info.types),
                "has_mutations": schema_info.mutation_type is not None,
                "has_subscriptions": schema_info.subscription_type is not None,
                "deprecated_fields": [],
                "type_distribution": {},
                "complexity_indicators": []
            }
            
            # Analyze type distribution
            for type_info in schema_info.types:
                kind = type_info.kind.value
                analysis["type_distribution"][kind] = analysis["type_distribution"].get(kind, 0) + 1
                
                # Check for deprecated fields
                for field in type_info.fields:
                    if field.is_deprecated:
                        analysis["deprecated_fields"].append(f"{type_info.name}.{field.name}")
            
            # Add complexity indicators
            if analysis["total_types"] > 100:
                analysis["complexity_indicators"].append("Large number of types")
            
            if len(analysis["deprecated_fields"]) > 10:
                analysis["complexity_indicators"].append("Many deprecated fields")
            
            return analysis
            
        except Exception as e:
            logger.error(f"Failed to analyze schema: {e}")
            return {"error": str(e)}