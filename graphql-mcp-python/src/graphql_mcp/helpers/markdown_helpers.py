"""Markdown formatting helpers for GraphQL responses."""

import logging
from typing import Any, Dict, List, Optional, Union
from datetime import datetime

logger = logging.getLogger(__name__)


class MarkdownFormatHelpers:
    """Utilities for formatting GraphQL data as Markdown."""
    
    @staticmethod
    def format_header(text: str, level: int = 1) -> str:
        """Format text as Markdown header."""
        if level < 1 or level > 6:
            level = 1
        return f"{'#' * level} {text}\n\n"
    
    @staticmethod
    def format_code_block(code: str, language: str = "graphql") -> str:
        """Format code as Markdown code block."""
        return f"```{language}\n{code}\n```\n\n"
    
    @staticmethod
    def format_inline_code(code: str) -> str:
        """Format code as inline Markdown code."""
        return f"`{code}`"
    
    @staticmethod
    def format_list(items: List[str], ordered: bool = False) -> str:
        """Format list as Markdown list."""
        if not items:
            return ""
        
        formatted_items = []
        for i, item in enumerate(items):
            if ordered:
                formatted_items.append(f"{i + 1}. {item}")
            else:
                formatted_items.append(f"- {item}")
        
        return "\n".join(formatted_items) + "\n\n"
    
    @staticmethod
    def format_table(
        headers: List[str], 
        rows: List[List[str]], 
        alignment: Optional[List[str]] = None
    ) -> str:
        """
        Format data as Markdown table.
        
        Args:
            headers: Table headers
            rows: Table rows
            alignment: Column alignment ('left', 'center', 'right')
        """
        if not headers or not rows:
            return ""
        
        # Format header row
        header_row = "| " + " | ".join(headers) + " |"
        
        # Format separator row with alignment
        separators = []
        for i, header in enumerate(headers):
            if alignment and i < len(alignment):
                align = alignment[i].lower()
                if align == "center":
                    separators.append(":---:")
                elif align == "right":
                    separators.append("---:")
                else:
                    separators.append("---")
            else:
                separators.append("---")
        
        separator_row = "| " + " | ".join(separators) + " |"
        
        # Format data rows
        data_rows = []
        for row in rows:
            # Ensure row has same number of columns as headers
            padded_row = row + [""] * (len(headers) - len(row))
            padded_row = padded_row[:len(headers)]  # Truncate if too long
            data_rows.append("| " + " | ".join(str(cell) for cell in padded_row) + " |")
        
        return "\n".join([header_row, separator_row] + data_rows) + "\n\n"
    
    @staticmethod
    def format_schema_type(type_info: Dict[str, Any]) -> str:
        """Format GraphQL type information as Markdown."""
        try:
            output = []
            
            # Type header
            type_name = type_info.get("name", "Unknown")
            type_kind = type_info.get("kind", "OBJECT")
            output.append(MarkdownFormatHelpers.format_header(f"{type_name} ({type_kind})", 3))
            
            # Description
            description = type_info.get("description")
            if description:
                output.append(f"{description}\n\n")
            
            # Fields
            fields = type_info.get("fields", [])
            if fields:
                output.append(MarkdownFormatHelpers.format_header("Fields", 4))
                field_rows = []
                for field in fields:
                    field_name = field.get("name", "")
                    field_type = MarkdownFormatHelpers._format_type_ref(field.get("type", {}))
                    field_desc = field.get("description", "")
                    field_deprecated = "⚠️ Deprecated" if field.get("isDeprecated", False) else ""
                    
                    field_rows.append([
                        MarkdownFormatHelpers.format_inline_code(field_name),
                        MarkdownFormatHelpers.format_inline_code(field_type),
                        field_desc,
                        field_deprecated
                    ])
                
                output.append(MarkdownFormatHelpers.format_table(
                    ["Field", "Type", "Description", "Status"],
                    field_rows
                ))
            
            # Enum values
            enum_values = type_info.get("enumValues", [])
            if enum_values:
                output.append(MarkdownFormatHelpers.format_header("Values", 4))
                enum_rows = []
                for enum_val in enum_values:
                    enum_name = enum_val.get("name", "")
                    enum_desc = enum_val.get("description", "")
                    enum_deprecated = "⚠️ Deprecated" if enum_val.get("isDeprecated", False) else ""
                    
                    enum_rows.append([
                        MarkdownFormatHelpers.format_inline_code(enum_name),
                        enum_desc,
                        enum_deprecated
                    ])
                
                output.append(MarkdownFormatHelpers.format_table(
                    ["Value", "Description", "Status"],
                    enum_rows
                ))
            
            return "".join(output)
            
        except Exception as e:
            logger.error(f"Error formatting schema type: {e}")
            return f"Error formatting type information: {str(e)}\n\n"
    
    @staticmethod
    def _format_type_ref(type_ref: Dict[str, Any]) -> str:
        """Format GraphQL type reference."""
        try:
            kind = type_ref.get("kind")
            name = type_ref.get("name")
            of_type = type_ref.get("ofType")
            
            if kind == "NON_NULL":
                return f"{MarkdownFormatHelpers._format_type_ref(of_type)}!"
            elif kind == "LIST":
                return f"[{MarkdownFormatHelpers._format_type_ref(of_type)}]"
            else:
                return name or "Unknown"
        except:
            return "Unknown"
    
    @staticmethod
    def format_query_example(
        query: str, 
        variables: Optional[Dict[str, Any]] = None,
        response: Optional[Dict[str, Any]] = None
    ) -> str:
        """Format GraphQL query example as Markdown."""
        output = []
        
        # Query
        output.append(MarkdownFormatHelpers.format_header("Query", 4))
        output.append(MarkdownFormatHelpers.format_code_block(query, "graphql"))
        
        # Variables
        if variables:
            output.append(MarkdownFormatHelpers.format_header("Variables", 4))
            variables_json = MarkdownFormatHelpers._format_json(variables)
            output.append(MarkdownFormatHelpers.format_code_block(variables_json, "json"))
        
        # Response
        if response:
            output.append(MarkdownFormatHelpers.format_header("Response", 4))
            response_json = MarkdownFormatHelpers._format_json(response)
            output.append(MarkdownFormatHelpers.format_code_block(response_json, "json"))
        
        return "".join(output)
    
    @staticmethod
    def format_error_info(errors: List[Dict[str, Any]]) -> str:
        """Format GraphQL errors as Markdown."""
        if not errors:
            return ""
        
        output = []
        output.append(MarkdownFormatHelpers.format_header("Errors", 3))
        
        for i, error in enumerate(errors, 1):
            output.append(MarkdownFormatHelpers.format_header(f"Error {i}", 4))
            
            message = error.get("message", "Unknown error")
            output.append(f"**Message:** {message}\n\n")
            
            path = error.get("path")
            if path:
                path_str = " → ".join(str(p) for p in path)
                output.append(f"**Path:** {path_str}\n\n")
            
            locations = error.get("locations")
            if locations:
                loc_strs = []
                for loc in locations:
                    line = loc.get("line", "?")
                    column = loc.get("column", "?")
                    loc_strs.append(f"Line {line}, Column {column}")
                output.append(f"**Locations:** {', '.join(loc_strs)}\n\n")
            
            extensions = error.get("extensions")
            if extensions:
                ext_json = MarkdownFormatHelpers._format_json(extensions)
                output.append(f"**Extensions:**\n{MarkdownFormatHelpers.format_code_block(ext_json, 'json')}")
        
        return "".join(output)
    
    @staticmethod
    def format_performance_metrics(metadata: Dict[str, Any]) -> str:
        """Format performance metrics as Markdown."""
        try:
            output = []
            output.append(MarkdownFormatHelpers.format_header("Performance Metrics", 3))
            
            metrics = []
            
            # Execution time
            exec_time = metadata.get("executionTimeMs", metadata.get("execution_time_ms"))
            if exec_time is not None:
                metrics.append(["Execution Time", f"{exec_time}ms"])
            
            # Complexity score
            complexity = metadata.get("complexityScore", metadata.get("complexity_score"))
            if complexity is not None:
                metrics.append(["Complexity Score", str(complexity)])
            
            # Depth score
            depth = metadata.get("depthScore", metadata.get("depth_score"))
            if depth is not None:
                metrics.append(["Depth Score", str(depth)])
            
            # Field count
            field_count = metadata.get("fieldCount", metadata.get("field_count"))
            if field_count is not None:
                metrics.append(["Field Count", str(field_count)])
            
            # Cache hit
            cache_hit = metadata.get("cacheHit", metadata.get("cache_hit"))
            if cache_hit is not None:
                metrics.append(["Cache Hit", "✅ Yes" if cache_hit else "❌ No"])
            
            if metrics:
                output.append(MarkdownFormatHelpers.format_table(
                    ["Metric", "Value"],
                    metrics
                ))
            
            return "".join(output)
            
        except Exception as e:
            logger.error(f"Error formatting performance metrics: {e}")
            return f"Error formatting metrics: {str(e)}\n\n"
    
    @staticmethod
    def format_recommendations(recommendations: List[str], title: str = "Recommendations") -> str:
        """Format recommendations as Markdown list."""
        if not recommendations:
            return ""
        
        output = []
        output.append(MarkdownFormatHelpers.format_header(title, 3))
        output.append(MarkdownFormatHelpers.format_list(recommendations))
        
        return "".join(output)
    
    @staticmethod
    def _format_json(data: Any) -> str:
        """Format data as pretty JSON."""
        try:
            import json
            return json.dumps(data, indent=2, ensure_ascii=False)
        except Exception:
            return str(data)
    
    @staticmethod
    def format_endpoint_summary(endpoint_info: Dict[str, Any]) -> str:
        """Format endpoint information as Markdown."""
        try:
            output = []
            
            name = endpoint_info.get("name", "Unknown")
            output.append(MarkdownFormatHelpers.format_header(f"Endpoint: {name}", 2))
            
            # Basic info
            url = endpoint_info.get("url", "")
            if url:
                output.append(f"**URL:** {url}\n\n")
            
            # Features
            features = []
            if endpoint_info.get("allowMutations", endpoint_info.get("allow_mutations")):
                features.append("✅ Mutations")
            else:
                features.append("❌ Mutations")
            
            tool_prefix = endpoint_info.get("toolPrefix", endpoint_info.get("tool_prefix", ""))
            if tool_prefix:
                features.append(f"🏷️ Tool Prefix: {tool_prefix}")
            
            if features:
                output.append("**Features:**\n")
                output.append(MarkdownFormatHelpers.format_list(features))
            
            # Headers (if any non-sensitive)
            headers = endpoint_info.get("headers", {})
            if headers:
                safe_headers = {
                    k: v for k, v in headers.items() 
                    if not any(sensitive in k.lower() for sensitive in ['token', 'key', 'secret', 'auth'])
                }
                if safe_headers:
                    output.append("**Headers:**\n")
                    for key, value in safe_headers.items():
                        output.append(f"- {key}: {value}\n")
                    output.append("\n")
            
            return "".join(output)
            
        except Exception as e:
            logger.error(f"Error formatting endpoint summary: {e}")
            return f"Error formatting endpoint: {str(e)}\n\n"
    
    @staticmethod
    def create_collapsible_section(title: str, content: str) -> str:
        """Create a collapsible section in Markdown."""
        return f"""<details>
<summary>{title}</summary>

{content}
</details>

"""