"""HTTP client helper for GraphQL operations."""

import asyncio
import json
import logging
from typing import Any, Dict, List, Optional, Tuple, Union
from datetime import datetime, timedelta

import httpx
from httpx import AsyncClient, Response

from ..models.core import ErrorInfo, GraphQlError, ExecutionError
from ..models.performance import ExecutionMetadata


logger = logging.getLogger(__name__)


class GraphQLHttpClient:
    """HTTP client for GraphQL operations with comprehensive error handling."""
    
    def __init__(self, timeout: int = 30):
        self.timeout = timeout
        self.default_headers = {
            "Content-Type": "application/json",
            "User-Agent": "GraphQL-MCP-Server-Python/1.0",
        }
    
    async def create_client(
        self, 
        headers: Optional[Dict[str, str]] = None,
        timeout: Optional[int] = None
    ) -> AsyncClient:
        """Create an HTTP client with specified configuration."""
        client_headers = self.default_headers.copy()
        if headers:
            client_headers.update(headers)
        
        client_timeout = timeout or self.timeout
        
        return AsyncClient(
            headers=client_headers,
            timeout=client_timeout,
            follow_redirects=True,
        )
    
    async def execute_query(
        self,
        url: str,
        query: str,
        variables: Optional[Dict[str, Any]] = None,
        headers: Optional[Dict[str, str]] = None,
        timeout: Optional[int] = None,
        operation_name: Optional[str] = None
    ) -> Tuple[Optional[Dict[str, Any]], List[ExecutionError], ExecutionMetadata]:
        """
        Execute a GraphQL query with comprehensive error handling.
        
        Returns:
            Tuple of (data, errors, metadata)
        """
        start_time = datetime.utcnow()
        metadata = ExecutionMetadata(
            start_time=start_time,
            operation_name=operation_name
        )
        
        try:
            async with await self.create_client(headers, timeout) as client:
                payload = {
                    "query": query,
                    "variables": variables or {},
                }
                
                if operation_name:
                    payload["operationName"] = operation_name
                
                response = await client.post(url, json=payload)
                end_time = datetime.utcnow()
                
                # Update metadata
                metadata.end_time = end_time
                metadata.execution_time_ms = int((end_time - start_time).total_seconds() * 1000)
                metadata.server_time_ms = self._extract_server_time(response)
                metadata.network_time_ms = metadata.execution_time_ms - (metadata.server_time_ms or 0)
                
                return await self._process_response(response, metadata)
                
        except httpx.TimeoutException as e:
            logger.error(f"Request timeout for {url}: {e}")
            error = ExecutionError(
                message=f"Request timeout after {timeout or self.timeout} seconds",
                category="Timeout",
                severity="Error",
                suggestions=["Increase timeout", "Check network connectivity", "Verify server status"]
            )
            metadata.end_time = datetime.utcnow()
            metadata.execution_time_ms = int((metadata.end_time - start_time).total_seconds() * 1000)
            return None, [error], metadata
            
        except httpx.ConnectError as e:
            logger.error(f"Connection error for {url}: {e}")
            error = ExecutionError(
                message=f"Failed to connect to GraphQL endpoint: {str(e)}",
                category="Connection",
                severity="Error",
                suggestions=["Check URL", "Verify network connectivity", "Check firewall settings"]
            )
            metadata.end_time = datetime.utcnow()
            metadata.execution_time_ms = int((metadata.end_time - start_time).total_seconds() * 1000)
            return None, [error], metadata
            
        except Exception as e:
            logger.error(f"Unexpected error for {url}: {e}")
            error = ExecutionError(
                message=f"Unexpected error: {str(e)}",
                category="Unexpected",
                severity="Error",
                suggestions=["Check query syntax", "Verify endpoint configuration", "Review error logs"]
            )
            metadata.end_time = datetime.utcnow()
            metadata.execution_time_ms = int((metadata.end_time - start_time).total_seconds() * 1000)
            return None, [error], metadata
    
    async def _process_response(
        self, 
        response: Response, 
        metadata: ExecutionMetadata
    ) -> Tuple[Optional[Dict[str, Any]], List[ExecutionError], ExecutionMetadata]:
        """Process HTTP response and extract GraphQL data/errors."""
        errors = []
        
        try:
            if response.status_code != 200:
                error = ExecutionError(
                    message=f"HTTP {response.status_code}: {response.reason_phrase}",
                    category="HTTP",
                    severity="Error",
                    suggestions=self._get_http_error_suggestions(response.status_code)
                )
                errors.append(error)
                return None, errors, metadata
            
            response_data = response.json()
            
            # Extract GraphQL data and errors
            data = response_data.get("data")
            graphql_errors = response_data.get("errors", [])
            
            # Convert GraphQL errors to ExecutionError objects
            for error_data in graphql_errors:
                error = ExecutionError(
                    message=error_data.get("message", "Unknown GraphQL error"),
                    path=error_data.get("path"),
                    extensions=error_data.get("extensions"),
                    category="GraphQL",
                    severity="Error",
                    suggestions=self._get_graphql_error_suggestions(error_data)
                )
                errors.append(error)
            
            return data, errors, metadata
            
        except json.JSONDecodeError as e:
            logger.error(f"Invalid JSON response: {e}")
            error = ExecutionError(
                message=f"Invalid JSON response: {str(e)}",
                category="JSON",
                severity="Error",
                suggestions=["Check server response format", "Verify content-type header"]
            )
            errors.append(error)
            return None, errors, metadata
        
        except Exception as e:
            logger.error(f"Error processing response: {e}")
            error = ExecutionError(
                message=f"Error processing response: {str(e)}",
                category="Processing",
                severity="Error"
            )
            errors.append(error)
            return None, errors, metadata
    
    def _extract_server_time(self, response: Response) -> Optional[int]:
        """Extract server processing time from response headers."""
        # Common headers for server timing
        timing_headers = [
            "x-response-time",
            "x-processing-time",
            "server-timing",
            "x-execution-time"
        ]
        
        for header in timing_headers:
            if header in response.headers:
                try:
                    # Try to extract milliseconds from header value
                    value = response.headers[header]
                    # Handle various formats like "123ms", "0.123s", "123"
                    if "ms" in value:
                        return int(float(value.replace("ms", "")))
                    elif "s" in value:
                        return int(float(value.replace("s", "")) * 1000)
                    else:
                        return int(float(value))
                except (ValueError, TypeError):
                    continue
        
        return None
    
    def _get_http_error_suggestions(self, status_code: int) -> List[str]:
        """Get suggestions based on HTTP status code."""
        suggestions = {
            400: ["Check query syntax", "Verify request format", "Review variables"],
            401: ["Check authentication", "Verify API key", "Review headers"],
            403: ["Check permissions", "Verify authorization", "Review endpoint access"],
            404: ["Check endpoint URL", "Verify service is running", "Review path"],
            429: ["Reduce request rate", "Implement retry logic", "Check rate limits"],
            500: ["Check server logs", "Retry request", "Contact support"],
            502: ["Check gateway configuration", "Verify upstream service", "Retry request"],
            503: ["Service temporarily unavailable", "Retry with backoff", "Check maintenance status"],
            504: ["Increase timeout", "Check upstream service", "Retry request"],
        }
        return suggestions.get(status_code, ["Check server status", "Review error logs"])
    
    def _get_graphql_error_suggestions(self, error_data: Dict[str, Any]) -> List[str]:
        """Get suggestions based on GraphQL error."""
        message = error_data.get("message", "").lower()
        extensions = error_data.get("extensions", {})
        
        suggestions = []
        
        # Common GraphQL error patterns
        if "field" in message and "not found" in message:
            suggestions.extend(["Check field name", "Verify schema", "Review field availability"])
        elif "syntax" in message or "parse" in message:
            suggestions.extend(["Check query syntax", "Verify brackets and braces", "Review GraphQL spec"])
        elif "validation" in message:
            suggestions.extend(["Check field types", "Verify required fields", "Review schema"])
        elif "authorization" in message or "permission" in message:
            suggestions.extend(["Check permissions", "Verify authentication", "Review access rights"])
        elif "rate limit" in message:
            suggestions.extend(["Reduce request rate", "Implement retry logic", "Check rate limits"])
        else:
            suggestions.extend(["Check query structure", "Verify field names", "Review documentation"])
        
        # Add extension-specific suggestions
        if extensions.get("code"):
            code = extensions["code"].lower()
            if "timeout" in code:
                suggestions.extend(["Increase timeout", "Simplify query", "Check server performance"])
            elif "complexity" in code:
                suggestions.extend(["Reduce query complexity", "Use fragments", "Paginate results"])
        
        return suggestions
    
    async def introspect_schema(
        self,
        url: str,
        headers: Optional[Dict[str, str]] = None,
        timeout: Optional[int] = None
    ) -> Tuple[Optional[Dict[str, Any]], List[ExecutionError], ExecutionMetadata]:
        """Execute GraphQL introspection query."""
        introspection_query = """
        query IntrospectionQuery {
          __schema {
            queryType { name }
            mutationType { name }
            subscriptionType { name }
            types {
              ...FullType
            }
            directives {
              name
              description
              locations
              args {
                ...InputValue
              }
            }
          }
        }
        
        fragment FullType on __Type {
          kind
          name
          description
          fields(includeDeprecated: true) {
            name
            description
            args {
              ...InputValue
            }
            type {
              ...TypeRef
            }
            isDeprecated
            deprecationReason
          }
          inputFields {
            ...InputValue
          }
          interfaces {
            ...TypeRef
          }
          enumValues(includeDeprecated: true) {
            name
            description
            isDeprecated
            deprecationReason
          }
          possibleTypes {
            ...TypeRef
          }
        }
        
        fragment InputValue on __InputValue {
          name
          description
          type { ...TypeRef }
          defaultValue
        }
        
        fragment TypeRef on __Type {
          kind
          name
          ofType {
            kind
            name
            ofType {
              kind
              name
              ofType {
                kind
                name
                ofType {
                  kind
                  name
                  ofType {
                    kind
                    name
                    ofType {
                      kind
                      name
                      ofType {
                        kind
                        name
                      }
                    }
                  }
                }
              }
            }
          }
        }
        """
        
        return await self.execute_query(
            url=url,
            query=introspection_query,
            headers=headers,
            timeout=timeout,
            operation_name="IntrospectionQuery"
        )