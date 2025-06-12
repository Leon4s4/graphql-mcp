using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Prompts;

[McpServerPromptType]
public static class GraphQlWorkflowPrompts
{
    [McpServerPrompt, Description("Guide for implementing GraphQL best practices in development workflows")]
    public static string DevelopmentWorkflow(
        [Description("Development phase (planning, implementation, testing, deployment)")] string phase,
        [Description("Team size (solo, small, medium, large)")] string teamSize = "small",
        [Description("Technology stack (node, dotnet, python, java)")] string techStack = "dotnet")
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("# GraphQL Development Workflow Guide");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL development process expert. Provide comprehensive guidance for implementing GraphQL best practices in development workflows.");
        prompt.AppendLine();
        prompt.AppendLine($"**Development Phase:** {phase}");
        prompt.AppendLine($"**Team Size:** {teamSize}");
        prompt.AppendLine($"**Technology Stack:** {techStack}");
        prompt.AppendLine();
        
        switch (phase.ToLower())
        {
            case "planning":
                prompt.AppendLine("## Planning Phase Guidelines:");
                prompt.AppendLine("### Requirements Analysis:");
                prompt.AppendLine("- Map business requirements to GraphQL operations");
                prompt.AppendLine("- Identify data relationships and access patterns");
                prompt.AppendLine("- Plan for client needs and use cases");
                prompt.AppendLine("- Consider performance and scalability requirements");
                prompt.AppendLine();
                prompt.AppendLine("### Schema Design Planning:");
                prompt.AppendLine("- Design type hierarchy and relationships");
                prompt.AppendLine("- Plan for evolution and backwards compatibility");
                prompt.AppendLine("- Define naming conventions and standards");
                prompt.AppendLine("- Consider security and authorization patterns");
                break;
                
            case "implementation":
                prompt.AppendLine("## Implementation Phase Guidelines:");
                prompt.AppendLine("### Development Best Practices:");
                prompt.AppendLine("- Implement schema-first development approach");
                prompt.AppendLine("- Use code generation tools for type safety");
                prompt.AppendLine("- Implement proper error handling and logging");
                prompt.AppendLine("- Follow resolver implementation patterns");
                prompt.AppendLine();
                prompt.AppendLine("### Quality Assurance:");
                prompt.AppendLine("- Write tests for resolvers and schema validation");
                prompt.AppendLine("- Implement static analysis and linting");
                prompt.AppendLine("- Use GraphQL-specific development tools");
                prompt.AppendLine("- Document schema and resolver logic");
                break;
                
            case "testing":
                prompt.AppendLine("## Testing Phase Guidelines:");
                prompt.AppendLine("### Testing Strategy:");
                prompt.AppendLine("- Unit testing for individual resolvers");
                prompt.AppendLine("- Integration testing for complete query flows");
                prompt.AppendLine("- Performance testing for query complexity");
                prompt.AppendLine("- Security testing for authorization and DoS protection");
                prompt.AppendLine();
                prompt.AppendLine("### Test Automation:");
                prompt.AppendLine("- Automated schema validation and regression testing");
                prompt.AppendLine("- Continuous integration for GraphQL APIs");
                prompt.AppendLine("- Mock data generation for testing scenarios");
                prompt.AppendLine("- Client compatibility testing");
                break;
                
            case "deployment":
                prompt.AppendLine("## Deployment Phase Guidelines:");
                prompt.AppendLine("### Deployment Strategy:");
                prompt.AppendLine("- Blue-green deployment for schema changes");
                prompt.AppendLine("- Feature flags for gradual rollout");
                prompt.AppendLine("- Monitoring and alerting setup");
                prompt.AppendLine("- Rollback procedures for issues");
                prompt.AppendLine();
                prompt.AppendLine("### Production Considerations:");
                prompt.AppendLine("- Performance monitoring and optimization");
                prompt.AppendLine("- Error tracking and debugging tools");
                prompt.AppendLine("- Security hardening and rate limiting");
                prompt.AppendLine("- Documentation and API versioning");
                break;
        }
        
        // Team size specific considerations
        if (teamSize == "large")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Large Team Considerations:");
            prompt.AppendLine("- Implement schema federation for team autonomy");
            prompt.AppendLine("- Establish code review processes for schema changes");
            prompt.AppendLine("- Use shared tooling and development standards");
            prompt.AppendLine("- Implement proper change management processes");
        }
        
        // Technology stack specific guidance
        prompt.AppendLine();
        prompt.AppendLine($"## {techStack.ToUpper()} Specific Recommendations:");
        
        switch (techStack.ToLower())
        {
            case "node":
                prompt.AppendLine("- Use Apollo Server or GraphQL Yoga for server implementation");
                prompt.AppendLine("- Implement DataLoader for N+1 query prevention");
                prompt.AppendLine("- Use TypeScript for type safety");
                prompt.AppendLine("- Consider GraphQL Code Generator for client types");
                break;
                
            case "dotnet":
                prompt.AppendLine("- Use Hot Chocolate or GraphQL.NET for server implementation");
                prompt.AppendLine("- Leverage Entity Framework with projection for data access");
                prompt.AppendLine("- Implement proper dependency injection patterns");
                prompt.AppendLine("- Use StrawberryShake for strongly-typed clients");
                break;
                
            case "python":
                prompt.AppendLine("- Use Strawberry, Graphene, or Ariadne for server implementation");
                prompt.AppendLine("- Implement async resolvers for performance");
                prompt.AppendLine("- Use dataclasses or Pydantic for type definitions");
                prompt.AppendLine("- Consider Tortoise ORM or SQLAlchemy for data access");
                break;
                
            case "java":
                prompt.AppendLine("- Use GraphQL Java or Spring GraphQL for server implementation");
                prompt.AppendLine("- Implement DataFetcher interfaces properly");
                prompt.AppendLine("- Use JPA/Hibernate with proper query optimization");
                prompt.AppendLine("- Consider GraphQL Java Tools for schema-first development");
                break;
        }
        
        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Process Overview**: Step-by-step workflow for the specified phase");
        prompt.AppendLine("2. **Tools and Techniques**: Recommended tools and implementation approaches");
        prompt.AppendLine("3. **Quality Gates**: Checkpoints and validation criteria");
        prompt.AppendLine("4. **Common Pitfalls**: Issues to avoid and how to prevent them");
        prompt.AppendLine("5. **Success Metrics**: How to measure progress and quality");
        prompt.AppendLine("6. **Next Steps**: How to transition to the next development phase");
        
        return prompt.ToString();
    }

    [McpServerPrompt, Description("Generate GraphQL API documentation and guides for developers")]
    public static string DocumentationGuide(
        [Description("Documentation type (api-reference, getting-started, best-practices, migration)")] string docType,
        [Description("Target audience (developers, qa, devops, business)")] string audience = "developers",
        [Description("Documentation format (markdown, interactive, video-script)")] string format = "markdown")
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("# GraphQL Documentation Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a technical writing expert specializing in GraphQL API documentation. Create comprehensive, user-friendly documentation that helps teams understand and use GraphQL APIs effectively.");
        prompt.AppendLine();
        prompt.AppendLine($"**Documentation Type:** {docType}");
        prompt.AppendLine($"**Target Audience:** {audience}");
        prompt.AppendLine($"**Format:** {format}");
        prompt.AppendLine();
        
        switch (docType.ToLower())
        {
            case "api-reference":
                prompt.AppendLine("## API Reference Documentation Structure:");
                prompt.AppendLine("### Essential Sections:");
                prompt.AppendLine("- **Schema Overview**: High-level schema structure and relationships");
                prompt.AppendLine("- **Type Definitions**: Detailed documentation for all types");
                prompt.AppendLine("- **Query Examples**: Practical examples for each query operation");
                prompt.AppendLine("- **Mutation Guides**: Step-by-step mutation usage");
                prompt.AppendLine("- **Error Handling**: Common errors and resolution strategies");
                prompt.AppendLine();
                prompt.AppendLine("### Documentation Standards:");
                prompt.AppendLine("- Include clear field descriptions and parameter explanations");
                prompt.AppendLine("- Provide working code examples in multiple languages");
                prompt.AppendLine("- Show both successful and error response examples");
                prompt.AppendLine("- Include authentication and authorization requirements");
                break;
                
            case "getting-started":
                prompt.AppendLine("## Getting Started Guide Structure:");
                prompt.AppendLine("### Onboarding Flow:");
                prompt.AppendLine("- **Quick Start**: 5-minute setup and first query");
                prompt.AppendLine("- **Authentication Setup**: How to obtain and use API credentials");
                prompt.AppendLine("- **Basic Concepts**: GraphQL fundamentals specific to this API");
                prompt.AppendLine("- **Common Use Cases**: Most frequent integration patterns");
                prompt.AppendLine("- **Next Steps**: Advanced features and optimization");
                prompt.AppendLine();
                prompt.AppendLine("### Learning Resources:");
                prompt.AppendLine("- Interactive GraphQL playground with pre-loaded examples");
                prompt.AppendLine("- Video tutorials for visual learners");
                prompt.AppendLine("- Sample applications and starter projects");
                prompt.AppendLine("- FAQ addressing common questions and issues");
                break;
                
            case "best-practices":
                prompt.AppendLine("## Best Practices Guide Structure:");
                prompt.AppendLine("### Development Guidelines:");
                prompt.AppendLine("- **Query Optimization**: How to write efficient queries");
                prompt.AppendLine("- **Error Handling**: Proper error management strategies");
                prompt.AppendLine("- **Caching Strategies**: Client and server-side caching");
                prompt.AppendLine("- **Security Practices**: Authentication, authorization, and DoS prevention");
                prompt.AppendLine("- **Performance Monitoring**: Tracking and optimizing API usage");
                prompt.AppendLine();
                prompt.AppendLine("### Integration Patterns:");
                prompt.AppendLine("- Client library recommendations and usage");
                prompt.AppendLine("- Batching and request optimization techniques");
                prompt.AppendLine("- Real-time subscriptions implementation");
                prompt.AppendLine("- Testing strategies and tools");
                break;
                
            case "migration":
                prompt.AppendLine("## Migration Guide Structure:");
                prompt.AppendLine("### Migration Planning:");
                prompt.AppendLine("- **Breaking Changes**: Detailed list of incompatible changes");
                prompt.AppendLine("- **Migration Timeline**: Deprecation schedule and deadlines");
                prompt.AppendLine("- **Compatibility Matrix**: Version compatibility information");
                prompt.AppendLine("- **Risk Assessment**: Potential issues and mitigation strategies");
                prompt.AppendLine();
                prompt.AppendLine("### Implementation Steps:");
                prompt.AppendLine("- Step-by-step migration process");
                prompt.AppendLine("- Code transformation examples");
                prompt.AppendLine("- Testing and validation procedures");
                prompt.AppendLine("- Rollback procedures if needed");
                break;
        }
        
        // Audience-specific considerations
        prompt.AppendLine();
        prompt.AppendLine($"## {audience.ToUpper()} Audience Considerations:");
        
        switch (audience.ToLower())
        {
            case "developers":
                prompt.AppendLine("- Focus on technical implementation details");
                prompt.AppendLine("- Include code examples and integration patterns");
                prompt.AppendLine("- Provide troubleshooting and debugging guidance");
                prompt.AppendLine("- Reference related tools and libraries");
                break;
                
            case "qa":
                prompt.AppendLine("- Emphasize testing strategies and validation");
                prompt.AppendLine("- Include test case examples and automation");
                prompt.AppendLine("- Focus on error scenarios and edge cases");
                prompt.AppendLine("- Provide performance testing guidelines");
                break;
                
            case "devops":
                prompt.AppendLine("- Focus on deployment and operational concerns");
                prompt.AppendLine("- Include monitoring and alerting setup");
                prompt.AppendLine("- Provide security and performance configuration");
                prompt.AppendLine("- Cover scaling and infrastructure considerations");
                break;
                
            case "business":
                prompt.AppendLine("- Use non-technical language and focus on benefits");
                prompt.AppendLine("- Include business impact and ROI information");
                prompt.AppendLine("- Provide timeline and resource requirements");
                prompt.AppendLine("- Focus on risk mitigation and success metrics");
                break;
        }
        
        // Format-specific guidance
        if (format == "interactive")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Interactive Documentation Features:");
            prompt.AppendLine("- Embedded GraphQL playground with live examples");
            prompt.AppendLine("- Interactive schema explorer with search");
            prompt.AppendLine("- Code generation tools for different languages");
            prompt.AppendLine("- Real-time validation and error feedback");
        }
        
        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Document Structure**: Complete outline with sections and subsections");
        prompt.AppendLine("2. **Content Guidelines**: What to include in each section");
        prompt.AppendLine("3. **Example Content**: Sample sections with actual content");
        prompt.AppendLine("4. **Visual Elements**: Diagrams, screenshots, and interactive components");
        prompt.AppendLine("5. **Maintenance Plan**: How to keep documentation current and accurate");
        prompt.AppendLine("6. **Success Metrics**: How to measure documentation effectiveness");
        
        return prompt.ToString();
    }

    [McpServerPrompt, Description("Create GraphQL learning and training materials for teams")]
    public static string TrainingMaterial(
        [Description("Training level (beginner, intermediate, advanced, expert)")] string level,
        [Description("Training format (workshop, course, bootcamp, self-paced)")] string format = "workshop",
        [Description("Focus area (concepts, implementation, optimization, architecture)")] string focus = "concepts",
        [Description("Duration in hours")] int duration = 8)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("# GraphQL Training Material Creator");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL education expert. Create comprehensive training materials that effectively teach GraphQL concepts, implementation, and best practices to development teams.");
        prompt.AppendLine();
        prompt.AppendLine($"**Training Level:** {level}");
        prompt.AppendLine($"**Format:** {format}");
        prompt.AppendLine($"**Focus Area:** {focus}");
        prompt.AppendLine($"**Duration:** {duration} hours");
        prompt.AppendLine();
        
        // Level-specific learning objectives
        switch (level.ToLower())
        {
            case "beginner":
                prompt.AppendLine("## Beginner Learning Objectives:");
                prompt.AppendLine("- Understand GraphQL fundamentals and advantages over REST");
                prompt.AppendLine("- Learn basic query and mutation syntax");
                prompt.AppendLine("- Understand schema definition and type system");
                prompt.AppendLine("- Perform basic operations using GraphQL playground");
                prompt.AppendLine("- Recognize common GraphQL patterns and anti-patterns");
                break;
                
            case "intermediate":
                prompt.AppendLine("## Intermediate Learning Objectives:");
                prompt.AppendLine("- Implement GraphQL servers with popular frameworks");
                prompt.AppendLine("- Design effective schema architecture");
                prompt.AppendLine("- Implement authentication and authorization");
                prompt.AppendLine("- Handle complex data relationships and N+1 problems");
                prompt.AppendLine("- Integrate GraphQL with existing systems and databases");
                break;
                
            case "advanced":
                prompt.AppendLine("## Advanced Learning Objectives:");
                prompt.AppendLine("- Design scalable GraphQL architectures");
                prompt.AppendLine("- Implement schema federation and microservices");
                prompt.AppendLine("- Optimize performance and implement caching strategies");
                prompt.AppendLine("- Handle real-time subscriptions and event-driven updates");
                prompt.AppendLine("- Implement comprehensive testing and monitoring");
                break;
                
            case "expert":
                prompt.AppendLine("## Expert Learning Objectives:");
                prompt.AppendLine("- Architect enterprise-scale GraphQL systems");
                prompt.AppendLine("- Lead GraphQL adoption and migration strategies");
                prompt.AppendLine("- Implement custom directives and schema transformations");
                prompt.AppendLine("- Design GraphQL governance and best practices");
                prompt.AppendLine("- Contribute to GraphQL ecosystem and tooling");
                break;
        }
        
        // Focus area specific content
        prompt.AppendLine();
        prompt.AppendLine($"## {focus.ToUpper()} Focus Area Content:");
        
        switch (focus.ToLower())
        {
            case "concepts":
                prompt.AppendLine("### Core Concepts Module:");
                prompt.AppendLine("- GraphQL vs REST comparison and when to use each");
                prompt.AppendLine("- Type system and schema definition language");
                prompt.AppendLine("- Queries, mutations, and subscriptions");
                prompt.AppendLine("- Resolvers and execution model");
                prompt.AppendLine("- Introspection and tooling ecosystem");
                break;
                
            case "implementation":
                prompt.AppendLine("### Implementation Module:");
                prompt.AppendLine("- Server setup and framework selection");
                prompt.AppendLine("- Schema design and resolver implementation");
                prompt.AppendLine("- Data layer integration and ORM patterns");
                prompt.AppendLine("- Error handling and validation");
                prompt.AppendLine("- Client integration and code generation");
                break;
                
            case "optimization":
                prompt.AppendLine("### Optimization Module:");
                prompt.AppendLine("- Query complexity analysis and limiting");
                prompt.AppendLine("- DataLoader pattern and batching");
                prompt.AppendLine("- Caching strategies (server and client)");
                prompt.AppendLine("- Performance monitoring and profiling");
                prompt.AppendLine("- Security hardening and DoS prevention");
                break;
                
            case "architecture":
                prompt.AppendLine("### Architecture Module:");
                prompt.AppendLine("- Schema federation and microservices patterns");
                prompt.AppendLine("- API gateway integration and service composition");
                prompt.AppendLine("- Event-driven architecture with GraphQL");
                prompt.AppendLine("- Scalability patterns and deployment strategies");
                prompt.AppendLine("- Monitoring, observability, and operations");
                break;
        }
        
        // Format-specific structure
        prompt.AppendLine();
        prompt.AppendLine($"## {format.ToUpper()} Format Structure:");
        
        switch (format.ToLower())
        {
            case "workshop":
                prompt.AppendLine("### Workshop Structure (Interactive Learning):");
                prompt.AppendLine("- **Introduction (10%)**: Context setting and objectives");
                prompt.AppendLine("- **Concepts (30%)**: Theory with interactive demonstrations");
                prompt.AppendLine("- **Hands-on Labs (50%)**: Practical exercises and coding");
                prompt.AppendLine("- **Q&A and Wrap-up (10%)**: Questions and next steps");
                prompt.AppendLine();
                prompt.AppendLine("### Workshop Requirements:");
                prompt.AppendLine("- Live coding environments and sample projects");
                prompt.AppendLine("- Interactive exercises with immediate feedback");
                prompt.AppendLine("- Group activities and pair programming");
                prompt.AppendLine("- Real-world problem-solving scenarios");
                break;
                
            case "course":
                prompt.AppendLine("### Course Structure (Comprehensive Learning):");
                prompt.AppendLine("- **Module 1 (25%)**: Fundamentals and foundations");
                prompt.AppendLine("- **Module 2 (35%)**: Implementation and practice");
                prompt.AppendLine("- **Module 3 (25%)**: Advanced topics and patterns");
                prompt.AppendLine("- **Module 4 (15%)**: Capstone project and assessment");
                prompt.AppendLine();
                prompt.AppendLine("### Course Components:");
                prompt.AppendLine("- Video lectures with slides and notes");
                prompt.AppendLine("- Progressive assignments building on each other");
                prompt.AppendLine("- Quizzes and knowledge checks");
                prompt.AppendLine("- Final project demonstrating mastery");
                break;
                
            case "bootcamp":
                prompt.AppendLine("### Bootcamp Structure (Intensive Learning):");
                prompt.AppendLine("- **Foundation Day**: Core concepts and basic implementation");
                prompt.AppendLine("- **Development Days**: Building complete applications");
                prompt.AppendLine("- **Advanced Day**: Performance, security, and architecture");
                prompt.AppendLine("- **Project Day**: Team-based capstone project");
                prompt.AppendLine();
                prompt.AppendLine("### Bootcamp Features:");
                prompt.AppendLine("- Intensive hands-on coding sessions");
                prompt.AppendLine("- Mentored project development");
                prompt.AppendLine("- Peer learning and code reviews");
                prompt.AppendLine("- Industry expert guest sessions");
                break;
                
            case "self-paced":
                prompt.AppendLine("### Self-Paced Structure (Flexible Learning):");
                prompt.AppendLine("- **Learning Modules**: Bite-sized lessons with clear objectives");
                prompt.AppendLine("- **Practice Exercises**: Interactive coding challenges");
                prompt.AppendLine("- **Assessment Tools**: Self-check quizzes and projects");
                prompt.AppendLine("- **Resource Library**: Documentation, examples, and references");
                prompt.AppendLine();
                prompt.AppendLine("### Self-Paced Features:");
                prompt.AppendLine("- Adaptive learning paths based on experience");
                prompt.AppendLine("- Progress tracking and achievement badges");
                prompt.AppendLine("- Community forums and peer support");
                prompt.AppendLine("- Optional instructor check-ins");
                break;
        }
        
        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Learning Plan**: Detailed curriculum with timeline and milestones");
        prompt.AppendLine("2. **Content Outline**: Session-by-session breakdown with learning objectives");
        prompt.AppendLine("3. **Hands-on Exercises**: Practical coding exercises and projects");
        prompt.AppendLine("4. **Assessment Strategy**: How to measure learning progress and outcomes");
        prompt.AppendLine("5. **Resources and Materials**: Required tools, documentation, and references");
        prompt.AppendLine("6. **Success Metrics**: How to evaluate training effectiveness and ROI");
        
        return prompt.ToString();
    }

    [McpServerPrompt, Description("How to register endpoints and manage dynamic tools in this MCP server")]
    public static string EndpointManagementGuide(
        [Description("Level of detail (summary, detailed)")] string detailLevel = "summary")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# Endpoint Management Guide");
        prompt.AppendLine();
        prompt.AppendLine("This MCP server supports dynamic multi-endpoint configuration and automatic tool generation.");
        prompt.AppendLine("Use the following tools to manage GraphQL endpoints at runtime:");
        prompt.AppendLine();
        prompt.AppendLine("- `RegisterEndpoint` – add a GraphQL endpoint and generate tools");
        prompt.AppendLine("- `ListDynamicTools` – list all registered endpoints and generated tools");
        prompt.AppendLine("- `ExecuteDynamicOperation` – run any generated query or mutation");
        prompt.AppendLine("- `RefreshEndpointTools` – update tools after schema changes");
        prompt.AppendLine("- `UnregisterEndpoint` – remove an endpoint and its tools");

        if (detailLevel == "detailed")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Typical Workflow");
            prompt.AppendLine("1. Start the server with `dotnet run`.");
            prompt.AppendLine("2. Call `RegisterEndpoint` with endpoint URL, name, and optional headers.");
            prompt.AppendLine("3. Use `ListDynamicTools` to see newly created tools (e.g., `gh_query_viewer`).");
            prompt.AppendLine("4. Execute operations with `ExecuteDynamicOperation` or the generated tools.");
            prompt.AppendLine("5. When the schema updates, call `RefreshEndpointTools` to sync tools.");
            prompt.AppendLine("6. Use `UnregisterEndpoint` to clean up when an API is no longer needed.");
        }

        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Step-by-Step Instructions** for managing endpoints.");
        prompt.AppendLine("2. **Tool Descriptions** with example payloads.");
        prompt.AppendLine("3. **Best Practices** for multi-endpoint workflows.");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Overview of available MCP server tools and capabilities")]
    public static string ToolsOverview(
        [Description("Focus area (all, query, schema, performance, security)")] string focus = "all")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# MCP Server Tools Overview");
        prompt.AppendLine();
        prompt.AppendLine("This server exposes a wide range of GraphQL development tools.");

        if (focus == "all" || focus == "query")
        {
            prompt.AppendLine("## Query & Validation");
            prompt.AppendLine("- `ValidateQuery`, `TestQuery`, `ExplainQuery`, `OptimizeQuery`");
            prompt.AppendLine("- `QueryGraphQL` and auto-generated tools for endpoint operations");
        }

        if (focus == "all" || focus == "schema")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Schema & Introspection");
            prompt.AppendLine("- `GetSchema`, `DiffSchemas`, `IntrospectSchema`");
            prompt.AppendLine("- `SchemaEvolutionReport` and resolver documentation helpers");
        }

        if (focus == "all" || focus == "performance")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Performance & Analytics");
            prompt.AppendLine("- `MeasureQueryPerformance`, field usage analytics, query analyzers");
        }

        if (focus == "all" || focus == "security")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Security & Testing");
            prompt.AppendLine("- `SecurityAnalysis`, testing helpers, mock data generation");
        }

        prompt.AppendLine();
        prompt.AppendLine("Endpoint management tools are always available: `RegisterEndpoint`, `ListDynamicTools`, `ExecuteDynamicOperation`, `RefreshEndpointTools`, and `UnregisterEndpoint`.");

        prompt.AppendLine();
        prompt.AppendLine("Use this overview to discover which tool best fits your current task.");

        return prompt.ToString();
    }
}
