"""GraphQL schema helper for introspection and analysis."""

import logging
from typing import Any, Dict, List, Optional
from datetime import datetime

from ..models.schema import SchemaInfo, GraphQlTypeInfo, TypeReference
from ..models.core import GraphQlEndpointInfo
from .http_client import GraphQLHttpClient

logger = logging.getLogger(__name__)


class GraphQLSchemaHelper:
    """Helper for GraphQL schema introspection and analysis."""
    
    def __init__(self):
        self.http_client = GraphQLHttpClient()
    
    async def get_schema_async(
        self,
        endpoint_info: GraphQlEndpointInfo,
        max_depth: int = 3
    ) -> Optional[SchemaInfo]:
        """Get schema information from a GraphQL endpoint."""
        try:
            data, errors, metadata = await self.http_client.introspect_schema(
                url=endpoint_info.url,
                headers=endpoint_info.headers,
                timeout=30
            )
            
            if errors:
                logger.error(f"Schema introspection errors: {errors}")
                return None
            
            if not data or "__schema" not in data:
                logger.error("Invalid introspection response")
                return None
            
            return self._parse_introspection_data(data["__schema"])
            
        except Exception as e:
            logger.error(f"Schema introspection failed: {e}")
            return None
    
    def _parse_introspection_data(self, schema_data: Dict[str, Any]) -> SchemaInfo:
        """Parse introspection data into SchemaInfo model."""
        try:
            # Parse basic schema info
            query_type = self._parse_type_ref(schema_data.get("queryType"))
            mutation_type = self._parse_type_ref(schema_data.get("mutationType"))
            subscription_type = self._parse_type_ref(schema_data.get("subscriptionType"))
            
            # Parse types
            types = []
            for type_data in schema_data.get("types", []):
                if type_data.get("name", "").startswith("__"):
                    continue  # Skip introspection types
                
                parsed_type = self._parse_type_info(type_data)
                if parsed_type:
                    types.append(parsed_type)
            
            return SchemaInfo(
                query_type=query_type,
                mutation_type=mutation_type,
                subscription_type=subscription_type,
                types=types,
                last_modified=datetime.utcnow(),
                version="1.0"
            )
            
        except Exception as e:
            logger.error(f"Failed to parse introspection data: {e}")
            raise
    
    def _parse_type_ref(self, type_data: Optional[Dict[str, Any]]) -> Optional[TypeReference]:
        """Parse a type reference from introspection data."""
        if not type_data:
            return None
        
        try:
            from ..models.base import TypeKind
            
            kind_str = type_data.get("kind", "")
            kind = TypeKind(kind_str) if kind_str in TypeKind.__members__.values() else TypeKind.OBJECT
            
            of_type = None
            if "ofType" in type_data and type_data["ofType"]:
                of_type = self._parse_type_ref(type_data["ofType"])
            
            return TypeReference(
                kind=kind,
                name=type_data.get("name"),
                description=type_data.get("description"),
                of_type=of_type
            )
            
        except Exception as e:
            logger.error(f"Failed to parse type reference: {e}")
            return None
    
    def _parse_type_info(self, type_data: Dict[str, Any]) -> Optional[GraphQlTypeInfo]:
        """Parse type information from introspection data."""
        try:
            from ..models.base import TypeKind
            from ..models.schema import FieldInfo, InputFieldInfo, EnumValueInfo, ArgumentInfo
            
            kind_str = type_data.get("kind", "")
            kind = TypeKind(kind_str) if kind_str in TypeKind.__members__.values() else TypeKind.OBJECT
            
            # Parse fields
            fields = []
            for field_data in type_data.get("fields", []):
                field_type = self._parse_type_ref(field_data.get("type"))
                
                # Parse arguments
                args = []
                for arg_data in field_data.get("args", []):
                    arg_type = self._parse_type_ref(arg_data.get("type"))
                    args.append(ArgumentInfo(
                        name=arg_data.get("name", ""),
                        description=arg_data.get("description"),
                        type=arg_type,
                        default_value=arg_data.get("defaultValue"),
                        is_required=arg_type and arg_type.kind.value == "NON_NULL"
                    ))
                
                fields.append(FieldInfo(
                    name=field_data.get("name", ""),
                    description=field_data.get("description"),
                    type=field_type,
                    args=args,
                    is_deprecated=field_data.get("isDeprecated", False),
                    deprecation_reason=field_data.get("deprecationReason")
                ))
            
            # Parse input fields
            input_fields = []
            for input_field_data in type_data.get("inputFields", []):
                input_field_type = self._parse_type_ref(input_field_data.get("type"))
                input_fields.append(InputFieldInfo(
                    name=input_field_data.get("name", ""),
                    description=input_field_data.get("description"),
                    type=input_field_type,
                    default_value=input_field_data.get("defaultValue"),
                    is_required=input_field_type and input_field_type.kind.value == "NON_NULL"
                ))
            
            # Parse enum values
            enum_values = []
            for enum_data in type_data.get("enumValues", []):
                enum_values.append(EnumValueInfo(
                    name=enum_data.get("name", ""),
                    description=enum_data.get("description"),
                    is_deprecated=enum_data.get("isDeprecated", False),
                    deprecation_reason=enum_data.get("deprecationReason")
                ))
            
            # Parse interfaces and possible types
            interfaces = [
                self._parse_type_ref(interface_data)
                for interface_data in type_data.get("interfaces", [])
            ]
            interfaces = [iface for iface in interfaces if iface]
            
            possible_types = [
                self._parse_type_ref(possible_type_data)
                for possible_type_data in type_data.get("possibleTypes", [])
            ]
            possible_types = [ptype for ptype in possible_types if ptype]
            
            return GraphQlTypeInfo(
                kind=kind,
                name=type_data.get("name", ""),
                description=type_data.get("description"),
                fields=fields,
                input_fields=input_fields,
                interfaces=interfaces,
                enum_values=enum_values,
                possible_types=possible_types
            )
            
        except Exception as e:
            logger.error(f"Failed to parse type info: {e}")
            return None
    
    async def generate_tools_from_schema(
        self,
        endpoint_info: GraphQlEndpointInfo,
        schema_info: SchemaInfo
    ) -> List[str]:
        """Generate MCP tool names from schema information."""
        tools = []
        
        try:
            prefix = endpoint_info.tool_prefix or endpoint_info.name
            
            # Generate query tools
            if schema_info.query_type:
                for type_info in schema_info.types:
                    if type_info.name == schema_info.query_type.name:
                        for field in type_info.fields:
                            tool_name = f"{prefix}_{field.name}"
                            tools.append(tool_name)
            
            # Generate mutation tools if allowed
            if endpoint_info.allow_mutations and schema_info.mutation_type:
                for type_info in schema_info.types:
                    if type_info.name == schema_info.mutation_type.name:
                        for field in type_info.fields:
                            tool_name = f"{prefix}_mutation_{field.name}"
                            tools.append(tool_name)
            
            logger.info(f"Generated {len(tools)} tools for endpoint {endpoint_info.name}")
            return tools
            
        except Exception as e:
            logger.error(f"Failed to generate tools from schema: {e}")
            return []