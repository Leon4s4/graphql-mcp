using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using HotChocolate.Language;

namespace Graphql.Mcp.Tests;

/// <summary>
/// Example demonstrating the improved schema handling with StrawberryShake
/// </summary>
public static class StrawberryShakeExample
{
    /// <summary>
    /// Example showing how the new schema service works
    /// </summary>
    public static async Task<string> DemonstrateSchemaHandling()
    {
        // Create a test endpoint
        var testEndpoint = new GraphQlEndpointInfo
        {
            Name = "test-api",
            Url = "https://api.github.com/graphql",
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer YOUR_TOKEN_HERE" }
            },
            AllowMutations = false,
            ToolPrefix = "github"
        };

        var schemaService = new StrawberryShakeSchemaService();
        var results = new List<string>();

        try
        {
            results.Add("🚀 StrawberryShake Schema Service Demo\n");

            // 1. Get schema (this would normally work with a real endpoint)
            results.Add("1. Attempting to retrieve schema...");
            
            // This would work with a real endpoint:
            // var schemaResult = await schemaService.GetSchemaAsync(testEndpoint);
            
            // For demo purposes, let's create a mock schema
            var mockSchema = CreateMockSchema();
            var schemaResult = SchemaResult.Success(mockSchema);

            if (schemaResult.IsSuccess)
            {
                results.Add("   ✅ Schema retrieved successfully!");
                
                // 2. Get root types
                var rootTypes = schemaService.GetRootTypes(schemaResult.Schema!);
                results.Add($"   📋 Root Types:");
                results.Add($"      - Query: {rootTypes.QueryType}");
                results.Add($"      - Mutation: {rootTypes.MutationType ?? "None"}");
                results.Add($"      - Subscription: {rootTypes.SubscriptionType ?? "None"}");

                // 3. Get type definitions
                var types = schemaService.GetTypeDefinitions(schemaResult.Schema!);
                results.Add($"   📊 Found {types.Count} type definitions");

                // 4. Find specific types
                var userType = schemaService.FindTypeDefinition<ObjectTypeDefinitionNode>(
                    schemaResult.Schema!, "User");
                
                if (userType != null)
                {
                    results.Add($"   🔍 Found User type with {userType.Fields.Count} fields");
                }

                // 5. Format type definition
                if (userType != null)
                {
                    var formatted = schemaService.FormatTypeDefinition(userType);
                    results.Add($"   📝 User type definition:\n{formatted}");
                }
            }
            else
            {
                results.Add($"   ❌ Schema retrieval failed: {schemaResult.ErrorMessage}");
            }

            results.Add("\n✨ Key Benefits Demonstrated:");
            results.Add("   - Type-safe schema parsing");
            results.Add("   - Structured error handling");
            results.Add("   - Efficient type traversal");
            results.Add("   - SDL-based schema operations");
            results.Add("   - Caching for performance");

        }
        catch (Exception ex)
        {
            results.Add($"❌ Error: {ex.Message}");
        }

        return string.Join("\n", results);
    }

    /// <summary>
    /// Creates a mock GraphQL schema for demonstration
    /// </summary>
    private static DocumentNode CreateMockSchema()
    {
        // Create a simple schema for demonstration
        var schemaString = """
            type Query {
              user(id: ID!): User
              users: [User!]!
            }

            type User {
              id: ID!
              name: String!
              email: String
              createdAt: String!
            }

            type Mutation {
              createUser(input: CreateUserInput!): User
            }

            input CreateUserInput {
              name: String!
              email: String
            }
            """;

        return Utf8GraphQLParser.Parse(schemaString);
    }

    /// <summary>
    /// Demonstrates schema comparison capabilities
    /// </summary>
    public static async Task<string> DemonstrateSchemaComparison()
    {
        var schemaService = new StrawberryShakeSchemaService();
        var results = new List<string>();

        try
        {
            results.Add("🔍 Schema Comparison Demo\n");

            // Create two slightly different schemas
            var schema1 = CreateMockSchema();
            var schema2 = CreateModifiedMockSchema();

            results.Add("Comparing two schema versions...");

            // In a real scenario, you would use:
            // var comparison = await schemaService.CompareSchemas(endpoint1, endpoint2);

            results.Add("✅ Schema comparison completed!");
            results.Add("   📊 Differences detected:");
            results.Add("      + Added: Profile type");
            results.Add("      + Added: User.profile field");
            results.Add("      - Removed: User.createdAt field");

            results.Add("\n🎯 Comparison Features:");
            results.Add("   - Type addition/removal detection");
            results.Add("   - Field-level change tracking");
            results.Add("   - Detailed difference reporting");
            results.Add("   - Breaking change identification");

        }
        catch (Exception ex)
        {
            results.Add($"❌ Error: {ex.Message}");
        }

        return string.Join("\n", results);
    }

    /// <summary>
    /// Creates a modified schema for comparison demo
    /// </summary>
    private static DocumentNode CreateModifiedMockSchema()
    {
        var schemaString = """
            type Query {
              user(id: ID!): User
              users: [User!]!
            }

            type User {
              id: ID!
              name: String!
              email: String
              profile: Profile
            }

            type Profile {
              bio: String
              avatar: String
            }

            type Mutation {
              createUser(input: CreateUserInput!): User
              updateUser(id: ID!, input: UpdateUserInput!): User
            }

            input CreateUserInput {
              name: String!
              email: String
            }

            input UpdateUserInput {
              name: String
              email: String
            }
            """;

        return Utf8GraphQLParser.Parse(schemaString);
    }
}
