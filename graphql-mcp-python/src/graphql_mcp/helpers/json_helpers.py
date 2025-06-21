"""JSON helper utilities for GraphQL operations."""

import json
import logging
from datetime import datetime, timedelta
from typing import Any, Dict, List, Optional, Union
from decimal import Decimal
from enum import Enum

logger = logging.getLogger(__name__)


class JsonHelpers:
    """JSON serialization and deserialization utilities."""
    
    @staticmethod
    def serialize_datetime(obj: Any) -> str:
        """Serialize datetime objects to ISO format strings."""
        if isinstance(obj, datetime):
            return obj.isoformat()
        elif isinstance(obj, timedelta):
            return str(obj.total_seconds())
        elif isinstance(obj, Decimal):
            return float(obj)
        elif isinstance(obj, Enum):
            return obj.value
        else:
            raise TypeError(f"Object of type {type(obj)} is not JSON serializable")
    
    @staticmethod
    def to_json(
        data: Any, 
        indent: Optional[int] = None, 
        sort_keys: bool = False,
        ensure_ascii: bool = False
    ) -> str:
        """
        Convert data to JSON string with custom serialization.
        
        Args:
            data: Data to serialize
            indent: JSON indentation level
            sort_keys: Whether to sort keys
            ensure_ascii: Whether to ensure ASCII output
            
        Returns:
            JSON string
        """
        try:
            return json.dumps(
                data,
                default=JsonHelpers.serialize_datetime,
                indent=indent,
                sort_keys=sort_keys,
                ensure_ascii=ensure_ascii,
                separators=(',', ': ') if indent else (',', ':')
            )
        except Exception as e:
            logger.error(f"JSON serialization error: {e}")
            # Return error info as JSON
            return json.dumps({
                "error": "JSON serialization failed",
                "message": str(e),
                "type": str(type(data))
            })
    
    @staticmethod
    def from_json(json_str: str) -> Any:
        """
        Parse JSON string to Python object.
        
        Args:
            json_str: JSON string to parse
            
        Returns:
            Parsed Python object
        """
        try:
            return json.loads(json_str)
        except json.JSONDecodeError as e:
            logger.error(f"JSON parsing error: {e}")
            return {
                "error": "JSON parsing failed",
                "message": str(e),
                "position": e.pos if hasattr(e, 'pos') else None
            }
        except Exception as e:
            logger.error(f"Unexpected JSON error: {e}")
            return {
                "error": "Unexpected JSON error",
                "message": str(e)
            }
    
    @staticmethod
    def prettify(data: Any) -> str:
        """Format data as pretty-printed JSON."""
        return JsonHelpers.to_json(data, indent=2, sort_keys=True)
    
    @staticmethod
    def minify(data: Any) -> str:
        """Format data as minified JSON."""
        return JsonHelpers.to_json(data, indent=None, sort_keys=False)
    
    @staticmethod
    def safe_get(data: Dict[str, Any], key: str, default: Any = None) -> Any:
        """Safely get a value from a dictionary with optional default."""
        try:
            return data.get(key, default)
        except (AttributeError, TypeError):
            return default
    
    @staticmethod
    def safe_get_nested(
        data: Dict[str, Any], 
        keys: List[str], 
        default: Any = None
    ) -> Any:
        """
        Safely get a nested value from a dictionary.
        
        Args:
            data: Dictionary to search
            keys: List of keys for nested access
            default: Default value if key path doesn't exist
            
        Returns:
            Value at nested key path or default
        """
        try:
            current = data
            for key in keys:
                if isinstance(current, dict) and key in current:
                    current = current[key]
                else:
                    return default
            return current
        except (AttributeError, TypeError, KeyError):
            return default
    
    @staticmethod
    def merge_dicts(dict1: Dict[str, Any], dict2: Dict[str, Any]) -> Dict[str, Any]:
        """
        Recursively merge two dictionaries.
        
        Args:
            dict1: First dictionary
            dict2: Second dictionary (takes precedence)
            
        Returns:
            Merged dictionary
        """
        try:
            result = dict1.copy()
            
            for key, value in dict2.items():
                if (key in result and 
                    isinstance(result[key], dict) and 
                    isinstance(value, dict)):
                    result[key] = JsonHelpers.merge_dicts(result[key], value)
                else:
                    result[key] = value
            
            return result
        except Exception as e:
            logger.error(f"Dictionary merge error: {e}")
            return dict2.copy()  # Return second dict as fallback
    
    @staticmethod
    def flatten_dict(
        data: Dict[str, Any], 
        separator: str = ".",
        prefix: str = ""
    ) -> Dict[str, Any]:
        """
        Flatten a nested dictionary.
        
        Args:
            data: Dictionary to flatten
            separator: Separator for nested keys
            prefix: Prefix for keys
            
        Returns:
            Flattened dictionary
        """
        try:
            result = {}
            
            for key, value in data.items():
                new_key = f"{prefix}{separator}{key}" if prefix else key
                
                if isinstance(value, dict):
                    result.update(JsonHelpers.flatten_dict(value, separator, new_key))
                elif isinstance(value, list):
                    for i, item in enumerate(value):
                        list_key = f"{new_key}{separator}{i}"
                        if isinstance(item, dict):
                            result.update(JsonHelpers.flatten_dict(item, separator, list_key))
                        else:
                            result[list_key] = item
                else:
                    result[new_key] = value
            
            return result
        except Exception as e:
            logger.error(f"Dictionary flattening error: {e}")
            return {"error": f"Flattening failed: {str(e)}"}
    
    @staticmethod
    def extract_graphql_errors(response_data: Dict[str, Any]) -> List[Dict[str, Any]]:
        """Extract GraphQL errors from response data."""
        try:
            errors = response_data.get("errors", [])
            if not isinstance(errors, list):
                return []
            
            processed_errors = []
            for error in errors:
                if isinstance(error, dict):
                    processed_errors.append({
                        "message": error.get("message", "Unknown error"),
                        "path": error.get("path", []),
                        "locations": error.get("locations", []),
                        "extensions": error.get("extensions", {})
                    })
            
            return processed_errors
        except Exception as e:
            logger.error(f"Error extracting GraphQL errors: {e}")
            return [{"message": f"Error processing: {str(e)}"}]
    
    @staticmethod
    def sanitize_for_json(data: Any) -> Any:
        """
        Sanitize data for JSON serialization by removing non-serializable objects.
        
        Args:
            data: Data to sanitize
            
        Returns:
            JSON-serializable version of the data
        """
        try:
            if data is None:
                return None
            elif isinstance(data, (str, int, float, bool)):
                return data
            elif isinstance(data, datetime):
                return data.isoformat()
            elif isinstance(data, timedelta):
                return str(data.total_seconds())
            elif isinstance(data, Decimal):
                return float(data)
            elif isinstance(data, Enum):
                return data.value
            elif isinstance(data, dict):
                return {
                    key: JsonHelpers.sanitize_for_json(value)
                    for key, value in data.items()
                }
            elif isinstance(data, (list, tuple)):
                return [JsonHelpers.sanitize_for_json(item) for item in data]
            elif isinstance(data, set):
                return [JsonHelpers.sanitize_for_json(item) for item in data]
            else:
                # For custom objects, try to convert to dict
                if hasattr(data, '__dict__'):
                    return JsonHelpers.sanitize_for_json(data.__dict__)
                else:
                    return str(data)
        except Exception as e:
            logger.error(f"Data sanitization error: {e}")
            return {"error": f"Sanitization failed: {str(e)}"}
    
    @staticmethod
    def validate_json_string(json_str: str) -> Dict[str, Any]:
        """
        Validate a JSON string and return validation result.
        
        Args:
            json_str: JSON string to validate
            
        Returns:
            Dictionary with validation results
        """
        try:
            parsed = json.loads(json_str)
            return {
                "valid": True,
                "data": parsed,
                "error": None,
                "size_bytes": len(json_str.encode('utf-8'))
            }
        except json.JSONDecodeError as e:
            return {
                "valid": False,
                "data": None,
                "error": {
                    "type": "JSONDecodeError",
                    "message": str(e),
                    "line": e.lineno if hasattr(e, 'lineno') else None,
                    "column": e.colno if hasattr(e, 'colno') else None,
                    "position": e.pos if hasattr(e, 'pos') else None
                },
                "size_bytes": len(json_str.encode('utf-8'))
            }
        except Exception as e:
            return {
                "valid": False,
                "data": None,
                "error": {
                    "type": str(type(e).__name__),
                    "message": str(e)
                },
                "size_bytes": len(json_str.encode('utf-8'))
            }