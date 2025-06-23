#!/usr/bin/env python3
"""Test script to verify persona agent setup works correctly."""

import os
from persona_migration_agent import create_persona_agent

def test_agent_creation():
    """Test that agent can be created in both online and offline modes."""
    
    print("🧪 Testing Persona Migration Agent Setup")
    print("=" * 50)
    
    # Test 1: Local models (should always work)
    print("\n1️⃣ Testing local models...")
    try:
        local_agent = create_persona_agent(use_online_models=False)
        print("✅ Local agent created successfully")
        print(f"   - Graph available: {hasattr(local_agent, 'graph')}")
        print(f"   - Has 4 personas: {all(hasattr(local_agent, attr) for attr in ['product_manager_llm', 'architect_llm', 'implementer_llm', 'problem_solver_llm'])}")
    except Exception as e:
        print(f"❌ Local agent failed: {e}")
    
    # Test 2: Online models (depends on API keys)
    print("\n2️⃣ Testing online models...")
    openai_key = os.getenv("OPENAI_API_KEY")
    anthropic_key = os.getenv("ANTHROPIC_API_KEY")
    
    print(f"   - OPENAI_API_KEY: {'✅ Set' if openai_key else '❌ Not set'}")
    print(f"   - ANTHROPIC_API_KEY: {'✅ Set' if anthropic_key else '❌ Not set'}")
    
    try:
        online_agent = create_persona_agent(use_online_models=True)
        print("✅ Online agent created successfully")
    except Exception as e:
        print(f"⚠️  Online agent creation issue (will fallback to local): {e}")
    
    # Test 3: Graph export (for LangGraph Studio)
    print("\n3️⃣ Testing graph export...")
    try:
        from persona_migration_agent import graph
        print("✅ Graph exported successfully for LangGraph Studio")
        print(f"   - Graph type: {type(graph)}")
    except Exception as e:
        print(f"❌ Graph export failed: {e}")
    
    # Test 4: Environment recommendations
    print("\n4️⃣ Environment recommendations:")
    if not openai_key:
        print("   📝 Add OPENAI_API_KEY to use GPT-4o-mini and o1-mini models")
    if not anthropic_key:
        print("   📝 Add ANTHROPIC_API_KEY to use Claude 3.5 Sonnet for problem solving")
    
    langsmith_key = os.getenv("LANGCHAIN_API_KEY")
    if not langsmith_key:
        print("   📝 Add LANGCHAIN_API_KEY for LangSmith tracing (recommended for LangGraph Studio)")
    
    print("\n🎯 Setup Status:")
    if openai_key and anthropic_key and langsmith_key:
        print("   ✅ Fully configured for online models + tracing")
    elif openai_key:
        print("   ⚠️  Partially configured (OpenAI only)")
    else:
        print("   🏠 Local-only mode (requires LM Studio)")
    
    print("\n" + "=" * 50)
    print("🚀 Ready for LangGraph Studio!")

if __name__ == "__main__":
    test_agent_creation()