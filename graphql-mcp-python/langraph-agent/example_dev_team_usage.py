"""Example usage of the generic software development team agent."""

from software_dev_team_agent import create_dev_team_agent

def example_new_feature():
    """Example: Develop a new feature using the software development team."""
    
    # Create the development team agent
    team = create_dev_team_agent(use_online_models=True)
    
    # Define a new feature request
    project_request = """
    We need to add a user authentication system to our web application.
    
    Requirements:
    - Users should be able to register with email and password
    - Users should be able to log in and log out
    - Implement password reset functionality
    - Add role-based access control (admin, user, guest)
    - Include session management and remember me functionality
    - Ensure security best practices (password hashing, rate limiting)
    - Mobile-responsive login/register forms
    
    Current Stack:
    - Frontend: React with TypeScript
    - Backend: Node.js with Express
    - Database: PostgreSQL
    - Already using: JWT for tokens, bcrypt for hashing
    
    Timeline: 2-3 weeks
    Team size: 1 full-stack developer
    """
    
    print("🚀 Starting new feature development with the software team...\n")
    
    # Run the development process
    result = team.develop_project(project_request, project_type="feature")
    
    if result["success"]:
        print("✅ Feature development completed successfully!\n")
        
        # Display team collaboration results
        print("=" * 70)
        print("SOFTWARE DEVELOPMENT TEAM RESULTS")
        print("=" * 70)
        
        for i, message in enumerate(result["messages"], 1):
            print(f"\n--- Team Member Response {i} ---")
            print(message)
            print("-" * 50)
        
        # Display structured results
        if result.get("user_stories"):
            print("\n📋 USER STORIES FROM PRODUCT MANAGER:")
            for story in result["user_stories"]:
                print(f"  • {story.title} (Priority: {story.priority}, Points: {story.story_points})")
                print(f"    {story.description}")
        
        if result.get("technical_spec"):
            spec = result["technical_spec"]
            print(f"\n🏗️  TECHNICAL SPEC FROM ARCHITECT:")
            print(f"   Title: {spec.title}")
            print(f"   Testing Strategy: {spec.testing_strategy}")
            print("   Implementation Plan:")
            for step in spec.implementation_plan:
                print(f"     - {step}")
        
        if result.get("code_deliverables"):
            deliverables = result["code_deliverables"]
            print(f"\n💻 CODE DELIVERABLES FROM ENGINEER:")
            print(f"   Files Created: {', '.join(deliverables.files_created)}")
            print(f"   Files Modified: {', '.join(deliverables.files_modified)}")
            print(f"   Tests Written: {', '.join(deliverables.tests_written)}")
            print(f"   Documentation: {', '.join(deliverables.documentation_updates)}")
        
        print(f"\n🔧 ISSUES RESOLVED: {result.get('issues_resolved', 0)}")
        
    else:
        print(f"❌ Feature development failed: {result.get('error')}")

def example_bug_fix():
    """Example: Fix a bug using the software development team."""
    
    team = create_dev_team_agent(use_online_models=True)
    
    project_request = """
    URGENT: Payment processing is failing for credit card transactions.
    
    Problem Description:
    - Users report that credit card payments are being declined even with valid cards
    - Error appears to happen during the Stripe API integration
    - Issue started appearing after yesterday's deployment
    - Affects approximately 30% of transactions
    - Error logs show "Invalid card number" but cards are valid
    
    Error Details:
    - Error in payment.js line 45: "Card validation failed"
    - Stripe webhook returning status 400
    - Database shows incomplete payment records
    
    Impact:
    - Revenue loss: ~$5,000/hour
    - Customer complaints increasing
    - Support team overwhelmed
    
    Constraints:
    - Must be fixed within 4 hours
    - Cannot take payment system offline
    - Need to maintain PCI compliance
    - Must verify fix doesn't break other payment methods (PayPal, bank transfer)
    """
    
    print("🚨 Starting urgent bug fix with the software team...\n")
    
    result = team.develop_project(project_request, project_type="bugfix")
    
    if result["success"]:
        print("✅ Bug fix completed successfully!\n")
        
        for i, message in enumerate(result["messages"], 1):
            print(f"\n--- Team Response {i} ---")
            print(message)
            print("-" * 40)
    else:
        print(f"❌ Bug fix failed: {result.get('error')}")

def example_refactoring():
    """Example: Refactor legacy code using the software development team."""
    
    team = create_dev_team_agent(use_online_models=True)
    
    project_request = """
    Refactor the legacy user management module to improve maintainability and performance.
    
    Current Issues:
    - Code is 3+ years old with minimal documentation
    - Uses deprecated libraries and patterns
    - Performance is degrading with growing user base (50,000+ users)
    - Hard to add new features due to tight coupling
    - Test coverage is only 30%
    - Security vulnerabilities in old authentication code
    
    Goals:
    - Modernize to current framework versions
    - Improve performance by 50%
    - Increase test coverage to 90%+
    - Add proper documentation
    - Implement clean architecture patterns
    - Maintain 100% backward compatibility during transition
    
    Scope:
    - User registration and login flows
    - Profile management
    - Password reset functionality
    - User roles and permissions
    - Session management
    
    Technology Updates:
    - Update from Express 3.x to 4.x
    - Replace custom ORM with Prisma
    - Update from React 16 to 18
    - Implement TypeScript throughout
    - Add proper error handling and logging
    
    Timeline: 6 weeks
    Risk: Cannot break existing functionality
    """
    
    print("🔧 Starting code refactoring with the software team...\n")
    
    result = team.develop_project(project_request, project_type="refactor")
    
    if result["success"]:
        print("✅ Refactoring completed successfully!\n")
        
        for i, message in enumerate(result["messages"], 1):
            print(f"\n--- Team Response {i} ---")
            print(message)
            print("-" * 40)
    else:
        print(f"❌ Refactoring failed: {result.get('error')}")

def example_new_project():
    """Example: Start a new project using the software development team."""
    
    team = create_dev_team_agent(use_online_models=True)
    
    project_request = """
    Build a new microservice for real-time notifications.
    
    Business Requirements:
    - Handle 10,000+ concurrent WebSocket connections
    - Support push notifications (iOS, Android, Web)
    - Email and SMS fallback options
    - Real-time delivery status tracking
    - Multi-tenant architecture for different clients
    - Admin dashboard for monitoring and analytics
    
    Technical Requirements:
    - High availability (99.9% uptime)
    - Horizontal scalability
    - Sub-second delivery times
    - Rate limiting and throttling
    - Message queuing for reliability
    - Comprehensive monitoring and alerting
    
    Integration Needs:
    - REST API for sending notifications
    - GraphQL subscription support
    - Integration with existing user service
    - Support for template-based messages
    - A/B testing capabilities for notification content
    
    Technology Preferences:
    - Backend: Node.js or Go
    - Database: Redis for caching, PostgreSQL for persistence
    - Message Queue: Redis or RabbitMQ
    - WebSocket: Socket.io or native WebSocket
    - Push Services: Firebase FCM, Apple APNS
    - Monitoring: Prometheus + Grafana
    
    Timeline: 8 weeks
    Team: 2 backend developers, 1 DevOps engineer
    Budget: $50,000
    """
    
    print("🆕 Starting new project with the software team...\n")
    
    result = team.develop_project(project_request, project_type="new_project")
    
    if result["success"]:
        print("✅ New project planning completed successfully!\n")
        
        for i, message in enumerate(result["messages"], 1):
            print(f"\n--- Team Response {i} ---")
            print(message)
            print("-" * 40)
    else:
        print(f"❌ Project planning failed: {result.get('error')}")

def main():
    """Run example scenarios for the software development team."""
    print("SOFTWARE DEVELOPMENT TEAM AGENT EXAMPLES")
    print("=" * 60)
    
    # Example 1: New Feature Development
    print("\n🔹 Example 1: New Feature - User Authentication System")
    example_new_feature()
    
    print("\n" + "=" * 60)
    
    # Example 2: Bug Fix
    print("\n🔹 Example 2: Bug Fix - Payment Processing Issue")
    example_bug_fix()
    
    print("\n" + "=" * 60)
    
    # Example 3: Code Refactoring
    print("\n🔹 Example 3: Refactoring - Legacy User Management")
    example_refactoring()
    
    print("\n" + "=" * 60)
    
    # Example 4: New Project
    print("\n🔹 Example 4: New Project - Notification Microservice")
    example_new_project()

if __name__ == "__main__":
    main()