"""LangGraph agent for migrating C# REST code to GraphQL queries."""

import json
import re
from typing import Dict, List, Any, Optional, TypedDict, Annotated
from dataclasses import dataclass
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage
from langchain_core.tools import tool
from langgraph.graph import StateGraph, END, START
from langgraph.graph.message import add_messages
# from langgraph.prebuilt.tool_node import ToolNode
import logging

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


class AgentState(TypedDict):
    """State for the C# to GraphQL migration agent."""
    messages: Annotated[List[Any], add_messages]
    csharp_code: str
    rest_calls: List[Dict[str, Any]]
    entities: List[str]
    graphql_queries: List[Dict[str, Any]]
    migration_plan: Optional[str]
    analysis_complete: bool


@dataclass
class RestCall:
    """Represents a REST API call found in C# code."""
    method: str
    endpoint: str
    purpose: str
    parameters: List[str]
    response_type: str
    line_number: int


@dataclass
class GraphQLQuery:
    """Represents a generated GraphQL query."""
    name: str
    query: str
    variables: Dict[str, Any]
    replaces_rest_calls: List[str]
    optimization_techniques: List[str]


class CSharpToGraphQLAgent:
    """LangGraph agent for C# to GraphQL migration."""
    
    def __init__(self, llm_base_url: str = "http://localhost:1234/v1", model_name: str = "gemma-3"):
        """Initialize the agent with LM Studio configuration."""
        self.llm = ChatOpenAI(
            base_url=llm_base_url,
            api_key="lm-studio",  # Can be any string for local models
            model=model_name,
            temperature=0.1
        )
        
        # Create the agent graph
        self.graph = self._create_graph()
    
    def _create_graph(self) -> StateGraph:
        """Create the LangGraph state graph."""
        # Create the graph
        workflow = StateGraph(AgentState)
        
        # Add nodes
        workflow.add_node("analyze_code", self._analyze_code_node)
        workflow.add_node("generate_queries", self._generate_queries_node)
        workflow.add_node("create_plan", self._create_plan_node)
        workflow.add_node("finalize", self._finalize_node)
        
        # Add edges
        workflow.add_edge(START, "analyze_code")
        workflow.add_edge("analyze_code", "generate_queries")
        workflow.add_edge("generate_queries", "create_plan")
        workflow.add_edge("create_plan", "finalize")
        workflow.add_edge("finalize", END)
        
        return workflow.compile()
    
    @tool
    def _analyze_csharp_code(self, csharp_code: str) -> Dict[str, Any]:
        """Analyze C# code to extract REST API calls and entities."""
        try:
            # Extract REST API calls using regex patterns
            rest_calls = []
            entities = set()
            
            # Pattern for HttpClient calls
            http_patterns = [
                r'(?:await\s+)?(?:httpClient|client|http)\.(?:GetAsync|PostAsync|PutAsync|DeleteAsync|SendAsync)\s*\(\s*["\']([^"\']+)["\']',
                r'(?:await\s+)?HttpClient\w*\.[A-Za-z]+\s*\(\s*["\']([^"\']+)["\']'
            ]
            
            line_number = 1
            for line in csharp_code.split('\n'):
                for pattern in http_patterns:
                    matches = re.finditer(pattern, line, re.IGNORECASE)
                    for match in matches:
                        endpoint = match.group(1)
                        method = self._determine_http_method(match.group(0))
                        
                        rest_call = {
                            "method": method,
                            "endpoint": endpoint,
                            "purpose": self._determine_purpose(endpoint),
                            "parameters": self._extract_parameters(endpoint),
                            "response_type": self._guess_response_type(endpoint),
                            "line_number": line_number
                        }
                        rest_calls.append(rest_call)
                        
                        # Extract entity from endpoint
                        entity = self._extract_entity_from_endpoint(endpoint)
                        if entity:
                            entities.add(entity)
                
                line_number += 1
            
            # Extract class definitions
            class_pattern = r'class\s+(\w+)|public\s+class\s+(\w+)'
            class_matches = re.finditer(class_pattern, csharp_code, re.IGNORECASE)
            for match in class_matches:
                class_name = match.group(1) or match.group(2)
                if class_name:
                    entities.add(class_name)
            
            return {
                "rest_calls": rest_calls,
                "entities": list(entities),
                "analysis_summary": f"Found {len(rest_calls)} REST calls and {len(entities)} entities"
            }
            
        except Exception as e:
            logger.error(f"Error analyzing C# code: {e}")
            return {"error": str(e), "rest_calls": [], "entities": []}
    
    @tool
    def _generate_graphql_queries(self, rest_calls: List[Dict[str, Any]], entities: List[str]) -> List[Dict[str, Any]]:
        """Generate optimized GraphQL queries to replace REST calls."""
        try:
            graphql_queries = []
            
            # Group related REST calls
            grouped_calls = self._group_related_calls(rest_calls)
            
            for group in grouped_calls:
                # Generate query name
                entities_in_group = set()
                for call in group:
                    entity = self._extract_entity_from_endpoint(call["endpoint"])
                    if entity:
                        entities_in_group.add(entity)
                
                query_name = f"Get{('And'.join(entities_in_group) or 'Data').title()}Query"
                
                # Build GraphQL query
                query_parts = ["query " + query_name + "($id: ID!) {"]
                variables = {"id": "example-id"}
                
                for call in group:
                    entity = self._extract_entity_from_endpoint(call["endpoint"]).lower()
                    if entity:
                        query_parts.append(f"  {entity}(id: $id) {{")
                        query_parts.append("    id")
                        query_parts.append("    name")
                        
                        # Add entity-specific fields
                        if "user" in entity.lower():
                            query_parts.extend(["    email", "    createdAt"])
                        elif "product" in entity.lower():
                            query_parts.extend(["    price", "    description"])
                        elif "order" in entity.lower():
                            query_parts.extend(["    total", "    status"])
                        
                        query_parts.append("  }")
                
                query_parts.append("}")
                
                graphql_query = {
                    "name": query_name,
                    "query": "\n".join(query_parts),
                    "variables": variables,
                    "replaces_rest_calls": [f"{call['method']} {call['endpoint']}" for call in group],
                    "optimization_techniques": ["Field Selection", "Single Request", "Variable Usage"],
                    "performance_improvement": self._calculate_performance_improvement(len(group))
                }
                
                graphql_queries.append(graphql_query)
            
            return graphql_queries
            
        except Exception as e:
            logger.error(f"Error generating GraphQL queries: {e}")
            return [{"error": str(e)}]
    
    @tool
    def _create_migration_plan(self, rest_calls: List[Dict[str, Any]], graphql_queries: List[Dict[str, Any]]) -> Dict[str, Any]:
        """Create a step-by-step migration plan."""
        try:
            migration_steps = [
                "1. Install GraphQL client library (e.g., GraphQL.Client, StrawberryShake)",
                "2. Set up GraphQL client configuration with endpoint URL",
                "3. Create GraphQL query documents for each generated query",
                "4. Update data transfer objects (DTOs) to match GraphQL response structure",
                "5. Replace REST API calls with GraphQL query execution",
                "6. Add error handling for GraphQL-specific errors",
                "7. Implement query result caching for better performance",
                "8. Test all GraphQL queries with various parameters",
                "9. Monitor performance and optimize queries if needed",
                "10. Remove unused REST API code and dependencies"
            ]
            
            # Generate migration code example
            if graphql_queries:
                first_query = graphql_queries[0]
                migration_code = self._generate_migration_code(first_query)
            else:
                migration_code = "// No GraphQL queries generated"
            
            # Calculate performance benefits
            rest_call_count = len(rest_calls)
            graphql_query_count = len(graphql_queries)
            network_reduction = max(0, ((rest_call_count - graphql_query_count) * 100) // rest_call_count) if rest_call_count > 0 else 0
            
            return {
                "migration_steps": migration_steps,
                "migration_code_example": migration_code,
                "performance_benefits": {
                    "rest_calls_before": rest_call_count,
                    "graphql_queries_after": graphql_query_count,
                    "network_reduction_percent": network_reduction,
                    "estimated_performance_gain": "Moderate to High"
                },
                "recommendations": [
                    "Use GraphQL fragments for reusable field sets",
                    "Implement query complexity analysis to prevent expensive queries",
                    "Consider using GraphQL subscriptions for real-time data",
                    "Add proper input validation for GraphQL variables",
                    "Implement connection-based pagination for large datasets"
                ]
            }
            
        except Exception as e:
            logger.error(f"Error creating migration plan: {e}")
            return {"error": str(e)}
    
    def _analyze_code_node(self, state: AgentState) -> AgentState:
        """Node to analyze C# code."""
        logger.info("Analyzing C# code...")
        
        system_prompt = """You are an expert C# and GraphQL developer. Analyze the provided C# code to:
1. Identify all REST API calls (HttpClient, WebClient, etc.)
2. Extract entities and data models
3. Understand data flow and relationships
4. Prepare for GraphQL query generation

Be thorough and precise in your analysis."""
        
        messages = [
            SystemMessage(content=system_prompt),
            HumanMessage(content=f"Analyze this C# code:\n\n{state['csharp_code']}")
        ]
        
        response = self.llm.invoke(messages)
        
        # Use the tool to get structured analysis
        analysis_result = self._analyze_csharp_code.invoke({"csharp_code": state["csharp_code"]})
        
        state["rest_calls"] = analysis_result.get("rest_calls", [])
        state["entities"] = analysis_result.get("entities", [])
        state["messages"].append(AIMessage(content=response.content))
        
        return state
    
    def _generate_queries_node(self, state: AgentState) -> AgentState:
        """Node to generate GraphQL queries."""
        logger.info("Generating GraphQL queries...")
        
        system_prompt = """You are a GraphQL expert. Generate optimized GraphQL queries to replace the identified REST calls.
Focus on:
1. Combining multiple REST calls into single GraphQL queries
2. Using proper field selection to avoid over-fetching
3. Implementing variables for dynamic queries
4. Adding fragments for reusable field sets"""
        
        rest_calls_summary = f"REST calls found: {len(state['rest_calls'])}\n"
        for call in state['rest_calls']:
            rest_calls_summary += f"- {call['method']} {call['endpoint']}\n"
        
        messages = state["messages"] + [
            SystemMessage(content=system_prompt),
            HumanMessage(content=f"Generate GraphQL queries for these REST calls:\n{rest_calls_summary}")
        ]
        
        response = self.llm.invoke(messages)
        
        # Use the tool to generate structured queries
        queries_result = self._generate_graphql_queries.invoke({
            "rest_calls": state["rest_calls"],
            "entities": state["entities"]
        })
        
        state["graphql_queries"] = queries_result
        state["messages"].append(AIMessage(content=response.content))
        
        return state
    
    def _create_plan_node(self, state: AgentState) -> AgentState:
        """Node to create migration plan."""
        logger.info("Creating migration plan...")
        
        system_prompt = """You are a software migration expert. Create a comprehensive migration plan to help developers
transition from REST API calls to GraphQL queries. Include:
1. Step-by-step migration instructions
2. Code examples showing before/after
3. Performance benefits analysis
4. Best practices and recommendations"""
        
        messages = state["messages"] + [
            SystemMessage(content=system_prompt),
            HumanMessage(content="Create a migration plan based on the analysis and generated GraphQL queries.")
        ]
        
        response = self.llm.invoke(messages)
        
        # Use the tool to create structured plan
        plan_result = self._create_migration_plan.invoke({
            "rest_calls": state["rest_calls"],
            "graphql_queries": state["graphql_queries"]
        })
        
        state["migration_plan"] = json.dumps(plan_result, indent=2)
        state["messages"].append(AIMessage(content=response.content))
        
        return state
    
    def _finalize_node(self, state: AgentState) -> AgentState:
        """Node to finalize the analysis and provide comprehensive results."""
        logger.info("Finalizing migration analysis...")
        
        state["analysis_complete"] = True
        
        # Create final summary
        summary = f"""
# C# to GraphQL Migration Analysis Complete

## Summary
- **REST Calls Found**: {len(state['rest_calls'])}
- **Entities Identified**: {len(state['entities'])}
- **GraphQL Queries Generated**: {len(state['graphql_queries'])}

## Next Steps
1. Review the generated GraphQL queries
2. Follow the migration plan
3. Test thoroughly before deploying

The migration analysis is now complete. Use the generated queries and migration plan to proceed with your C# to GraphQL migration.
"""
        
        state["messages"].append(AIMessage(content=summary))
        return state
    
    # Helper methods
    def _determine_http_method(self, match_value: str) -> str:
        """Determine HTTP method from the matched string."""
        lower_match = match_value.lower()
        if "getasync" in lower_match or "get" in lower_match:
            return "GET"
        elif "postasync" in lower_match or "post" in lower_match:
            return "POST"
        elif "putasync" in lower_match or "put" in lower_match:
            return "PUT"
        elif "deleteasync" in lower_match or "delete" in lower_match:
            return "DELETE"
        return "GET"
    
    def _determine_purpose(self, endpoint: str) -> str:
        """Determine the purpose of an endpoint."""
        endpoint_lower = endpoint.lower()
        if "user" in endpoint_lower:
            return "User management"
        elif "product" in endpoint_lower:
            return "Product data"
        elif "order" in endpoint_lower:
            return "Order processing"
        elif "search" in endpoint_lower:
            return "Search operations"
        return "Data retrieval"
    
    def _extract_parameters(self, endpoint: str) -> List[str]:
        """Extract parameters from endpoint URL."""
        parameters = []
        param_pattern = r'\{(\w+)\}'
        matches = re.finditer(param_pattern, endpoint)
        for match in matches:
            parameters.append(match.group(1))
        return parameters
    
    def _guess_response_type(self, endpoint: str) -> str:
        """Guess the response type based on endpoint."""
        endpoint_lower = endpoint.lower()
        if "user" in endpoint_lower:
            return "User"
        elif "product" in endpoint_lower:
            return "Product"
        elif "order" in endpoint_lower:
            return "Order"
        return "Object"
    
    def _extract_entity_from_endpoint(self, endpoint: str) -> str:
        """Extract entity name from endpoint URL."""
        parts = endpoint.split('/')
        for part in parts:
            if part and not part.startswith('{') and not part.startswith('api') and not part.startswith('v'):
                return part.rstrip('s')  # Remove plural
        return ""
    
    def _group_related_calls(self, rest_calls: List[Dict[str, Any]]) -> List[List[Dict[str, Any]]]:
        """Group related REST calls that can be combined into single GraphQL queries."""
        groups = []
        processed = set()
        
        for i, call in enumerate(rest_calls):
            if i in processed:
                continue
            
            group = [call]
            processed.add(i)
            
            entity1 = self._extract_entity_from_endpoint(call["endpoint"])
            
            for j, other_call in enumerate(rest_calls):
                if j in processed:
                    continue
                
                entity2 = self._extract_entity_from_endpoint(other_call["endpoint"])
                
                if entity1 and entity2 and entity1.lower() == entity2.lower():
                    group.append(other_call)
                    processed.add(j)
            
            groups.append(group)
        
        return groups
    
    def _calculate_performance_improvement(self, rest_call_count: int) -> str:
        """Calculate performance improvement description."""
        if rest_call_count == 1:
            return "Minimal improvement (single call)"
        elif rest_call_count <= 3:
            return f"Moderate improvement ({rest_call_count} calls → 1 query)"
        elif rest_call_count <= 5:
            return f"Significant improvement ({rest_call_count} calls → 1 query)"
        else:
            return f"Major improvement ({rest_call_count} calls → 1 query)"
    
    def _generate_migration_code(self, graphql_query: Dict[str, Any]) -> str:
        """Generate migration code example."""
        return f'''// Before: Multiple REST calls
// var user = await httpClient.GetAsync("/api/users/{{id}}");
// var posts = await httpClient.GetAsync("/api/users/{{id}}/posts");

// After: Single GraphQL query
public async Task<{graphql_query["name"]}Response> {graphql_query["name"]}Async(string id)
{{
    var query = @"
{graphql_query["query"]}
";

    var variables = new {{ id = id }};
    
    var request = new GraphQLRequest
    {{
        Query = query,
        Variables = variables
    }};
    
    var response = await graphqlClient.SendQueryAsync<{graphql_query["name"]}Response>(request);
    
    if (response.Errors?.Any() == true)
    {{
        throw new GraphQLException(response.Errors);
    }}
    
    return response.Data;
}}'''
    
    def migrate_csharp_code(self, csharp_code: str) -> Dict[str, Any]:
        """Main method to migrate C# code to GraphQL."""
        try:
            # Initialize state
            initial_state = AgentState(
                messages=[],
                csharp_code=csharp_code,
                rest_calls=[],
                entities=[],
                graphql_queries=[],
                migration_plan=None,
                analysis_complete=False
            )
            
            # Run the graph
            final_state = self.graph.invoke(initial_state)
            
            return {
                "success": True,
                "rest_calls": final_state["rest_calls"],
                "entities": final_state["entities"],
                "graphql_queries": final_state["graphql_queries"],
                "migration_plan": final_state["migration_plan"],
                "messages": [msg.content for msg in final_state["messages"] if hasattr(msg, 'content')]
            }
            
        except Exception as e:
            logger.error(f"Error in migration process: {e}")
            return {
                "success": False,
                "error": str(e),
                "rest_calls": [],
                "entities": [],
                "graphql_queries": [],
                "migration_plan": None
            }


def create_agent(llm_base_url: str = "http://localhost:1234/v1", model_name: str = "gemma-3") -> CSharpToGraphQLAgent:
    """Factory function to create the C# to GraphQL migration agent."""
    return CSharpToGraphQLAgent(llm_base_url=llm_base_url, model_name=model_name)


# Export graph for LangGraph Studio
agent = create_agent()
graph = agent.graph