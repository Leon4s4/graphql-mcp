using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Prompts;

[McpServerPromptType]
public class GraphQlPrompts
{
    [McpServerPrompt, Description("Generate optimized GraphQL queries based on requirements and schema analysis")]
    public static string GenerateQuery(
        [Description("What data you want to retrieve")]
        string dataRequirement,
        [Description("GraphQL endpoint name (optional)")]
        string? endpointName = null,
        [Description("Performance requirements (fast, normal, comprehensive)")]
        string performance = "normal",
        [Description("Include related data (yes/no)")]
        string includeRelated = "yes")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Query Generation Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are an expert GraphQL developer. Generate an optimized GraphQL query based on these requirements:");
        prompt.AppendLine();
        prompt.AppendLine($"**Data Requirement:** {dataRequirement}");

        if (!string.IsNullOrEmpty(endpointName))
        {
            prompt.AppendLine($"**Target Endpoint:** {endpointName}");
        }

        prompt.AppendLine($"**Performance Level:** {performance}");
        prompt.AppendLine($"**Include Related Data:** {includeRelated}");
        prompt.AppendLine();

        prompt.AppendLine("## Guidelines:");
        prompt.AppendLine("- Write efficient, well-structured GraphQL queries");
        prompt.AppendLine("- Use fragments for reusable field sets");
        prompt.AppendLine("- Consider query complexity and depth");
        prompt.AppendLine("- Include proper variable definitions");
        prompt.AppendLine("- Add helpful comments explaining the query structure");

        if (performance == "fast")
        {
            prompt.AppendLine("- Prioritize minimal field selection for fast response");
            prompt.AppendLine("- Avoid deep nesting where possible");
        }
        else if (performance == "comprehensive")
        {
            prompt.AppendLine("- Include comprehensive field selection");
            prompt.AppendLine("- Consider pagination for large datasets");
        }

        if (includeRelated == "yes")
        {
            prompt.AppendLine("- Include related/connected data where relevant");
            prompt.AppendLine("- Use proper relationship traversal");
        }

        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. Provide the complete GraphQL query");
        prompt.AppendLine("2. Explain the query structure and choices");
        prompt.AppendLine("3. Suggest any variables that should be parameterized");
        prompt.AppendLine("4. Include usage examples if helpful");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Analyze and explain GraphQL schemas with best practices and recommendations")]
    public static string AnalyzeSchema(
        [Description("What aspect to focus on (structure, performance, security, evolution)")]
        string focus = "structure",
        [Description("GraphQL endpoint name")] string endpointName = "",
        [Description("Specific type or field to analyze (optional)")]
        string? specificType = null)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Schema Analysis Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL schema expert. Analyze the provided GraphQL schema and provide insights based on the following criteria:");
        prompt.AppendLine();
        prompt.AppendLine($"**Analysis Focus:** {focus}");
        prompt.AppendLine($"**Endpoint:** {endpointName}");

        if (!string.IsNullOrEmpty(specificType))
        {
            prompt.AppendLine($"**Specific Focus:** {specificType}");
        }

        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "structure":
                prompt.AppendLine("## Structure Analysis Focus:");
                prompt.AppendLine("- Overall schema organization and design patterns");
                prompt.AppendLine("- Type relationships and hierarchy");
                prompt.AppendLine("- Interface and union usage");
                prompt.AppendLine("- Field naming conventions and consistency");
                prompt.AppendLine("- Input type design and reusability");
                break;

            case "performance":
                prompt.AppendLine("## Performance Analysis Focus:");
                prompt.AppendLine("- Potential N+1 query problems");
                prompt.AppendLine("- Complex nested relationships");
                prompt.AppendLine("- Missing pagination on list fields");
                prompt.AppendLine("- Query complexity and depth concerns");
                prompt.AppendLine("- DataLoader optimization opportunities");
                break;

            case "security":
                prompt.AppendLine("## Security Analysis Focus:");
                prompt.AppendLine("- Query complexity and DoS vulnerability");
                prompt.AppendLine("- Sensitive data exposure in schema");
                prompt.AppendLine("- Authorization patterns and field-level security");
                prompt.AppendLine("- Introspection and information disclosure");
                prompt.AppendLine("- Input validation requirements");
                break;

            case "evolution":
                prompt.AppendLine("## Evolution Analysis Focus:");
                prompt.AppendLine("- Schema versioning and backward compatibility");
                prompt.AppendLine("- Deprecated fields and migration paths");
                prompt.AppendLine("- Breaking vs non-breaking changes");
                prompt.AppendLine("- Extension points and future flexibility");
                prompt.AppendLine("- Client impact assessment");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Summary**: High-level findings and overall assessment");
        prompt.AppendLine("2. **Key Issues**: Specific problems or concerns identified");
        prompt.AppendLine("3. **Recommendations**: Actionable improvements and best practices");
        prompt.AppendLine("4. **Implementation Notes**: How to implement suggested changes");
        prompt.AppendLine("5. **Priority Ranking**: Order recommendations by importance/impact");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Debug GraphQL errors with context-aware troubleshooting guidance")]
    public static string DebuggingAssistant(
        [Description("Type of issue (query-error, performance, schema, resolver)")]
        string issueType,
        [Description("Severity level (critical, high, medium, low)")]
        string severity = "medium",
        [Description("Environment context (development, staging, production)")]
        string environment = "development")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Debugging Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL debugging expert. Help troubleshoot and resolve GraphQL issues with systematic analysis.");
        prompt.AppendLine();
        prompt.AppendLine($"**Issue Type:** {issueType}");
        prompt.AppendLine($"**Severity:** {severity}");
        prompt.AppendLine($"**Environment:** {environment}");
        prompt.AppendLine();

        prompt.AppendLine("## Debugging Approach:");

        switch (issueType.ToLower())
        {
            case "query-error":
                prompt.AppendLine("### Query Error Analysis:");
                prompt.AppendLine("1. **Syntax Validation**: Check GraphQL query syntax and structure");
                prompt.AppendLine("2. **Schema Compliance**: Verify fields and types exist in schema");
                prompt.AppendLine("3. **Variable Validation**: Check variable types and required fields");
                prompt.AppendLine("4. **Fragments**: Validate fragment usage and type conditions");
                prompt.AppendLine("5. **Authorization**: Consider field-level permissions and auth context");
                break;

            case "performance":
                prompt.AppendLine("### Performance Analysis:");
                prompt.AppendLine("1. **Query Complexity**: Analyze depth and breadth of query");
                prompt.AppendLine("2. **N+1 Detection**: Look for resolver patterns causing multiple DB calls");
                prompt.AppendLine("3. **DataLoader Usage**: Check for proper batching implementation");
                prompt.AppendLine("4. **Cache Strategy**: Evaluate caching at resolver and response levels");
                prompt.AppendLine("5. **Database Queries**: Analyze underlying data fetching patterns");
                break;

            case "schema":
                prompt.AppendLine("### Schema Issue Analysis:");
                prompt.AppendLine("1. **Type Definitions**: Check for circular references and invalid types");
                prompt.AppendLine("2. **Resolver Mapping**: Verify all fields have corresponding resolvers");
                prompt.AppendLine("3. **Interface Implementation**: Validate interface compliance");
                prompt.AppendLine("4. **Enum Values**: Check enum definition and usage");
                prompt.AppendLine("5. **Directive Usage**: Validate custom directive implementation");
                break;

            case "resolver":
                prompt.AppendLine("### Resolver Issue Analysis:");
                prompt.AppendLine("1. **Return Types**: Verify resolver returns match schema types");
                prompt.AppendLine("2. **Error Handling**: Check exception handling and error propagation");
                prompt.AppendLine("3. **Context Usage**: Validate context object usage and data access");
                prompt.AppendLine("4. **Async Patterns**: Review promise/async implementation");
                prompt.AppendLine("5. **Data Sources**: Check database/API connectivity and queries");
                break;
        }

        if (severity == "critical" || severity == "high")
        {
            prompt.AppendLine();
            prompt.AppendLine("## High Priority Actions:");
            prompt.AppendLine("- Focus on immediate resolution and system stability");
            prompt.AppendLine("- Consider temporary workarounds to restore service");
            prompt.AppendLine("- Document incident details for post-mortem analysis");
        }

        if (environment == "production")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Production Considerations:");
            prompt.AppendLine("- Minimize changes and prefer safe, incremental fixes");
            prompt.AppendLine("- Use feature flags for any schema or resolver changes");
            prompt.AppendLine("- Monitor impact of changes with proper observability");
            prompt.AppendLine("- Have rollback plan ready before implementing fixes");
        }

        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Root Cause Analysis**: What is likely causing the issue");
        prompt.AppendLine("2. **Diagnostic Steps**: How to confirm the root cause");
        prompt.AppendLine("3. **Resolution Plan**: Step-by-step fix implementation");
        prompt.AppendLine("4. **Validation**: How to verify the fix works");
        prompt.AppendLine("5. **Prevention**: How to avoid similar issues in the future");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Generate comprehensive testing strategies for GraphQL APIs")]
    public static string TestingStrategy(
        [Description("Testing focus (unit, integration, performance, security)")]
        string testingType,
        [Description("GraphQL operation type (query, mutation, subscription)")]
        string operationType = "query",
        [Description("Test framework preference (jest, vitest, xunit, pytest)")]
        string framework = "jest")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Testing Strategy Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL testing expert. Create comprehensive testing strategies and test cases for GraphQL APIs.");
        prompt.AppendLine();
        prompt.AppendLine($"**Testing Type:** {testingType}");
        prompt.AppendLine($"**Operation Type:** {operationType}");
        prompt.AppendLine($"**Preferred Framework:** {framework}");
        prompt.AppendLine();

        switch (testingType.ToLower())
        {
            case "unit":
                prompt.AppendLine("## Unit Testing Strategy:");
                prompt.AppendLine("### Resolver Testing:");
                prompt.AppendLine("- Test individual resolvers in isolation");
                prompt.AppendLine("- Mock data sources and dependencies");
                prompt.AppendLine("- Verify correct return types and values");
                prompt.AppendLine("- Test error conditions and edge cases");
                prompt.AppendLine();
                prompt.AppendLine("### Schema Testing:");
                prompt.AppendLine("- Validate schema compilation and structure");
                prompt.AppendLine("- Test type relationships and constraints");
                prompt.AppendLine("- Verify directive implementations");
                break;

            case "integration":
                prompt.AppendLine("## Integration Testing Strategy:");
                prompt.AppendLine("### End-to-End Query Testing:");
                prompt.AppendLine("- Test complete query execution flow");
                prompt.AppendLine("- Verify data consistency across resolvers");
                prompt.AppendLine("- Test with real database connections");
                prompt.AppendLine("- Validate authentication and authorization");
                prompt.AppendLine();
                prompt.AppendLine("### API Contract Testing:");
                prompt.AppendLine("- Test schema compliance and backwards compatibility");
                prompt.AppendLine("- Verify response formats and structures");
                prompt.AppendLine("- Test with various client configurations");
                break;

            case "performance":
                prompt.AppendLine("## Performance Testing Strategy:");
                prompt.AppendLine("### Load Testing:");
                prompt.AppendLine("- Test query performance under load");
                prompt.AppendLine("- Measure response times and throughput");
                prompt.AppendLine("- Identify bottlenecks and scaling limits");
                prompt.AppendLine();
                prompt.AppendLine("### Query Complexity Testing:");
                prompt.AppendLine("- Test deeply nested queries");
                prompt.AppendLine("- Verify query complexity limits");
                prompt.AppendLine("- Test DataLoader and caching effectiveness");
                break;

            case "security":
                prompt.AppendLine("## Security Testing Strategy:");
                prompt.AppendLine("### Query Security:");
                prompt.AppendLine("- Test query complexity and DoS protection");
                prompt.AppendLine("- Verify authorization on all fields");
                prompt.AppendLine("- Test introspection security");
                prompt.AppendLine();
                prompt.AppendLine("### Input Validation:");
                prompt.AppendLine("- Test malicious input handling");
                prompt.AppendLine("- Verify sanitization and validation");
                prompt.AppendLine("- Test injection attack prevention");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Test Case Generation Guidelines:");
        prompt.AppendLine("1. **Happy Path**: Normal successful operations");
        prompt.AppendLine("2. **Edge Cases**: Boundary conditions and unusual inputs");
        prompt.AppendLine("3. **Error Cases**: Invalid inputs and failure scenarios");
        prompt.AppendLine("4. **Performance Cases**: Large datasets and complex queries");
        prompt.AppendLine("5. **Security Cases**: Malicious inputs and unauthorized access");
        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Test Plan Overview**: Strategy and scope");
        prompt.AppendLine("2. **Test Cases**: Specific test scenarios with expected outcomes");
        prompt.AppendLine("3. **Test Implementation**: Code examples using the specified framework");
        prompt.AppendLine("4. **Mocking Strategy**: How to mock dependencies and external services");
        prompt.AppendLine("5. **Assertions**: What to verify in each test type");
        prompt.AppendLine("6. **Continuous Integration**: How to integrate tests into CI/CD pipeline");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Design and optimize GraphQL schema architecture with best practices")]
    public static string SchemaDesign(
        [Description("Design focus (new-schema, refactoring, migration, optimization)")]
        string designFocus,
        [Description("Domain context (e-commerce, social, api-gateway, microservices)")]
        string domain,
        [Description("Scale requirements (small, medium, large, enterprise)")]
        string scale = "medium")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Schema Design Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL architecture expert. Help design, refactor, or optimize GraphQL schemas following industry best practices.");
        prompt.AppendLine();
        prompt.AppendLine($"**Design Focus:** {designFocus}");
        prompt.AppendLine($"**Domain Context:** {domain}");
        prompt.AppendLine($"**Scale Requirements:** {scale}");
        prompt.AppendLine();

        switch (designFocus.ToLower())
        {
            case "new-schema":
                prompt.AppendLine("## New Schema Design Principles:");
                prompt.AppendLine("### Core Design Guidelines:");
                prompt.AppendLine("- Design for client needs, not database structure");
                prompt.AppendLine("- Use meaningful, consistent naming conventions");
                prompt.AppendLine("- Plan for future evolution and backwards compatibility");
                prompt.AppendLine("- Consider query complexity and performance from the start");
                prompt.AppendLine();
                prompt.AppendLine("### Type System Design:");
                prompt.AppendLine("- Use interfaces for common patterns");
                prompt.AppendLine("- Implement proper scalar types for data validation");
                prompt.AppendLine("- Design input types for reusability");
                prompt.AppendLine("- Plan enum values for extensibility");
                break;

            case "refactoring":
                prompt.AppendLine("## Schema Refactoring Strategy:");
                prompt.AppendLine("### Safe Refactoring Practices:");
                prompt.AppendLine("- Use deprecation markers before removing fields");
                prompt.AppendLine("- Introduce new fields before deprecating old ones");
                prompt.AppendLine("- Version schema changes with proper migration paths");
                prompt.AppendLine("- Maintain backwards compatibility during transitions");
                prompt.AppendLine();
                prompt.AppendLine("### Refactoring Targets:");
                prompt.AppendLine("- Consolidate similar types and reduce duplication");
                prompt.AppendLine("- Improve type relationships and hierarchies");
                prompt.AppendLine("- Optimize resolver patterns and data fetching");
                break;

            case "migration":
                prompt.AppendLine("## Schema Migration Strategy:");
                prompt.AppendLine("### Migration Planning:");
                prompt.AppendLine("- Analyze current schema usage and client dependencies");
                prompt.AppendLine("- Plan phased migration with clear milestones");
                prompt.AppendLine("- Implement feature flags for gradual rollout");
                prompt.AppendLine("- Create comprehensive testing strategy for migration");
                prompt.AppendLine();
                prompt.AppendLine("### Client Communication:");
                prompt.AppendLine("- Document breaking changes and migration guides");
                prompt.AppendLine("- Provide migration tools and helper utilities");
                prompt.AppendLine("- Set clear timelines for deprecation and removal");
                break;

            case "optimization":
                prompt.AppendLine("## Schema Optimization Strategy:");
                prompt.AppendLine("### Performance Optimization:");
                prompt.AppendLine("- Identify and resolve N+1 query patterns");
                prompt.AppendLine("- Implement proper pagination strategies");
                prompt.AppendLine("- Design efficient resolver hierarchies");
                prompt.AppendLine("- Optimize query complexity and depth limits");
                prompt.AppendLine();
                prompt.AppendLine("### Developer Experience:");
                prompt.AppendLine("- Improve documentation and field descriptions");
                prompt.AppendLine("- Standardize error handling patterns");
                prompt.AppendLine("- Implement consistent authorization patterns");
                break;
        }

        // Add domain-specific considerations
        prompt.AppendLine();
        prompt.AppendLine($"## {domain.ToUpper()} Domain Considerations:");

        switch (domain.ToLower())
        {
            case "e-commerce":
                prompt.AppendLine("- Product catalog and inventory management");
                prompt.AppendLine("- Order processing and payment workflows");
                prompt.AppendLine("- User accounts and authentication");
                prompt.AppendLine("- Search and filtering capabilities");
                prompt.AppendLine("- Real-time inventory updates");
                break;

            case "social":
                prompt.AppendLine("- User profiles and social connections");
                prompt.AppendLine("- Content creation and sharing");
                prompt.AppendLine("- Activity feeds and notifications");
                prompt.AppendLine("- Real-time messaging and updates");
                prompt.AppendLine("- Privacy and content moderation");
                break;

            case "api-gateway":
                prompt.AppendLine("- Service composition and federation");
                prompt.AppendLine("- Authentication and authorization patterns");
                prompt.AppendLine("- Rate limiting and quota management");
                prompt.AppendLine("- Error handling and service resilience");
                prompt.AppendLine("- Monitoring and observability");
                break;

            case "microservices":
                prompt.AppendLine("- Service boundaries and data ownership");
                prompt.AppendLine("- Cross-service data fetching strategies");
                prompt.AppendLine("- Event-driven updates and consistency");
                prompt.AppendLine("- Service discovery and health checks");
                prompt.AppendLine("- Distributed tracing and monitoring");
                break;
        }

        // Add scale-specific guidance
        if (scale == "large" || scale == "enterprise")
        {
            prompt.AppendLine();
            prompt.AppendLine("## Enterprise Scale Considerations:");
            prompt.AppendLine("- Implement schema federation for team autonomy");
            prompt.AppendLine("- Design for horizontal scaling and load distribution");
            prompt.AppendLine("- Plan comprehensive monitoring and alerting");
            prompt.AppendLine("- Implement robust caching strategies");
            prompt.AppendLine("- Design for disaster recovery and high availability");
        }

        prompt.AppendLine();
        prompt.AppendLine("## Response Format:");
        prompt.AppendLine("1. **Architecture Overview**: High-level design approach and patterns");
        prompt.AppendLine("2. **Schema Structure**: Core types, relationships, and hierarchies");
        prompt.AppendLine("3. **Implementation Plan**: Step-by-step development approach");
        prompt.AppendLine("4. **Best Practices**: Specific recommendations for the domain and scale");
        prompt.AppendLine("5. **Potential Challenges**: Common pitfalls and how to avoid them");
        prompt.AppendLine("6. **Future Considerations**: How to prepare for growth and evolution");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Migrate C# REST API code to GraphQL with comprehensive analysis")]
    public static string MigrateCSharpToGraphQL(
        [Description("C# code containing REST API calls (HttpClient patterns)")]
        string csharpCode,
        [Description("Target GraphQL endpoint name")]
        string graphqlEndpoint,
        [Description("Analysis depth: basic, detailed, comprehensive")]
        string analysisLevel = "comprehensive")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# C# to GraphQL Migration Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL migration expert. Analyze the provided C# REST API code and generate equivalent GraphQL queries with comprehensive migration guidance.");
        prompt.AppendLine();
        prompt.AppendLine("## Source Code to Analyze:");
        prompt.AppendLine("```csharp");
        prompt.AppendLine(csharpCode);
        prompt.AppendLine("```");
        prompt.AppendLine();
        prompt.AppendLine($"**Target GraphQL Endpoint:** {graphqlEndpoint}");
        prompt.AppendLine($"**Analysis Level:** {analysisLevel}");
        prompt.AppendLine();

        prompt.AppendLine("## Migration Analysis Tasks:");
        prompt.AppendLine("1. **REST Call Extraction**: Identify all HTTP client calls (GET, POST, PUT, DELETE)");
        prompt.AppendLine("2. **Data Flow Analysis**: Understand how data flows between API calls");
        prompt.AppendLine("3. **Entity Relationship Mapping**: Identify related entities and their connections");
        prompt.AppendLine("4. **Aggregation Pattern Detection**: Find data combination and transformation logic");
        prompt.AppendLine("5. **GraphQL Query Generation**: Create equivalent queries that combine multiple REST calls");
        prompt.AppendLine();

        prompt.AppendLine("## Performance Benefits to Calculate:");
        prompt.AppendLine("- Network round trip reduction (from N calls to 1 query)");
        prompt.AppendLine("- Data over-fetching elimination");
        prompt.AppendLine("- Bandwidth usage optimization");
        prompt.AppendLine("- Client-side complexity reduction");
        prompt.AppendLine();

        prompt.AppendLine("## Migration Guide Requirements:");
        prompt.AppendLine("- Step-by-step migration instructions");
        prompt.AppendLine("- Before/after code comparisons");
        prompt.AppendLine("- GraphQL client implementation examples");
        prompt.AppendLine("- Error handling adaptation strategies");
        prompt.AppendLine("- Testing approach for GraphQL queries");
        prompt.AppendLine();

        prompt.AppendLine("## Use ExtractGraphQLFromCSharpCode tool with these parameters:");
        prompt.AppendLine($"- csharpCode: [the provided code]");
        prompt.AppendLine($"- graphqlEndpoint: \"{graphqlEndpoint}\"");
        prompt.AppendLine($"- analysisMode: \"{analysisLevel}\"");
        prompt.AppendLine("- includeDataFlowAnalysis: true");
        prompt.AppendLine("- includeOptimizations: true");
        prompt.AppendLine("- includeMigrationGuide: true");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Convert REST API endpoints to optimized GraphQL queries")]
    public static string ConvertRestToGraphQL(
        [Description("JSON array of REST endpoints with method, endpoint, and purpose")]
        string restEndpoints,
        [Description("Target GraphQL endpoint name")]
        string graphqlEndpoint,
        [Description("Entity relationships as JSON (optional)")]
        string? entityRelationships = null)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# REST to GraphQL Conversion Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL optimization expert. Convert the provided REST API endpoints into efficient GraphQL queries that combine multiple operations.");
        prompt.AppendLine();
        prompt.AppendLine("## REST Endpoints to Convert:");
        prompt.AppendLine(restEndpoints);
        prompt.AppendLine();
        prompt.AppendLine($"**Target GraphQL Endpoint:** {graphqlEndpoint}");

        if (!string.IsNullOrEmpty(entityRelationships))
        {
            prompt.AppendLine();
            prompt.AppendLine("**Entity Relationships:**");
            prompt.AppendLine(entityRelationships);
        }

        prompt.AppendLine();
        prompt.AppendLine("## Optimization Goals:");
        prompt.AppendLine("1. **Combine Operations**: Merge related REST calls into single GraphQL queries");
        prompt.AppendLine("2. **Field Selection**: Include only necessary fields to reduce payload");
        prompt.AppendLine("3. **Fragment Usage**: Create reusable field sets for common patterns");
        prompt.AppendLine("4. **Variable Implementation**: Use variables for dynamic query parameters");
        prompt.AppendLine("5. **Pagination Strategy**: Implement proper pagination for list queries");
        prompt.AppendLine();

        prompt.AppendLine("## Query Variations to Generate:");
        prompt.AppendLine("- **List Queries**: Paginated collections with filtering");
        prompt.AppendLine("- **Detail Queries**: Complete entity data with relationships");
        prompt.AppendLine("- **Search Queries**: Filtered results with search criteria");
        prompt.AppendLine("- **Minimal Queries**: Essential fields only for performance");
        prompt.AppendLine();

        prompt.AppendLine("## Performance Analysis Required:");
        prompt.AppendLine("- Network round trip reduction calculation");
        prompt.AppendLine("- Data over-fetching elimination metrics");
        prompt.AppendLine("- Query complexity assessment");
        prompt.AppendLine("- Caching strategy recommendations");
        prompt.AppendLine();

        prompt.AppendLine("## Use GenerateOptimizedGraphQLQueries tool with these parameters:");
        prompt.AppendLine($"- restEndpoints: {restEndpoints}");
        prompt.AppendLine($"- graphqlEndpoint: \"{graphqlEndpoint}\"");
        if (!string.IsNullOrEmpty(entityRelationships))
        {
            prompt.AppendLine($"- entityRelationships: {entityRelationships}");
        }
        prompt.AppendLine("- includeOptimizations: true");
        prompt.AppendLine("- includeVariations: true");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Plan a comprehensive REST to GraphQL migration strategy")]
    public static string PlanGraphQLMigration(
        [Description("Current REST API architecture description")]
        string currentArchitecture,
        [Description("Migration goals and business objectives")]
        string migrationGoals,
        [Description("Team size and timeline constraints")]
        string constraints,
        [Description("Risk tolerance: conservative, moderate, aggressive")]
        string riskTolerance = "moderate")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Migration Planning Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL migration strategist. Create a comprehensive, phased migration plan from REST to GraphQL that minimizes risk and maximizes business value.");
        prompt.AppendLine();
        prompt.AppendLine("## Current State Analysis:");
        prompt.AppendLine($"**REST Architecture:** {currentArchitecture}");
        prompt.AppendLine($"**Migration Goals:** {migrationGoals}");
        prompt.AppendLine($"**Constraints:** {constraints}");
        prompt.AppendLine($"**Risk Tolerance:** {riskTolerance}");
        prompt.AppendLine();

        prompt.AppendLine("## Migration Planning Framework:");
        prompt.AppendLine("### Phase 1: Assessment and Preparation");
        prompt.AppendLine("- REST API inventory and dependency mapping");
        prompt.AppendLine("- Client application analysis and impact assessment");
        prompt.AppendLine("- Team skill assessment and training needs");
        prompt.AppendLine("- Infrastructure and tooling requirements");
        prompt.AppendLine();

        prompt.AppendLine("### Phase 2: Pilot Implementation");
        prompt.AppendLine("- Select low-risk, high-value endpoints for initial migration");
        prompt.AppendLine("- Implement GraphQL schema and resolvers");
        prompt.AppendLine("- Create testing and validation procedures");
        prompt.AppendLine("- Establish monitoring and performance baselines");
        prompt.AppendLine();

        prompt.AppendLine("### Phase 3: Gradual Rollout");
        prompt.AppendLine("- Phased migration of remaining endpoints");
        prompt.AppendLine("- Client application updates and testing");
        prompt.AppendLine("- Performance monitoring and optimization");
        prompt.AppendLine("- Documentation and team knowledge transfer");
        prompt.AppendLine();

        prompt.AppendLine("### Phase 4: Optimization and Scaling");
        prompt.AppendLine("- Advanced GraphQL features implementation");
        prompt.AppendLine("- Performance tuning and caching strategies");
        prompt.AppendLine("- Legacy REST API deprecation planning");
        prompt.AppendLine("- Long-term maintenance and evolution strategy");
        prompt.AppendLine();

        switch (riskTolerance.ToLower())
        {
            case "conservative":
                prompt.AppendLine("## Conservative Migration Approach:");
                prompt.AppendLine("- Maintain parallel REST and GraphQL APIs during transition");
                prompt.AppendLine("- Extensive testing and validation at each phase");
                prompt.AppendLine("- Gradual client migration with feature flags");
                prompt.AppendLine("- Comprehensive rollback procedures");
                break;

            case "aggressive":
                prompt.AppendLine("## Aggressive Migration Approach:");
                prompt.AppendLine("- Rapid GraphQL implementation with direct replacement");
                prompt.AppendLine("- Automated testing and CI/CD pipeline integration");
                prompt.AppendLine("- Quick client migration with bulk updates");
                prompt.AppendLine("- Focus on performance gains and new capabilities");
                break;

            default: // moderate
                prompt.AppendLine("## Moderate Migration Approach:");
                prompt.AppendLine("- Balanced approach with calculated risks");
                prompt.AppendLine("- Phased implementation with validation gates");
                prompt.AppendLine("- Client migration based on criticality and complexity");
                prompt.AppendLine("- Regular performance and business impact assessment");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Success Metrics to Track:");
        prompt.AppendLine("- API response time improvements");
        prompt.AppendLine("- Network bandwidth reduction");
        prompt.AppendLine("- Developer productivity gains");
        prompt.AppendLine("- Client application performance");
        prompt.AppendLine("- Business feature delivery velocity");
        prompt.AppendLine();

        prompt.AppendLine("## Deliverables Required:");
        prompt.AppendLine("1. **Migration Roadmap**: Timeline with phases and milestones");
        prompt.AppendLine("2. **Risk Assessment**: Potential issues and mitigation strategies");
        prompt.AppendLine("3. **Resource Plan**: Team allocation and skill development");
        prompt.AppendLine("4. **Technical Architecture**: GraphQL implementation design");
        prompt.AppendLine("5. **Testing Strategy**: Validation and quality assurance approach");
        prompt.AppendLine("6. **Communication Plan**: Stakeholder updates and change management");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Create comprehensive GraphQL development workflow")]
    public static string SetupGraphQLWorkflow(
        [Description("GraphQL endpoint URL or name")]
        string endpoint,
        [Description("Development focus: exploration, development, testing, production")]
        string developmentFocus,
        [Description("Team experience level: beginner, intermediate, advanced")]
        string experienceLevel = "intermediate")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Development Workflow Setup");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL development expert. Set up a comprehensive development workflow that maximizes team productivity and code quality.");
        prompt.AppendLine();
        prompt.AppendLine($"**Target Endpoint:** {endpoint}");
        prompt.AppendLine($"**Development Focus:** {developmentFocus}");
        prompt.AppendLine($"**Team Experience:** {experienceLevel}");
        prompt.AppendLine();

        prompt.AppendLine("## Workflow Components to Establish:");

        switch (developmentFocus.ToLower())
        {
            case "exploration":
                prompt.AppendLine("### Schema Exploration Workflow:");
                prompt.AppendLine("1. **Endpoint Registration**: Set up GraphQL endpoint with proper authentication");
                prompt.AppendLine("2. **Schema Introspection**: Discover all available types, queries, and mutations");
                prompt.AppendLine("3. **Documentation Generation**: Create comprehensive API documentation");
                prompt.AppendLine("4. **Query Examples**: Generate practical query examples for key use cases");
                prompt.AppendLine("5. **Interactive Exploration**: Set up GraphQL playground or similar tools");
                break;

            case "development":
                prompt.AppendLine("### Development Workflow:");
                prompt.AppendLine("1. **Code Generation**: Generate typed client code from schema");
                prompt.AppendLine("2. **Query Building**: Establish patterns for query construction");
                prompt.AppendLine("3. **Error Handling**: Implement robust error handling strategies");
                prompt.AppendLine("4. **Performance Monitoring**: Set up query performance tracking");
                prompt.AppendLine("5. **Development Tools**: Configure IDE extensions and debugging tools");
                break;

            case "testing":
                prompt.AppendLine("### Testing Workflow:");
                prompt.AppendLine("1. **Test Suite Generation**: Create comprehensive test scenarios");
                prompt.AppendLine("2. **Mock Data Creation**: Generate realistic test data");
                prompt.AppendLine("3. **Integration Testing**: Set up end-to-end testing procedures");
                prompt.AppendLine("4. **Performance Testing**: Establish query performance benchmarks");
                prompt.AppendLine("5. **Continuous Testing**: Integrate tests into CI/CD pipeline");
                break;

            case "production":
                prompt.AppendLine("### Production Workflow:");
                prompt.AppendLine("1. **Monitoring Setup**: Implement comprehensive observability");
                prompt.AppendLine("2. **Performance Optimization**: Establish optimization procedures");
                prompt.AppendLine("3. **Security Hardening**: Implement security best practices");
                prompt.AppendLine("4. **Deployment Strategy**: Set up safe deployment procedures");
                prompt.AppendLine("5. **Incident Response**: Create troubleshooting and recovery procedures");
                break;
        }

        // Add experience-level specific guidance
        prompt.AppendLine();
        switch (experienceLevel.ToLower())
        {
            case "beginner":
                prompt.AppendLine("## Beginner-Friendly Approach:");
                prompt.AppendLine("- Start with simple queries and gradually increase complexity");
                prompt.AppendLine("- Use GraphQL playground for interactive learning");
                prompt.AppendLine("- Focus on understanding GraphQL fundamentals");
                prompt.AppendLine("- Provide extensive documentation and examples");
                prompt.AppendLine("- Implement guardrails to prevent common mistakes");
                break;

            case "advanced":
                prompt.AppendLine("## Advanced Team Optimizations:");
                prompt.AppendLine("- Implement advanced features like subscriptions and federation");
                prompt.AppendLine("- Set up sophisticated caching and performance strategies");
                prompt.AppendLine("- Use custom directives and advanced schema patterns");
                prompt.AppendLine("- Implement comprehensive monitoring and alerting");
                prompt.AppendLine("- Focus on architectural patterns and best practices");
                break;

            default: // intermediate
                prompt.AppendLine("## Intermediate Development Approach:");
                prompt.AppendLine("- Balance learning with productivity");
                prompt.AppendLine("- Implement standard patterns and practices");
                prompt.AppendLine("- Focus on practical application and real-world scenarios");
                prompt.AppendLine("- Provide guidance on common challenges and solutions");
                prompt.AppendLine("- Establish code review and quality processes");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Tools and Configuration:");
        prompt.AppendLine("1. **Use CompleteGraphQLWorkflow** to set up comprehensive development environment");
        prompt.AppendLine("2. **Use RegisterEndpoint** to properly configure the GraphQL endpoint");
        prompt.AppendLine("3. **Use CreateTestSuite** to establish testing procedures");
        prompt.AppendLine("4. **Use GenerateCode** to create typed client libraries");
        prompt.AppendLine("5. **Use DebugGraphQL** to set up debugging and troubleshooting procedures");
        prompt.AppendLine();

        prompt.AppendLine("## Success Criteria:");
        prompt.AppendLine("- Team can efficiently explore and understand GraphQL schemas");
        prompt.AppendLine("- Developers can confidently write and optimize GraphQL queries");
        prompt.AppendLine("- Comprehensive testing ensures quality and reliability");
        prompt.AppendLine("- Performance monitoring provides actionable insights");
        prompt.AppendLine("- Documentation supports team knowledge sharing and onboarding");

        return prompt.ToString();
    }
}