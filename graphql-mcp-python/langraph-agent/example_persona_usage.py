"""Example usage of the persona-based migration agent."""

import os
from persona_migration_agent import create_persona_agent

def example_graphql_migration():
    """Example: Migrate REST API to GraphQL using persona approach."""
    
    # Create the persona-based agent
    # Set use_online_models=False to use local LLM instead
    agent = create_persona_agent(use_online_models=True)
    
    # Define the migration request
    user_request = """
    I need to migrate our C# web application from REST API endpoints to GraphQL.
    
    Current situation:
    - We have multiple REST endpoints for user management, product catalog, and order processing
    - Each page makes 3-5 separate API calls to load data
    - Performance is suffering due to over-fetching and multiple round trips
    - Frontend developers are frustrated with the rigid API structure
    
    Goals:
    - Reduce API calls by combining related data fetches
    - Allow frontend to request only needed fields
    - Improve loading performance
    - Maintain backward compatibility during transition
    
    Constraints:
    - Must work with existing .NET Core backend
    - Can't break existing mobile app that uses REST
    - Need to complete migration within 2 months
    - Team has limited GraphQL experience
    """
    
    print("🚀 Starting persona-based migration process...\n")
    
    # Run the migration process
    result = agent.run_migration(user_request)
    
    if result["success"]:
        print("✅ Migration process completed successfully!\n")
        
        # Display results from each persona
        print("=" * 60)
        print("PERSONA-BASED MIGRATION RESULTS")
        print("=" * 60)
        
        for i, message in enumerate(result["messages"], 1):
            print(f"\n--- Message {i} ---")
            print(message)
            print("-" * 40)
        
        # Display structured results
        if result.get("requirements"):
            print("\n📋 REQUIREMENTS GATHERED:")
            for req in result["requirements"]:
                print(f"  • {req.description} (Priority: {req.priority})")
        
        if result.get("technical_design"):
            design = result["technical_design"]
            print(f"\n🏗️  TECHNICAL DESIGN: {design.title}")
            print(f"   Overview: {design.overview}")
            print("   Implementation Steps:")
            for step in design.implementation_steps:
                print(f"     - {step}")
        
        if result.get("implementation"):
            impl = result["implementation"]
            print(f"\n💻 IMPLEMENTATION COMPLETED:")
            print(f"   Files Created: {', '.join(impl.files_created)}")
            print(f"   Files Modified: {', '.join(impl.files_modified)}")
            print(f"   Tests Written: {'✅' if impl.tests_written else '❌'}")
            print(f"   Documentation Updated: {'✅' if impl.documentation_updated else '❌'}")
        
    else:
        print(f"❌ Migration process failed: {result.get('error')}")

def example_with_problem_solving():
    """Example: Handle migration with problems that need solving."""
    
    agent = create_persona_agent(use_online_models=True)
    
    user_request = """
    I'm trying to migrate our legacy PHP application to a modern Node.js stack,
    but I'm running into several issues:
    
    1. Database schema incompatibilities
    2. Authentication system differences  
    3. File upload handling changes
    4. Session management problems
    
    I need help designing the migration approach and solving these specific problems.
    """
    
    print("🔧 Starting migration with problem-solving focus...\n")
    
    result = agent.run_migration(user_request)
    
    if result["success"]:
        print("✅ Migration with problem solving completed!\n")
        
        for i, message in enumerate(result["messages"], 1):
            print(f"\n--- Response {i} ---")
            print(message)
            print("-" * 40)
    else:
        print(f"❌ Process failed: {result.get('error')}")

def main():
    """Run example scenarios."""
    print("PERSONA-BASED MIGRATION AGENT EXAMPLES")
    print("=" * 50)
    
    # Example 1: Standard GraphQL migration
    print("\n🔹 Example 1: REST to GraphQL Migration")
    example_graphql_migration()
    
    print("\n" + "=" * 50)
    
    # Example 2: Migration with problem solving
    print("\n🔹 Example 2: Legacy Migration with Problem Solving")
    example_with_problem_solving()

if __name__ == "__main__":
    main()