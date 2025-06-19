using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Granular GraphQL schema exploration tools for focused field and type discovery
/// </summary>
[McpServerToolType]
public static class SchemaExplorationTools
{
    [McpServerTool, Description(@"Lists all available root-level fields for GraphQL queries with comprehensive field information.

This tool provides a complete overview of all Query operations including:

Field Information:
- Field names and return types
- Parameter requirements with types and descriptions  
- Field descriptions and documentation
- Deprecation status and migration guidance
- SDL-formatted type signatures

Output Format:
- Organized by field name
- Return type with null/list indicators
- Required vs optional parameters
- Deprecation warnings highlighted
- Total field count summary

Use Cases:
- API discovery and exploration
- Understanding available data operations
- Planning query construction
- Identifying deprecated operations
- Quick reference for available queries

Example Output:
```
# Query Fields (15)
**Endpoint:** my-api
**Root Type:** Query

- **getUsers**: `[User]` - Retrieve paginated list of users
- **getUserById**: `User` - Get specific user by ID
- **searchUsers**: `[User]` - Search users by criteria [DEPRECATED]
```")]
    public static async Task<string> ListQueryFields(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName)
    {
        return await ListRootFields(endpointName, "Query");
    }

    [McpServerTool, Description(@"Gets detailed definition for a specific query field in SDL format with complete parameter and return type information.

This tool provides comprehensive field analysis including:

Field Definition:
- Complete SDL syntax with proper formatting
- Parameter names, types, and requirements
- Default values and optional indicators
- Return type with nested structure details
- Field description and usage guidance

Parameter Analysis:
- Required vs optional parameters
- Parameter types (scalars, objects, enums)
- Default values and validation rules
- Input type definitions and constraints
- Nested input object structures

Deprecation Information:
- Deprecation status and warnings
- Migration recommendations
- Alternative field suggestions
- Timeline for removal

Use Cases:
- Understanding specific operation signatures
- Planning query parameter construction
- Validating parameter requirements
- Generating client code
- API integration planning

Example Output:
```
# Query Field: getUserById

**Description:** Retrieves a specific user by their unique identifier

**Return Type:** `User`

## Arguments
- **id**: `ID!` - Unique identifier for the user

## SDL Definition
```graphql
getUserById(id: ID!): User
```")]
    public static async Task<string> GetQueryField(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Name of the query field to get details for")]
        string fieldName)
    {
        return await GetRootFieldDefinition(endpointName, "Query", fieldName);
    }

    [McpServerTool, Description(@"Lists all available root-level fields for GraphQL mutations with comprehensive operation details.

This tool provides complete mutation operation discovery including:

Mutation Operations:
- All available data modification operations
- Input parameter requirements and types
- Return types and response structures
- Operation descriptions and side effects
- Authentication and permission requirements

Data Modification Types:
- Create operations (insertions)
- Update operations (modifications)  
- Delete operations (removals)
- Bulk operations (batch processing)
- Custom business logic operations

Input Validation:
- Required input fields and constraints
- Input type definitions and structures
- Validation rules and restrictions
- File upload capabilities
- Complex nested input objects

Use Cases:
- Understanding available data modifications
- Planning mutation workflows
- Identifying input requirements
- Validating operation permissions
- Designing data update strategies

Example Output:
```
# Mutation Fields (8)
**Endpoint:** my-api  
**Root Type:** Mutation

- **createUser**: `User` - Create new user account
- **updateUser**: `User` - Update existing user information
- **deleteUser**: `Boolean` - Remove user account [REQUIRES_ADMIN]
- **bulkCreateUsers**: `[User]` - Create multiple users in batch
```")]
    public static async Task<string> ListMutationFields(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName)
    {
        return await ListRootFields(endpointName, "Mutation");
    }

    [McpServerTool, Description(@"Gets detailed definition for a specific mutation field in SDL format with comprehensive input/output analysis.

This tool provides complete mutation operation analysis including:

Operation Definition:
- Complete SDL syntax for the mutation
- Input parameter types and requirements
- Return type structure and nullable fields
- Operation description and behavior
- Side effects and data changes

Input Analysis:
- Required vs optional input fields
- Input type definitions and constraints
- Nested input object structures
- File upload parameter handling
- Validation rules and restrictions

Return Value Analysis:
- Success response structure
- Error handling and return codes
- Modified entity relationships
- Computed field updates
- Transaction rollback scenarios

Security & Permissions:
- Required authentication levels
- Permission checks and validation
- Rate limiting considerations
- Audit logging requirements
- Data access restrictions

Use Cases:
- Understanding mutation behavior
- Planning input data preparation
- Validating operation permissions
- Error handling implementation
- Integration testing scenarios

Example Output:
```
# Mutation Field: createUser

**Description:** Creates a new user account with validation and role assignment

**Return Type:** `User`

## Arguments
- **input**: `CreateUserInput!` - User creation data

## SDL Definition
```graphql
createUser(input: CreateUserInput!): User
```

CreateUserInput type: { name!, email!, role, department }")]
    public static async Task<string> GetMutationField(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Name of the mutation field to get details for")]
        string fieldName)
    {
        return await GetRootFieldDefinition(endpointName, "Mutation", fieldName);
    }

    [McpServerTool, Description(@"Lists all available root-level fields for GraphQL subscriptions with real-time event information.

This tool discovers subscription operations for real-time data streaming including:

Subscription Operations:
- Real-time event streams and updates
- Data change notifications
- Live query results with filters
- Event-driven data synchronization
- WebSocket connection requirements

Event Types:
- Entity creation/update/deletion events
- Field-level change notifications
- Filtered event streams
- Aggregated data updates
- Custom business event triggers

Connection Management:
- WebSocket protocol requirements
- Authentication for subscriptions
- Connection lifecycle management
- Retry and reconnection strategies
- Rate limiting and throttling

Filtering & Parameters:
- Event filtering criteria
- Subscription parameter options
- Real-time query modifications
- Dynamic subscription updates
- Multi-tenant event isolation

Use Cases:
- Real-time UI updates
- Live dashboard implementations
- Event-driven architectures
- Data synchronization strategies
- Notification system integration

Example Output:
```
# Subscription Fields (4)
**Endpoint:** my-api
**Root Type:** Subscription

- **userUpdated**: `User` - Live user profile changes
- **messageAdded**: `Message` - New chat messages in real-time
- **orderStatusChanged**: `Order` - Order status notifications
- **systemAlert**: `Alert` - Critical system notifications
```

Note: Returns empty if schema has no subscription support.")]
    public static async Task<string> ListSubscriptionFields(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName)
    {
        return await ListRootFields(endpointName, "Subscription");
    }

    [McpServerTool, Description(@"Gets detailed definition for a specific subscription field with real-time streaming configuration.

This tool provides comprehensive subscription analysis including:

Subscription Definition:
- Complete SDL syntax for the subscription
- Event trigger conditions and filters
- Return type structure for events
- Real-time data flow patterns
- WebSocket connection requirements

Event Configuration:
- Event filtering parameters
- Subscription lifecycle management
- Rate limiting and throttling rules
- Authentication requirements
- Multi-tenant event isolation

Real-time Behavior:
- Event frequency and timing
- Data consistency guarantees
- Connection management strategies
- Error handling and reconnection
- Subscription cleanup procedures

Performance Considerations:
- Memory usage for active subscriptions
- Network bandwidth requirements
- Server resource consumption
- Client-side caching strategies
- Subscription optimization techniques

Use Cases:
- Real-time feature implementation
- Live data synchronization
- Event-driven UI updates
- Notification system design
- WebSocket integration planning

Example Output:
```
# Subscription Field: userUpdated

**Description:** Streams real-time updates when user profiles are modified

**Return Type:** `User`

## Arguments
- **userId**: `ID` - Filter updates for specific user (optional)
- **fields**: `[String]` - Specific fields to watch for changes

## SDL Definition
```graphql
userUpdated(userId: ID, fields: [String]): User
```

Event triggers: profile updates, role changes, status modifications")]
    public static async Task<string> GetSubscriptionField(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Name of the subscription field to get details for")]
        string fieldName)
    {
        return await GetRootFieldDefinition(endpointName, "Subscription", fieldName);
    }

    [McpServerTool, Description(@"Lists all types defined in the GraphQL schema with comprehensive type information and relationships.

This tool provides complete schema type discovery including:

Type Categories:
- Object types (entities with fields)
- Input types (mutation/query parameters)
- Enum types (predefined value sets)
- Scalar types (primitive data types)
- Interface types (shared field contracts)
- Union types (polymorphic return types)

Type Information:
- Type names and descriptions
- Field counts and complexity
- Inheritance relationships
- Implementation details
- Custom scalar definitions

Filtering Options:
- Filter by type kind (OBJECT, INPUT_OBJECT, ENUM, SCALAR, INTERFACE, UNION). Leave empty for all types
- Exclude internal GraphQL types
- Sort by name or category
- Group by type relationships
- Show deprecated types

Relationship Analysis:
- Interface implementations
- Union member types
- Field type dependencies
- Input/output type mappings
- Circular reference detection

Use Cases:
- Schema architecture understanding
- Type relationship mapping
- API surface area analysis
- Code generation planning
- Documentation generation

Example Output:
```
# Types in Schema: my-api

## OBJECT Types (12)
- **User** - User account information
- **Order** - Customer order details
- **Product** - Product catalog entries

## INPUT_OBJECT Types (8)  
- **CreateUserInput** - User creation parameters
- **UpdateOrderInput** - Order modification data

## ENUM Types (4)
- **UserRole** - Available user permission levels
- **OrderStatus** - Order processing states

**Total:** 24 types
```")]
    public static async Task<string> ListTypes(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Filter types by kind (OBJECT, INPUT_OBJECT, ENUM, SCALAR, INTERFACE, UNION). Leave empty for all types")]
        string? typeKind = null)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
        {
            return schemaResult.FormatForDisplay();
        }

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
        if (!TryGetSchemaTypes(schemaData, out var types))
        {
            return "Failed to parse schema types from introspection result.";
        }

        var result = new StringBuilder();
        result.AppendLine($"# Types in Schema: {endpointName}\n");

        var filteredTypes = types.EnumerateArray()
            .Where(type => {
                // Skip introspection types
                if (!type.TryGetProperty("name", out var name) || 
                    name.GetString()?.StartsWith("__") == true)
                    return false;

                // Filter by kind if specified
                if (!string.IsNullOrEmpty(typeKind) && 
                    type.TryGetProperty("kind", out var kind) &&
                    !kind.GetString()?.Equals(typeKind, StringComparison.OrdinalIgnoreCase) == true)
                    return false;

                return true;
            })
            .OrderBy(type => type.GetProperty("name").GetString())
            .ToList();

        if (filteredTypes.Count == 0)
        {
            var filterText = !string.IsNullOrEmpty(typeKind) ? $" of kind '{typeKind}'" : "";
            return $"No types{filterText} found in schema.";
        }

        // Group by type kind
        var typesByKind = filteredTypes
            .GroupBy(type => type.GetProperty("kind").GetString() ?? "UNKNOWN")
            .OrderBy(group => group.Key);

        foreach (var group in typesByKind)
        {
            result.AppendLine($"## {group.Key} Types ({group.Count()})");
            result.AppendLine();

            foreach (var type in group)
            {
                var typeName = type.GetProperty("name").GetString() ?? "Unknown";
                var description = type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String
                    ? desc.GetString()
                    : null;

                result.AppendLine($"- **{typeName}**" + 
                    (!string.IsNullOrEmpty(description) ? $" - {description}" : ""));
            }
            result.AppendLine();
        }

        result.AppendLine($"**Total:** {filteredTypes.Count} types");
        return result.ToString();
    }

    [McpServerTool, Description(@"Gets detailed definition for a specific GraphQL type in SDL format with complete field and relationship analysis.

This tool provides comprehensive type analysis including:

Type Definition:
- Complete SDL syntax formatting
- All fields with types and descriptions
- Interface implementations
- Union member relationships
- Enum values and meanings

Field Analysis:
- Field names, types, and nullability
- Parameter requirements and defaults
- Field descriptions and documentation
- Deprecation status and alternatives
- Complex nested type structures

Type Relationships:
- Interface inheritance chains
- Union type memberships
- Field type dependencies
- Input/output type mappings
- Circular reference detection

Validation Rules:
- Required field constraints
- Type compatibility rules
- Input validation requirements
- Custom scalar formats
- Business logic constraints

Use Cases:
- Understanding entity structures
- Planning data model integration
- Validating type compatibility
- Generating client code types
- API contract verification

Example Output:
```
# Type: User

**Kind:** OBJECT
**Description:** Represents a user account in the system

## Implements
- Node
- Timestamped

## Fields (8)
- **id**: `ID!` - Unique identifier
- **name**: `String!` - Full display name
- **email**: `String!` - Contact email address
- **role**: `UserRole!` - Permission level
- **createdAt**: `DateTime!` - Account creation timestamp
- **posts**: `[Post]` - User's blog posts

## SDL Definition
```graphql
type User implements Node & Timestamped {
  id: ID!
  name: String!
  email: String!
  role: UserRole!
  createdAt: DateTime!
  posts: [Post]
}
```")]
    public static async Task<string> GetType(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Name of the type to get details for")]
        string typeName)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
        {
            return schemaResult.FormatForDisplay();
        }

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
        if (!TryGetSchemaTypes(schemaData, out var types))
        {
            return "Failed to parse schema types from introspection result.";
        }

        var type = types.EnumerateArray()
            .FirstOrDefault(t => t.TryGetProperty("name", out var name) && 
                           name.GetString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true);

        if (type.ValueKind == JsonValueKind.Undefined)
        {
            return $"Type '{typeName}' not found in schema.";
        }

        return FormatTypeDefinitionDetailed(type);
    }

    [McpServerTool, Description(@"Gets a simplified list of fields with their types for a specific GraphQL object type with clear field mapping.

This tool provides focused field analysis including:

Field Overview:
- All field names with their exact types
- Nullable vs required field indicators
- Array/list type specifications
- Scalar vs object field types
- Field count and structure summary

Type Information:
- Simple type name mappings
- Complex nested type structures
- Custom scalar type usage
- Enum field type specifications
- Interface field inheritance

Field Categories:
- ID and identifier fields
- Data fields (strings, numbers, dates)
- Relationship fields (objects, arrays)
- Computed fields (derived values)
- Metadata fields (timestamps, flags)

Usability Features:
- Clean, scannable field list
- Type information without complexity
- Field descriptions when available
- Deprecation warnings highlighted
- Quick reference format

Use Cases:
- Quick field reference lookup
- Planning query field selection
- Understanding entity structure
- Validating available data points
- Client code generation input

Example Output:
```
# Fields for Type: User

**Type Kind:** OBJECT
**Description:** User account and profile information
**Field Count:** 8

## Fields

- **id**: `ID!` - Unique user identifier
- **name**: `String!` - User's full display name  
- **email**: `String!` - Primary contact email
- **role**: `UserRole!` - Account permission level
- **isActive**: `Boolean!` - Account status flag
- **createdAt**: `DateTime!` - Registration timestamp
- **posts**: `[Post]` - User's published content
- **avatar**: `String` - Profile image URL
```

Note: Only works with OBJECT, INPUT_OBJECT, and INTERFACE types.")]
    public static async Task<string> GetTypeFields(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Name of the object type to get fields for")]
        string typeName)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
        {
            return schemaResult.FormatForDisplay();
        }

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
        if (!TryGetSchemaTypes(schemaData, out var types))
        {
            return "Failed to parse schema types from introspection result.";
        }

        var type = types.EnumerateArray()
            .FirstOrDefault(t => t.TryGetProperty("name", out var name) && 
                           name.GetString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true);

        if (type.ValueKind == JsonValueKind.Undefined)
        {
            return $"Type '{typeName}' not found in schema.";
        }

        if (!type.TryGetProperty("kind", out var kind) || 
            (kind.GetString() != "OBJECT" && kind.GetString() != "INPUT_OBJECT" && kind.GetString() != "INTERFACE"))
        {
            return $"Type '{typeName}' is not an object type, input type, or interface. It is: {kind.GetString() ?? "unknown"}";
        }

        if (!type.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Array)
        {
            return $"Type '{typeName}' has no fields.";
        }

        var result = new StringBuilder();
        result.AppendLine($"# Fields for Type: {typeName}\n");

        var typeKind = kind.GetString();
        result.AppendLine($"**Type Kind:** {typeKind}");
        
        if (type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
        {
            result.AppendLine($"**Description:** {desc.GetString()}");
        }

        result.AppendLine($"**Field Count:** {fields.GetArrayLength()}");
        result.AppendLine();

        result.AppendLine("## Fields\n");

        foreach (var field in fields.EnumerateArray())
        {
            if (!field.TryGetProperty("name", out var fieldName))
                continue;

            var name = fieldName.GetString() ?? "unknown";
            var typeStr = field.TryGetProperty("type", out var fieldType) 
                ? FormatTypeReference(fieldType) 
                : "unknown";

            var line = $"- **{name}**: `{typeStr}`";

            if (field.TryGetProperty("description", out var fieldDesc) && fieldDesc.ValueKind == JsonValueKind.String)
            {
                line += $" - {fieldDesc.GetString()}";
            }

            if (field.TryGetProperty("isDeprecated", out var deprecated) && deprecated.GetBoolean())
            {
                line += " **[DEPRECATED]**";
                if (field.TryGetProperty("deprecationReason", out var reason) && reason.ValueKind == JsonValueKind.String)
                {
                    line += $" - {reason.GetString()}";
                }
            }

            result.AppendLine(line);
        }

        return result.ToString();
    }

    [McpServerTool, Description(@"Searches for types or fields in the schema by name pattern with powerful regex-based discovery and comprehensive results.

This tool provides advanced schema search capabilities including:

Search Capabilities:
- Case-insensitive regex pattern matching
- Search types, fields, or both simultaneously
- Partial name matching and wildcards
- Complex pattern expressions
- Grouped and organized results

Search Targets:
- **types**: Search only type names (User, Order, etc.)
- **fields**: Search only field names (id, name, email, etc.)
- **both**: Search types and fields together

Pattern Examples:
- 'user' - Find anything containing 'user'
- '^get.*' - Find items starting with 'get'
- '.*Product.*' - Find items containing 'Product'
- 'create|update|delete' - Find CRUD operations
- '.*Id$' - Find fields ending with 'Id'

Result Organization:
- Type matches grouped by kind (OBJECT, ENUM, etc.)
- Field matches grouped by parent type
- Descriptions and context included
- Match counts and summaries
- Relevance-based ordering

Use Cases:
- API discovery and exploration
- Finding related operations
- Identifying naming patterns
- Locating specific functionality
- Schema navigation assistance

Example Output:
```
# Search Results for: 'user.*'

**Endpoint:** my-api
**Search Target:** both

## Type Matches (3)
- **User** (OBJECT) - User account information
- **UserRole** (ENUM) - Available permission levels
- **UserInput** (INPUT_OBJECT) - User creation parameters

## Field Matches (8)
### Query
- **getUser**: `User` - Retrieve user by ID
- **getUsersByRole**: `[User]` - Filter users by role

### Mutation  
- **createUser**: `User` - Create new user account
- **updateUser**: `User` - Modify user information

**Summary:** 3 type matches, 8 field matches
```")]
    public static async Task<string> SearchSchema(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Search pattern (case-insensitive regex). Examples: 'user', '^get.*', '.*Product.*'")]
        string searchPattern,
        [Description("What to search for: 'types', 'fields', or 'both'")]
        string searchTarget = "both")
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        if (!IsValidSearchTarget(searchTarget))
        {
            return "Invalid search target. Must be 'types', 'fields', or 'both'.";
        }

        try
        {
            var regex = new Regex(searchPattern, RegexOptions.IgnoreCase);
            
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
            {
                return schemaResult.FormatForDisplay();
            }

            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
            if (!TryGetSchemaTypes(schemaData, out var types))
            {
                return "Failed to parse schema types from introspection result.";
            }

            var result = new StringBuilder();
            result.AppendLine($"# Search Results for: '{searchPattern}'\n");
            result.AppendLine($"**Endpoint:** {endpointName}");
            result.AppendLine($"**Search Target:** {searchTarget}");
            result.AppendLine();

            var typeMatches = new List<(string name, string kind, string? description)>();
            var fieldMatches = new List<(string typeName, string fieldName, string fieldType, string? description)>();

            foreach (var type in types.EnumerateArray())
            {
                if (!type.TryGetProperty("name", out var nameElement) ||
                    nameElement.GetString()?.StartsWith("__") == true)
                    continue;

                var typeName = nameElement.GetString() ?? "";
                var typeKind = type.TryGetProperty("kind", out var kindElement) ? kindElement.GetString() ?? "" : "";
                var typeDesc = type.TryGetProperty("description", out var descElement) && descElement.ValueKind == JsonValueKind.String
                    ? descElement.GetString()
                    : null;

                // Search types
                if ((searchTarget == "types" || searchTarget == "both") && regex.IsMatch(typeName))
                {
                    typeMatches.Add((typeName, typeKind, typeDesc));
                }

                // Search fields
                if ((searchTarget == "fields" || searchTarget == "both") && 
                    type.TryGetProperty("fields", out var fields) && 
                    fields.ValueKind == JsonValueKind.Array)
                {
                    foreach (var field in fields.EnumerateArray())
                    {
                        if (!field.TryGetProperty("name", out var fieldNameElement))
                            continue;

                        var fieldName = fieldNameElement.GetString() ?? "";
                        if (regex.IsMatch(fieldName))
                        {
                            var fieldType = field.TryGetProperty("type", out var fieldTypeElement)
                                ? FormatTypeReference(fieldTypeElement)
                                : "unknown";
                            var fieldDesc = field.TryGetProperty("description", out var fieldDescElement) && fieldDescElement.ValueKind == JsonValueKind.String
                                ? fieldDescElement.GetString()
                                : null;

                            fieldMatches.Add((typeName, fieldName, fieldType, fieldDesc));
                        }
                    }
                }
            }

            // Display type matches
            if (typeMatches.Any())
            {
                result.AppendLine($"## Type Matches ({typeMatches.Count})\n");
                foreach (var (name, kind, description) in typeMatches.OrderBy(t => t.name))
                {
                    result.AppendLine($"- **{name}** ({kind})" + 
                        (!string.IsNullOrEmpty(description) ? $" - {description}" : ""));
                }
                result.AppendLine();
            }

            // Display field matches
            if (fieldMatches.Any())
            {
                result.AppendLine($"## Field Matches ({fieldMatches.Count})\n");
                var groupedFields = fieldMatches.GroupBy(f => f.typeName).OrderBy(g => g.Key);
                
                foreach (var group in groupedFields)
                {
                    result.AppendLine($"### {group.Key}");
                    foreach (var (_, fieldName, fieldType, description) in group.OrderBy(f => f.fieldName))
                    {
                        result.AppendLine($"- **{fieldName}**: `{fieldType}`" + 
                            (!string.IsNullOrEmpty(description) ? $" - {description}" : ""));
                    }
                    result.AppendLine();
                }
            }

            // Summary
            if (!typeMatches.Any() && !fieldMatches.Any())
            {
                result.AppendLine("No matches found for the given search pattern.");
            }
            else
            {
                result.AppendLine($"**Summary:** {typeMatches.Count} type matches, {fieldMatches.Count} field matches");
            }

            return result.ToString();
        }
        catch (ArgumentException ex)
        {
            return $"Invalid regex pattern: {ex.Message}";
        }
    }

    [McpServerTool, Description("Perform comprehensive schema exploration with intelligent analysis, field relationships, usage patterns, and development recommendations. This advanced tool provides deep schema insights for API discovery, development planning, and architectural analysis.")]
    public static async Task<string> ExploreSchemaComprehensive(
        [Description("Name of the registered GraphQL endpoint. Use GetAllEndpoints to see available endpoints")]
        string endpointName,
        [Description("Exploration focus: 'overview' for general analysis, 'development' for dev recommendations, 'architecture' for structural analysis")]
        string focusArea = "overview",
        [Description("Include field usage analytics and patterns")]
        bool includeUsageAnalytics = true,
        [Description("Include architectural recommendations and best practices")]
        bool includeArchitecturalAnalysis = true,
        [Description("Maximum depth for type relationship analysis")]
        int maxRelationshipDepth = 3)
    {
        try
        {
            var smartResponse = await SmartResponseService.Instance.CreateSchemaExplorationResponseAsync(
                endpointName, focusArea, includeUsageAnalytics, includeArchitecturalAnalysis, maxRelationshipDepth);
            
            return await SmartResponseService.Instance.FormatComprehensiveResponseAsync(smartResponse);
        }
        catch (Exception ex)
        {
            return await SmartResponseService.Instance.CreateErrorResponseAsync(
                "SchemaExplorationError", 
                ex.Message,
                new { endpointName, focusArea, maxRelationshipDepth });
        }
    }

    // Helper methods

    private static async Task<string> ListRootFields(string endpointName, string operationType)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
        {
            return schemaResult.FormatForDisplay();
        }

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
        if (!TryGetRootType(schemaData, operationType, out var rootTypeName))
        {
            return $"Schema does not have a {operationType} root type.";
        }

        if (!TryGetSchemaTypes(schemaData, out var types))
        {
            return "Failed to parse schema types from introspection result.";
        }

        var rootType = types.EnumerateArray()
            .FirstOrDefault(t => t.TryGetProperty("name", out var name) && 
                           name.GetString() == rootTypeName);

        if (rootType.ValueKind == JsonValueKind.Undefined)
        {
            return $"{operationType} root type '{rootTypeName}' not found in schema types.";
        }

        if (!rootType.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Array)
        {
            return $"No {operationType.ToLower()} fields found.";
        }

        var result = new StringBuilder();
        result.AppendLine($"# {operationType} Fields ({fields.GetArrayLength()})\n");
        result.AppendLine($"**Endpoint:** {endpointName}");
        result.AppendLine($"**Root Type:** {rootTypeName}");
        result.AppendLine();

        foreach (var field in fields.EnumerateArray())
        {
            if (!field.TryGetProperty("name", out var fieldName))
                continue;

            var name = fieldName.GetString() ?? "unknown";
            var returnType = field.TryGetProperty("type", out var type) 
                ? FormatTypeReference(type) 
                : "unknown";

            var line = $"- **{name}**: `{returnType}`";

            if (field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
            {
                line += $" - {desc.GetString()}";
            }

            if (field.TryGetProperty("isDeprecated", out var deprecated) && deprecated.GetBoolean())
            {
                line += " **[DEPRECATED]**";
            }

            result.AppendLine(line);
        }

        return result.ToString();
    }

    private static async Task<string> GetRootFieldDefinition(string endpointName, string operationType, string fieldName)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
        if (!schemaResult.IsSuccess)
        {
            return schemaResult.FormatForDisplay();
        }

        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaResult.Content!);
        if (!TryGetRootType(schemaData, operationType, out var rootTypeName))
        {
            return $"Schema does not have a {operationType} root type.";
        }

        if (!TryGetSchemaTypes(schemaData, out var types))
        {
            return "Failed to parse schema types from introspection result.";
        }

        var rootType = types.EnumerateArray()
            .FirstOrDefault(t => t.TryGetProperty("name", out var name) && 
                           name.GetString() == rootTypeName);

        if (rootType.ValueKind == JsonValueKind.Undefined)
        {
            return $"{operationType} root type '{rootTypeName}' not found in schema types.";
        }

        if (!rootType.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Array)
        {
            return $"No {operationType.ToLower()} fields found.";
        }

        var field = fields.EnumerateArray()
            .FirstOrDefault(f => f.TryGetProperty("name", out var name) && 
                           name.GetString()?.Equals(fieldName, StringComparison.OrdinalIgnoreCase) == true);

        if (field.ValueKind == JsonValueKind.Undefined)
        {
            return $"{operationType} field '{fieldName}' not found.";
        }

        return FormatFieldDefinitionDetailed(field, operationType);
    }

    private static string FormatFieldDefinitionDetailed(JsonElement field, string operationType)
    {
        var result = new StringBuilder();
        
        var fieldName = field.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "unknown" : "unknown";
        result.AppendLine($"# {operationType} Field: {fieldName}\n");

        if (field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
        {
            result.AppendLine($"**Description:** {desc.GetString()}\n");
        }

        // Return type
        if (field.TryGetProperty("type", out var returnType))
        {
            result.AppendLine($"**Return Type:** `{FormatTypeReference(returnType)}`\n");
        }

        // Arguments
        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array && args.GetArrayLength() > 0)
        {
            result.AppendLine("## Arguments\n");
            foreach (var arg in args.EnumerateArray())
            {
                if (!arg.TryGetProperty("name", out var argName))
                    continue;

                var name = argName.GetString() ?? "unknown";
                var type = arg.TryGetProperty("type", out var argType) 
                    ? FormatTypeReference(argType) 
                    : "unknown";

                var line = $"- **{name}**: `{type}`";

                if (arg.TryGetProperty("description", out var argDesc) && argDesc.ValueKind == JsonValueKind.String)
                {
                    line += $" - {argDesc.GetString()}";
                }

                if (arg.TryGetProperty("defaultValue", out var defaultValue) && defaultValue.ValueKind != JsonValueKind.Null)
                {
                    line += $" (default: `{defaultValue.GetRawText()}`)";
                }

                result.AppendLine(line);
            }
            result.AppendLine();
        }

        // Deprecation info
        if (field.TryGetProperty("isDeprecated", out var deprecated) && deprecated.GetBoolean())
        {
            result.AppendLine("## Deprecation\n");
            result.AppendLine("⚠️ **This field is deprecated.**");
            
            if (field.TryGetProperty("deprecationReason", out var reason) && reason.ValueKind == JsonValueKind.String)
            {
                result.AppendLine($"\n**Reason:** {reason.GetString()}");
            }
            result.AppendLine();
        }

        // SDL Format
        result.AppendLine("## SDL Definition\n");
        result.AppendLine("```graphql");
        result.AppendLine(FormatFieldAsSdl(field));
        result.AppendLine("```");

        return result.ToString();
    }

    private static string FormatTypeDefinitionDetailed(JsonElement type)
    {
        var result = new StringBuilder();
        
        var typeName = type.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "unknown" : "unknown";
        var typeKind = type.TryGetProperty("kind", out var kindElement) ? kindElement.GetString() ?? "unknown" : "unknown";
        
        result.AppendLine($"# Type: {typeName}\n");
        result.AppendLine($"**Kind:** {typeKind}");

        if (type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
        {
            result.AppendLine($"**Description:** {desc.GetString()}");
        }

        result.AppendLine();

        switch (typeKind)
        {
            case "OBJECT":
            case "INPUT_OBJECT":
            case "INTERFACE":
                FormatObjectTypeDetails(type, result);
                break;
            case "ENUM":
                FormatEnumTypeDetails(type, result);
                break;
            case "UNION":
                FormatUnionTypeDetails(type, result);
                break;
            case "SCALAR":
                result.AppendLine("This is a scalar type.");
                break;
        }

        // SDL Format
        result.AppendLine("## SDL Definition\n");
        result.AppendLine("```graphql");
        result.AppendLine(FormatTypeAsSdl(type));
        result.AppendLine("```");

        return result.ToString();
    }

    private static void FormatObjectTypeDetails(JsonElement type, StringBuilder result)
    {
        // Interfaces (for OBJECT types)
        if (type.TryGetProperty("interfaces", out var interfaces) && interfaces.ValueKind == JsonValueKind.Array && interfaces.GetArrayLength() > 0)
        {
            result.AppendLine("## Implements\n");
            foreach (var iface in interfaces.EnumerateArray())
            {
                if (iface.TryGetProperty("name", out var ifaceName))
                {
                    result.AppendLine($"- {ifaceName.GetString()}");
                }
            }
            result.AppendLine();
        }

        // Fields
        if (type.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine($"## Fields ({fields.GetArrayLength()})\n");
            foreach (var field in fields.EnumerateArray())
            {
                if (!field.TryGetProperty("name", out var fieldName))
                    continue;

                var name = fieldName.GetString() ?? "unknown";
                var fieldType = field.TryGetProperty("type", out var fieldTypeElement) 
                    ? FormatTypeReference(fieldTypeElement) 
                    : "unknown";

                var line = $"- **{name}**: `{fieldType}`";

                if (field.TryGetProperty("description", out var fieldDesc) && fieldDesc.ValueKind == JsonValueKind.String)
                {
                    line += $" - {fieldDesc.GetString()}";
                }

                if (field.TryGetProperty("isDeprecated", out var deprecated) && deprecated.GetBoolean())
                {
                    line += " **[DEPRECATED]**";
                }

                result.AppendLine(line);
            }
            result.AppendLine();
        }

        // Input fields (for INPUT_OBJECT types)
        if (type.TryGetProperty("inputFields", out var inputFields) && inputFields.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine($"## Input Fields ({inputFields.GetArrayLength()})\n");
            foreach (var field in inputFields.EnumerateArray())
            {
                if (!field.TryGetProperty("name", out var fieldName))
                    continue;

                var name = fieldName.GetString() ?? "unknown";
                var fieldType = field.TryGetProperty("type", out var fieldTypeElement) 
                    ? FormatTypeReference(fieldTypeElement) 
                    : "unknown";

                var line = $"- **{name}**: `{fieldType}`";

                if (field.TryGetProperty("description", out var fieldDesc) && fieldDesc.ValueKind == JsonValueKind.String)
                {
                    line += $" - {fieldDesc.GetString()}";
                }

                if (field.TryGetProperty("defaultValue", out var defaultValue) && defaultValue.ValueKind != JsonValueKind.Null)
                {
                    line += $" (default: `{defaultValue.GetRawText()}`)";
                }

                result.AppendLine(line);
            }
            result.AppendLine();
        }
    }

    private static void FormatEnumTypeDetails(JsonElement type, StringBuilder result)
    {
        if (type.TryGetProperty("enumValues", out var enumValues) && enumValues.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine($"## Values ({enumValues.GetArrayLength()})\n");
            foreach (var value in enumValues.EnumerateArray())
            {
                if (!value.TryGetProperty("name", out var valueName))
                    continue;

                var name = valueName.GetString() ?? "unknown";
                var line = $"- **{name}**";

                if (value.TryGetProperty("description", out var valueDesc) && valueDesc.ValueKind == JsonValueKind.String)
                {
                    line += $" - {valueDesc.GetString()}";
                }

                if (value.TryGetProperty("isDeprecated", out var deprecated) && deprecated.GetBoolean())
                {
                    line += " **[DEPRECATED]**";
                    if (value.TryGetProperty("deprecationReason", out var reason) && reason.ValueKind == JsonValueKind.String)
                    {
                        line += $" - {reason.GetString()}";
                    }
                }

                result.AppendLine(line);
            }
            result.AppendLine();
        }
    }

    private static void FormatUnionTypeDetails(JsonElement type, StringBuilder result)
    {
        if (type.TryGetProperty("possibleTypes", out var possibleTypes) && possibleTypes.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine($"## Possible Types ({possibleTypes.GetArrayLength()})\n");
            foreach (var possibleType in possibleTypes.EnumerateArray())
            {
                if (possibleType.TryGetProperty("name", out var typeName))
                {
                    result.AppendLine($"- {typeName.GetString()}");
                }
            }
            result.AppendLine();
        }
    }

    private static string FormatTypeReference(JsonElement typeRef)
    {
        if (!typeRef.TryGetProperty("kind", out var kind))
            return "unknown";

        return kind.GetString() switch
        {
            "NON_NULL" => typeRef.TryGetProperty("ofType", out var ofType) 
                ? FormatTypeReference(ofType) + "!" 
                : "unknown!",
            "LIST" => typeRef.TryGetProperty("ofType", out var listOfType) 
                ? "[" + FormatTypeReference(listOfType) + "]" 
                : "[unknown]",
            _ => typeRef.TryGetProperty("name", out var name) 
                ? name.GetString() ?? "unknown" 
                : "unknown"
        };
    }

    private static string FormatFieldAsSdl(JsonElement field)
    {
        var result = new StringBuilder();
        
        var fieldName = field.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "unknown" : "unknown";
        result.Append(fieldName);

        // Arguments
        if (field.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Array && args.GetArrayLength() > 0)
        {
            result.Append("(");
            var argList = new List<string>();
            
            foreach (var arg in args.EnumerateArray())
            {
                if (!arg.TryGetProperty("name", out var argName))
                    continue;

                var name = argName.GetString() ?? "unknown";
                var type = arg.TryGetProperty("type", out var argType) 
                    ? FormatTypeReference(argType) 
                    : "unknown";

                var argStr = $"{name}: {type}";
                
                if (arg.TryGetProperty("defaultValue", out var defaultValue) && defaultValue.ValueKind != JsonValueKind.Null)
                {
                    argStr += $" = {defaultValue.GetRawText()}";
                }

                argList.Add(argStr);
            }
            
            result.Append(string.Join(", ", argList));
            result.Append(")");
        }

        // Return type
        if (field.TryGetProperty("type", out var returnType))
        {
            result.Append($": {FormatTypeReference(returnType)}");
        }

        return result.ToString();
    }

    private static string FormatTypeAsSdl(JsonElement type)
    {
        var result = new StringBuilder();
        
        var typeName = type.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "unknown" : "unknown";
        var typeKind = type.TryGetProperty("kind", out var kindElement) ? kindElement.GetString() ?? "unknown" : "unknown";

        switch (typeKind)
        {
            case "OBJECT":
                result.Append($"type {typeName}");
                if (type.TryGetProperty("interfaces", out var interfaces) && interfaces.ValueKind == JsonValueKind.Array && interfaces.GetArrayLength() > 0)
                {
                    var implementsList = interfaces.EnumerateArray()
                        .Select(i => i.TryGetProperty("name", out var name) ? name.GetString() : null)
                        .Where(n => n != null)
                        .ToList();
                    
                    if (implementsList.Any())
                    {
                        result.Append($" implements {string.Join(" & ", implementsList)}");
                    }
                }
                break;
            case "INPUT_OBJECT":
                result.Append($"input {typeName}");
                break;
            case "INTERFACE":
                result.Append($"interface {typeName}");
                break;
            case "ENUM":
                result.Append($"enum {typeName}");
                break;
            case "UNION":
                result.Append($"union {typeName}");
                if (type.TryGetProperty("possibleTypes", out var possibleTypes) && possibleTypes.ValueKind == JsonValueKind.Array)
                {
                    var typesList = possibleTypes.EnumerateArray()
                        .Select(t => t.TryGetProperty("name", out var name) ? name.GetString() : null)
                        .Where(n => n != null)
                        .ToList();
                    
                    if (typesList.Any())
                    {
                        result.Append($" = {string.Join(" | ", typesList)}");
                    }
                }
                return result.ToString(); // Union types don't have field blocks
            case "SCALAR":
                result.Append($"scalar {typeName}");
                return result.ToString(); // Scalar types don't have field blocks
        }

        // Add field/value block for types that have them
        if (typeKind == "ENUM")
        {
            result.AppendLine(" {");
            if (type.TryGetProperty("enumValues", out var enumValues) && enumValues.ValueKind == JsonValueKind.Array)
            {
                foreach (var value in enumValues.EnumerateArray())
                {
                    if (value.TryGetProperty("name", out var valueName))
                    {
                        result.AppendLine($"  {valueName.GetString()}");
                    }
                }
            }
            result.Append("}");
        }
        else if (type.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine(" {");
            foreach (var field in fields.EnumerateArray())
            {
                result.AppendLine($"  {FormatFieldAsSdl(field)}");
            }
            result.Append("}");
        }
        else if (type.TryGetProperty("inputFields", out var inputFields) && inputFields.ValueKind == JsonValueKind.Array)
        {
            result.AppendLine(" {");
            foreach (var field in inputFields.EnumerateArray())
            {
                result.AppendLine($"  {FormatFieldAsSdl(field)}");
            }
            result.Append("}");
        }

        return result.ToString();
    }

    private static bool TryGetSchemaTypes(JsonElement schemaData, out JsonElement types)
    {
        types = default;
        return schemaData.TryGetProperty("data", out var data) &&
               data.TryGetProperty("__schema", out var schema) &&
               schema.TryGetProperty("types", out types);
    }

    private static bool TryGetRootType(JsonElement schemaData, string operationType, out string? rootTypeName)
    {
        rootTypeName = null;
        
        if (!schemaData.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("__schema", out var schema))
        {
            return false;
        }

        var propertyName = operationType.ToLower() + "Type";
        if (!schema.TryGetProperty(propertyName, out var rootType) || 
            rootType.ValueKind == JsonValueKind.Null)
        {
            return false;
        }

        if (!rootType.TryGetProperty("name", out var name))
        {
            return false;
        }

        rootTypeName = name.GetString();
        return !string.IsNullOrEmpty(rootTypeName);
    }

    private static bool IsValidSearchTarget(string target)
    {
        return target is "types" or "fields" or "both";
    }
}
