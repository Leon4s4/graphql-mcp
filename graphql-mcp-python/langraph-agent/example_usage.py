"""Example usage of the C# to GraphQL migration agent."""

import asyncio
import json
from csharp_to_graphql_agent import create_agent
from config import ENV_CONFIG

# Example C# code with REST API calls
EXAMPLE_CSHARP_CODE = '''
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class UserService
{
    private readonly HttpClient _httpClient;
    
    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<User> GetUserAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(content);
    }
    
    public async Task<List<Post>> GetUserPostsAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}/posts");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Post>>(content);
    }
    
    public async Task<List<Comment>> GetUserCommentsAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}/comments");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Comment>>(content);
    }
    
    public async Task<UserProfile> GetUserProfileAsync(int userId)
    {
        // Multiple REST calls that could be combined
        var user = await GetUserAsync(userId);
        var posts = await GetUserPostsAsync(userId);
        var comments = await GetUserCommentsAsync(userId);
        
        return new UserProfile
        {
            User = user,
            Posts = posts,
            Comments = comments,
            TotalContent = posts.Count + comments.Count
        };
    }
    
    public async Task<Product> CreateProductAsync(Product product)
    {
        var json = JsonSerializer.Serialize(product);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/products", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(responseContent);
    }
    
    public async Task<List<Order>> GetUserOrdersAsync(int userId, int pageSize = 10, int pageNumber = 1)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}/orders?pageSize={pageSize}&page={pageNumber}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Order>>(content);
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int AuthorId { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserProfile
{
    public User User { get; set; }
    public List<Post> Posts { get; set; }
    public List<Comment> Comments { get; set; }
    public int TotalContent { get; set; }
}
'''

async def main():
    """Main example function."""
    print("🚀 C# to GraphQL Migration Agent Example")
    print("=" * 50)
    
    # Create the agent with LM Studio configuration
    print(f"Creating agent with LM Studio at {ENV_CONFIG.llm_base_url}")
    print(f"Using model: {ENV_CONFIG.model_name}")
    
    agent = create_agent(
        llm_base_url=ENV_CONFIG.llm_base_url,
        model_name=ENV_CONFIG.model_name
    )
    
    print("\n📝 Analyzing C# code...")
    print("-" * 30)
    
    # Run the migration
    result = agent.migrate_csharp_code(EXAMPLE_CSHARP_CODE)
    
    if result["success"]:
        print("✅ Migration analysis completed successfully!")
        
        # Display results
        print(f"\n📊 Analysis Results:")
        print(f"- REST Calls Found: {len(result['rest_calls'])}")
        print(f"- Entities Identified: {len(result['entities'])}")
        print(f"- GraphQL Queries Generated: {len(result['graphql_queries'])}")
        
        # Show REST calls found
        if result['rest_calls']:
            print(f"\n🔍 REST Calls Identified:")
            for i, call in enumerate(result['rest_calls'], 1):
                print(f"  {i}. {call['method']} {call['endpoint']} - {call['purpose']}")
        
        # Show entities found
        if result['entities']:
            print(f"\n🏗️ Entities Identified:")
            for entity in result['entities']:
                print(f"  - {entity}")
        
        # Show generated GraphQL queries
        if result['graphql_queries']:
            print(f"\n⚡ Generated GraphQL Queries:")
            for i, query in enumerate(result['graphql_queries'], 1):
                print(f"\n  Query {i}: {query['name']}")
                print(f"  Replaces: {', '.join(query['replaces_rest_calls'])}")
                print(f"  Performance: {query['performance_improvement']}")
                print(f"  GraphQL Query:")
                print("  ```graphql")
                for line in query['query'].split('\n'):
                    print(f"  {line}")
                print("  ```")
                
                if query.get('variables'):
                    print(f"  Variables: {json.dumps(query['variables'], indent=2)}")
        
        # Show migration plan
        if result['migration_plan']:
            print(f"\n📋 Migration Plan:")
            try:
                plan = json.loads(result['migration_plan'])
                
                if 'migration_steps' in plan:
                    print("  Steps:")
                    for step in plan['migration_steps']:
                        print(f"    {step}")
                
                if 'performance_benefits' in plan:
                    benefits = plan['performance_benefits']
                    print(f"\n  📈 Performance Benefits:")
                    print(f"    - REST calls before: {benefits['rest_calls_before']}")
                    print(f"    - GraphQL queries after: {benefits['graphql_queries_after']}")
                    print(f"    - Network reduction: {benefits['network_reduction_percent']}%")
                    print(f"    - Estimated gain: {benefits['estimated_performance_gain']}")
                
                if 'recommendations' in plan:
                    print(f"\n  💡 Recommendations:")
                    for rec in plan['recommendations']:
                        print(f"    - {rec}")
                
                if 'migration_code_example' in plan:
                    print(f"\n  💻 Migration Code Example:")
                    print("  ```csharp")
                    for line in plan['migration_code_example'].split('\n'):
                        print(f"  {line}")
                    print("  ```")
                    
            except json.JSONDecodeError:
                print("  (Raw migration plan data)")
                print(f"  {result['migration_plan']}")
        
        # Show conversation messages
        if result.get('messages'):
            print(f"\n💬 Agent Conversation Summary:")
            for i, message in enumerate(result['messages'], 1):
                if message and len(message) > 100:
                    print(f"  {i}. {message[:100]}...")
                elif message:
                    print(f"  {i}. {message}")
    
    else:
        print("❌ Migration analysis failed!")
        print(f"Error: {result.get('error', 'Unknown error')}")

def test_simple_case():
    """Test with a simpler C# code example."""
    simple_code = '''
public class SimpleService
{
    private readonly HttpClient _client;
    
    public async Task<User> GetUserAsync(int id)
    {
        var response = await _client.GetAsync($"/api/users/{id}");
        return await response.Content.ReadFromJsonAsync<User>();
    }
}

public class User 
{
    public int Id { get; set; }
    public string Name { get; set; }
}
'''
    
    print("\n🧪 Testing Simple Case")
    print("=" * 30)
    
    agent = create_agent()
    result = agent.migrate_csharp_code(simple_code)
    
    if result["success"]:
        print("✅ Simple test passed!")
        print(f"REST calls: {len(result['rest_calls'])}")
        print(f"GraphQL queries: {len(result['graphql_queries'])}")
    else:
        print("❌ Simple test failed!")
        print(f"Error: {result.get('error')}")

if __name__ == "__main__":
    # Run the main example
    asyncio.run(main())
    
    # Run simple test
    test_simple_case()
    
    print("\n🎉 Example completed!")
    print("\n📚 Next Steps:")
    print("1. Ensure LM Studio is running with Gemma 3 model")
    print("2. Adjust the model name in config.py if needed")
    print("3. Customize the agent for your specific use case")
    print("4. Integrate with your existing MCP tools")