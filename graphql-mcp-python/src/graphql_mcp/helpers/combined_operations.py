"""Combined operations service for GraphQL MCP server."""

import asyncio
import logging
from typing import Any, Dict, List, Optional
from datetime import datetime

from ..models.core import GraphQlEndpointInfo, GraphQlExecutionResponse
from ..models.schema import SchemaInfo
from ..models.batch import BatchOperationResult, BatchExecutionResponse
from ..models.performance import ExecutionMetadata
from .http_client import GraphQLHttpClient
from .schema_helper import GraphQLSchemaHelper
from .endpoint_registry import EndpointRegistryService

logger = logging.getLogger(__name__)


class CombinedOperationsService:
    """Stateless service for combined GraphQL operations."""
    
    def __init__(self):
        self.http_client = GraphQLHttpClient()
        self.schema_helper = GraphQLSchemaHelper()
        self.registry = EndpointRegistryService()
    
    async def get_schema_async(
        self,
        endpoint_name: str,
        max_depth: int = 3,
        use_cache: bool = True
    ) -> Optional[SchemaInfo]:
        """Get schema with caching support."""
        try:
            # Check cache first
            if use_cache:
                cached_schema = await self.registry.get_endpoint_schema(endpoint_name)
                if cached_schema:
                    return cached_schema
            
            # Get endpoint info
            endpoint_info = await self.registry.get_endpoint(endpoint_name)
            if not endpoint_info:
                logger.error(f"Endpoint not found: {endpoint_name}")
                return None
            
            # Fetch schema
            schema_info = await self.schema_helper.get_schema_async(
                endpoint_info, max_depth
            )
            
            # Cache the result
            if schema_info and use_cache:
                await self.registry.update_endpoint_schema(endpoint_name, schema_info)
            
            return schema_info
            
        except Exception as e:
            logger.error(f"Failed to get schema for {endpoint_name}: {e}")
            return None
    
    async def execute_batch_operations_async(
        self,
        operations: List[Dict[str, Any]],
        execution_mode: str = "sequential",
        continue_on_error: bool = True,
        timeout_seconds: int = 30
    ) -> BatchExecutionResponse:
        """Execute multiple GraphQL operations."""
        try:
            from ..models.batch import BatchSummary
            from ..models.base import ExecutionMode
            
            start_time = datetime.utcnow()
            results = []
            
            mode = ExecutionMode(execution_mode) if execution_mode in ExecutionMode.__members__.values() else ExecutionMode.SEQUENTIAL
            
            if mode == ExecutionMode.PARALLEL:
                # Execute operations in parallel
                tasks = [
                    self._execute_single_operation(op, i, timeout_seconds)
                    for i, op in enumerate(operations)
                ]
                results = await asyncio.gather(*tasks, return_exceptions=True)
                
                # Convert exceptions to error results
                for i, result in enumerate(results):
                    if isinstance(result, Exception):
                        results[i] = BatchOperationResult(
                            name=operations[i].get("name", f"operation_{i}"),
                            success=False,
                            error=str(result),
                            index=i,
                            endpoint=operations[i].get("endpoint", "")
                        )
            else:
                # Execute operations sequentially
                for i, operation in enumerate(operations):
                    try:
                        result = await self._execute_single_operation(operation, i, timeout_seconds)
                        results.append(result)
                        
                        if not result.success and not continue_on_error:
                            break
                            
                    except Exception as e:
                        error_result = BatchOperationResult(
                            name=operation.get("name", f"operation_{i}"),
                            success=False,
                            error=str(e),
                            index=i,
                            endpoint=operation.get("endpoint", "")
                        )
                        results.append(error_result)
                        
                        if not continue_on_error:
                            break
            
            # Create summary
            end_time = datetime.utcnow()
            successful_ops = len([r for r in results if r.success])
            failed_ops = len(results) - successful_ops
            
            summary = BatchSummary(
                total_operations=len(operations),
                successful_operations=successful_ops,
                failed_operations=failed_ops,
                total_execution_time=end_time - start_time,
                execution_mode=mode,
                started_at=start_time,
                completed_at=end_time,
                continue_on_error=continue_on_error
            )
            
            return BatchExecutionResponse(
                results=results,
                summary=summary
            )
            
        except Exception as e:
            logger.error(f"Batch operation failed: {e}")
            return BatchExecutionResponse(
                results=[],
                summary=BatchSummary(),
                errors=[str(e)]
            )
    
    async def _execute_single_operation(
        self,
        operation: Dict[str, Any],
        index: int,
        timeout_seconds: int
    ) -> BatchOperationResult:
        """Execute a single GraphQL operation."""
        start_time = datetime.utcnow()
        
        try:
            endpoint_name = operation.get("endpoint", "")
            query = operation.get("query", "")
            variables = operation.get("variables", {})
            name = operation.get("name", f"operation_{index}")
            
            # Get endpoint info
            endpoint_info = await self.registry.get_endpoint(endpoint_name)
            if not endpoint_info:
                return BatchOperationResult(
                    name=name,
                    success=False,
                    error=f"Endpoint not found: {endpoint_name}",
                    index=index,
                    endpoint=endpoint_name,
                    query=query,
                    variables=variables
                )
            
            # Execute query
            data, errors, metadata = await self.http_client.execute_query(
                url=endpoint_info.url,
                query=query,
                variables=variables,
                headers=endpoint_info.headers,
                timeout=timeout_seconds
            )
            
            end_time = datetime.utcnow()
            execution_time = end_time - start_time
            
            # Record access
            await self.registry.record_endpoint_access(endpoint_name, success=len(errors) == 0)
            
            return BatchOperationResult(
                name=name,
                success=len(errors) == 0,
                data=data,
                error="; ".join([e.message for e in errors]) if errors else None,
                execution_time=execution_time,
                index=index,
                endpoint=endpoint_name,
                query=query,
                variables=variables
            )
            
        except Exception as e:
            end_time = datetime.utcnow()
            execution_time = end_time - start_time
            
            return BatchOperationResult(
                name=operation.get("name", f"operation_{index}"),
                success=False,
                error=str(e),
                execution_time=execution_time,
                index=index,
                endpoint=operation.get("endpoint", ""),
                query=operation.get("query", ""),
                variables=operation.get("variables", {})
            )
    
    async def analyze_schema_complexity_async(
        self,
        endpoint_name: str
    ) -> Dict[str, Any]:
        """Analyze schema complexity."""
        try:
            schema_info = await self.get_schema_async(endpoint_name)
            if not schema_info:
                return {"error": "Could not retrieve schema"}
            
            # Calculate complexity metrics
            total_types = len(schema_info.types)
            total_fields = sum(len(type_info.fields) for type_info in schema_info.types)
            total_operations = 0
            
            # Count operations
            if schema_info.query_type:
                query_type = next(
                    (t for t in schema_info.types if t.name == schema_info.query_type.name),
                    None
                )
                if query_type:
                    total_operations += len(query_type.fields)
            
            if schema_info.mutation_type:
                mutation_type = next(
                    (t for t in schema_info.types if t.name == schema_info.mutation_type.name),
                    None
                )
                if mutation_type:
                    total_operations += len(mutation_type.fields)
            
            complexity_score = min(100, (total_types * 2) + (total_fields // 10) + (total_operations * 3))
            
            return {
                "endpoint_name": endpoint_name,
                "total_types": total_types,
                "total_fields": total_fields,
                "total_operations": total_operations,
                "complexity_score": complexity_score,
                "complexity_rating": self._get_complexity_rating(complexity_score),
                "analysis_timestamp": datetime.utcnow().isoformat()
            }
            
        except Exception as e:
            logger.error(f"Schema complexity analysis failed: {e}")
            return {"error": str(e)}
    
    def _get_complexity_rating(self, score: int) -> str:
        """Get complexity rating based on score."""
        if score < 20:
            return "Simple"
        elif score < 50:
            return "Moderate"
        elif score < 80:
            return "Complex"
        else:
            return "Very Complex"
    
    async def compare_endpoint_schemas_async(
        self,
        endpoint_a: str,
        endpoint_b: str
    ) -> Dict[str, Any]:
        """Compare schemas between two endpoints."""
        try:
            schema_a = await self.get_schema_async(endpoint_a)
            schema_b = await self.get_schema_async(endpoint_b)
            
            if not schema_a or not schema_b:
                return {
                    "error": "Could not retrieve schemas for comparison",
                    "endpoint_a_available": schema_a is not None,
                    "endpoint_b_available": schema_b is not None
                }
            
            # Compare type counts
            types_a = {t.name for t in schema_a.types}
            types_b = {t.name for t in schema_b.types}
            
            common_types = types_a & types_b
            unique_a = types_a - types_b
            unique_b = types_b - types_a
            
            similarity_score = len(common_types) / max(len(types_a), len(types_b)) * 100
            
            return {
                "endpoint_a": endpoint_a,
                "endpoint_b": endpoint_b,
                "total_types_a": len(types_a),
                "total_types_b": len(types_b),
                "common_types": len(common_types),
                "unique_to_a": len(unique_a),
                "unique_to_b": len(unique_b),
                "similarity_score": round(similarity_score, 2),
                "are_compatible": similarity_score > 70,
                "comparison_timestamp": datetime.utcnow().isoformat()
            }
            
        except Exception as e:
            logger.error(f"Schema comparison failed: {e}")
            return {"error": str(e)}