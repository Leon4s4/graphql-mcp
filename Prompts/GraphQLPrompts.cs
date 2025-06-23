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
        prompt.AppendLine();
        prompt.AppendLine("## Alternative: Use SchemaTools for schema analysis:");
        prompt.AppendLine("- IntrospectSchema for understanding the target GraphQL schema");
        prompt.AppendLine("- SearchSchema to find relevant types and operations");

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
        prompt.AppendLine("1. **Use GraphqlServiceManager** to set up comprehensive development environment");
        prompt.AppendLine("2. **Use RegisterEndpoint** to properly configure the GraphQL endpoint");
        prompt.AppendLine("3. **Use CreateTestSuite** (DevelopmentTools) to establish testing procedures");
        prompt.AppendLine("4. **Use GenerateCode** (UtilityTools) to create typed client libraries");
        prompt.AppendLine("5. **Use DebugGraphQL** (DevelopmentTools) to set up debugging and troubleshooting procedures");
        prompt.AppendLine("6. **Use IntrospectSchema** (SchemaTools) for comprehensive schema analysis");
        prompt.AppendLine("7. **Use AnalyzeQuery** (QueryTools) for query validation and optimization");
        prompt.AppendLine();

        prompt.AppendLine("## Success Criteria:");
        prompt.AppendLine("- Team can efficiently explore and understand GraphQL schemas");
        prompt.AppendLine("- Developers can confidently write and optimize GraphQL queries");
        prompt.AppendLine("- Comprehensive testing ensures quality and reliability");
        prompt.AppendLine("- Performance monitoring provides actionable insights");
        prompt.AppendLine("- Documentation supports team knowledge sharing and onboarding");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Optimize GraphQL queries for performance and efficiency")]
    public static string QueryOptimization(
        [Description("GraphQL query to optimize")]
        string query,
        [Description("Target endpoint name")]
        string endpointName,
        [Description("Optimization focus: performance, size, complexity, security")]
        string focus = "performance",
        [Description("Performance target: fast, balanced, comprehensive")]
        string target = "balanced")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Query Optimization Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL performance expert. Analyze and optimize the provided GraphQL query for maximum efficiency.");
        prompt.AppendLine();
        prompt.AppendLine("## Query to Optimize:");
        prompt.AppendLine("```graphql");
        prompt.AppendLine(query);
        prompt.AppendLine("```");
        prompt.AppendLine();
        prompt.AppendLine($"**Target Endpoint:** {endpointName}");
        prompt.AppendLine($"**Optimization Focus:** {focus}");
        prompt.AppendLine($"**Performance Target:** {target}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "performance":
                prompt.AppendLine("## Performance Optimization Focus:");
                prompt.AppendLine("- Reduce query depth and complexity");
                prompt.AppendLine("- Optimize field selection for minimal data transfer");
                prompt.AppendLine("- Implement proper pagination strategies");
                prompt.AppendLine("- Use fragments to reduce duplication");
                prompt.AppendLine("- Consider DataLoader patterns for N+1 prevention");
                break;

            case "size":
                prompt.AppendLine("## Size Optimization Focus:");
                prompt.AppendLine("- Minimize query string length");
                prompt.AppendLine("- Remove unnecessary fields and nesting");
                prompt.AppendLine("- Use aliases effectively");
                prompt.AppendLine("- Optimize variable usage");
                break;

            case "complexity":
                prompt.AppendLine("## Complexity Optimization Focus:");
                prompt.AppendLine("- Reduce query complexity score");
                prompt.AppendLine("- Simplify nested selections");
                prompt.AppendLine("- Break complex queries into smaller operations");
                prompt.AppendLine("- Optimize resolver execution paths");
                break;

            case "security":
                prompt.AppendLine("## Security Optimization Focus:");
                prompt.AppendLine("- Prevent potential DoS attacks");
                prompt.AppendLine("- Avoid sensitive data exposure");
                prompt.AppendLine("- Implement proper authorization checks");
                prompt.AppendLine("- Use variables instead of inline values");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Use OptimizeQuery tool with these parameters:");
        prompt.AppendLine($"- query: [the provided query]");
        prompt.AppendLine($"- optimizationType: \"{focus}\"");
        prompt.AppendLine("- extractVariables: true");
        prompt.AppendLine("- suggestFragments: true");
        prompt.AppendLine();
        prompt.AppendLine("## Use AnalyzeQuery tool for comprehensive analysis:");
        prompt.AppendLine($"- query: [the provided query]");
        prompt.AppendLine($"- endpointName: \"{endpointName}\"");
        prompt.AppendLine("- analysisLevel: \"comprehensive\"");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Comprehensive GraphQL security analysis and hardening guidance")]
    public static string GraphQLSecurity(
        [Description("Security focus: assessment, hardening, monitoring, compliance")]
        string focus = "assessment",
        [Description("Target endpoint name")]
        string? endpointName = null,
        [Description("Environment: development, staging, production")]
        string environment = "production",
        [Description("Compliance requirements: GDPR, HIPAA, SOC2, PCI")]
        string? compliance = null)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Security Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL security expert. Provide comprehensive security guidance for GraphQL APIs.");
        prompt.AppendLine();
        prompt.AppendLine($"**Security Focus:** {focus}");
        if (!string.IsNullOrEmpty(endpointName))
            prompt.AppendLine($"**Target Endpoint:** {endpointName}");
        prompt.AppendLine($"**Environment:** {environment}");
        if (!string.IsNullOrEmpty(compliance))
            prompt.AppendLine($"**Compliance Requirements:** {compliance}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "assessment":
                prompt.AppendLine("## Security Assessment Areas:");
                prompt.AppendLine("### Query Security:");
                prompt.AppendLine("- Query complexity and depth limits");
                prompt.AppendLine("- Rate limiting and request throttling");
                prompt.AppendLine("- Query whitelisting/blacklisting");
                prompt.AppendLine("- Introspection exposure in production");
                prompt.AppendLine();
                prompt.AppendLine("### Authentication & Authorization:");
                prompt.AppendLine("- Field-level authorization patterns");
                prompt.AppendLine("- Token validation and expiration");
                prompt.AppendLine("- Role-based access control (RBAC)");
                prompt.AppendLine("- Context-aware permissions");
                prompt.AppendLine();
                prompt.AppendLine("### Data Protection:");
                prompt.AppendLine("- Sensitive data exposure prevention");
                prompt.AppendLine("- Input validation and sanitization");
                prompt.AppendLine("- Error message information leakage");
                prompt.AppendLine("- Data masking and filtering");
                break;

            case "hardening":
                prompt.AppendLine("## Security Hardening Checklist:");
                prompt.AppendLine("### Infrastructure Security:");
                prompt.AppendLine("- HTTPS enforcement with proper TLS configuration");
                prompt.AppendLine("- CORS policy configuration");
                prompt.AppendLine("- HTTP security headers implementation");
                prompt.AppendLine("- Network-level access controls");
                prompt.AppendLine();
                prompt.AppendLine("### Application Security:");
                prompt.AppendLine("- Query complexity analysis and limits");
                prompt.AppendLine("- Request size and timeout limits");
                prompt.AppendLine("- Disable introspection in production");
                prompt.AppendLine("- Implement query whitelisting");
                break;

            case "monitoring":
                prompt.AppendLine("## Security Monitoring Strategy:");
                prompt.AppendLine("### Real-time Monitoring:");
                prompt.AppendLine("- Suspicious query pattern detection");
                prompt.AppendLine("- Abnormal request volume alerts");
                prompt.AppendLine("- Failed authentication tracking");
                prompt.AppendLine("- Performance anomaly detection");
                prompt.AppendLine();
                prompt.AppendLine("### Audit and Logging:");
                prompt.AppendLine("- Comprehensive request logging");
                prompt.AppendLine("- Security event correlation");
                prompt.AppendLine("- Compliance audit trails");
                prompt.AppendLine("- Incident response procedures");
                break;

            case "compliance":
                prompt.AppendLine("## Compliance Implementation:");
                if (!string.IsNullOrEmpty(compliance))
                {
                    switch (compliance.ToUpper())
                    {
                        case "GDPR":
                            prompt.AppendLine("### GDPR Compliance:");
                            prompt.AppendLine("- Data minimization in GraphQL responses");
                            prompt.AppendLine("- Right to erasure implementation");
                            prompt.AppendLine("- Consent management integration");
                            prompt.AppendLine("- Data portability features");
                            break;
                        case "HIPAA":
                            prompt.AppendLine("### HIPAA Compliance:");
                            prompt.AppendLine("- PHI data encryption and protection");
                            prompt.AppendLine("- Access logging and audit trails");
                            prompt.AppendLine("- Minimum necessary access principles");
                            prompt.AppendLine("- Breach notification procedures");
                            break;
                    }
                }
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Tools to Use:");
        if (!string.IsNullOrEmpty(endpointName))
        {
            prompt.AppendLine($"1. **AnalyzeQuery** - Security analysis of queries");
            prompt.AppendLine($"2. **IntrospectSchema** - Schema security assessment");
            prompt.AppendLine($"3. **DebugGraphQL** - Security issue debugging");
        }
        prompt.AppendLine("4. **CreateTestSuite** - Security testing scenarios");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Track and analyze GraphQL schema evolution over time")]
    public static string SchemaEvolution(
        [Description("Base endpoint for comparison")]
        string baseEndpoint,
        [Description("Evolution analysis type: changes, impact, migration, versioning")]
        string analysisType = "changes",
        [Description("Target endpoint for comparison")]
        string? targetEndpoint = null,
        [Description("Time period: days, weeks, months")]
        string timePeriod = "months")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Schema Evolution Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL schema evolution expert. Analyze schema changes and provide migration guidance.");
        prompt.AppendLine();
        prompt.AppendLine($"**Analysis Type:** {analysisType}");
        prompt.AppendLine($"**Base Endpoint:** {baseEndpoint}");
        if (!string.IsNullOrEmpty(targetEndpoint))
            prompt.AppendLine($"**Target Endpoint:** {targetEndpoint}");
        prompt.AppendLine($"**Time Period:** {timePeriod}");
        prompt.AppendLine();

        switch (analysisType.ToLower())
        {
            case "changes":
                prompt.AppendLine("## Schema Change Analysis:");
                prompt.AppendLine("### Breaking Changes Detection:");
                prompt.AppendLine("- Removed fields, types, or operations");
                prompt.AppendLine("- Changed field types or nullability");
                prompt.AppendLine("- Modified argument requirements");
                prompt.AppendLine("- Deprecated field removals");
                prompt.AppendLine();
                prompt.AppendLine("### Non-Breaking Changes:");
                prompt.AppendLine("- New fields, types, or operations");
                prompt.AppendLine("- Added optional arguments");
                prompt.AppendLine("- Enhanced descriptions");
                prompt.AppendLine("- New deprecation markers");
                break;

            case "impact":
                prompt.AppendLine("## Impact Assessment:");
                prompt.AppendLine("### Client Impact Analysis:");
                prompt.AppendLine("- Affected client applications");
                prompt.AppendLine("- Query compatibility assessment");
                prompt.AppendLine("- Migration effort estimation");
                prompt.AppendLine("- Rollback considerations");
                prompt.AppendLine();
                prompt.AppendLine("### Business Impact:");
                prompt.AppendLine("- Feature availability changes");
                prompt.AppendLine("- Performance implications");
                prompt.AppendLine("- User experience effects");
                prompt.AppendLine("- Integration disruptions");
                break;

            case "migration":
                prompt.AppendLine("## Migration Strategy:");
                prompt.AppendLine("### Phased Migration Plan:");
                prompt.AppendLine("- Deprecation announcement timeline");
                prompt.AppendLine("- Client update coordination");
                prompt.AppendLine("- Backward compatibility period");
                prompt.AppendLine("- Legacy support sunset");
                prompt.AppendLine();
                prompt.AppendLine("### Migration Tools:");
                prompt.AppendLine("- Query transformation utilities");
                prompt.AppendLine("- Client code generators");
                prompt.AppendLine("- Testing and validation tools");
                prompt.AppendLine("- Monitoring and rollback procedures");
                break;

            case "versioning":
                prompt.AppendLine("## Schema Versioning Strategy:");
                prompt.AppendLine("### Versioning Approaches:");
                prompt.AppendLine("- Additive-only schema evolution");
                prompt.AppendLine("- Semantic versioning for schemas");
                prompt.AppendLine("- Feature flags and conditional fields");
                prompt.AppendLine("- Multiple schema versions support");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Tools to Use:");
        prompt.AppendLine($"1. **CompareSchemas** - Compare base and target schemas");
        prompt.AppendLine($"2. **IntrospectSchema** - Analyze current schema state");
        if (!string.IsNullOrEmpty(targetEndpoint))
            prompt.AppendLine($"3. **CompareAndAnalyzeSchemas** - Multi-endpoint comparison");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Systematic GraphQL error diagnosis and troubleshooting")]
    public static string ErrorDiagnosis(
        [Description("Error message or response")]
        string errorInput,
        [Description("Error category: syntax, schema, resolver, network, performance")]
        string category = "unknown",
        [Description("Severity level: critical, high, medium, low")]
        string severity = "medium",
        [Description("Context information: query, endpoint, environment details")]
        string? context = null)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Error Diagnosis Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL debugging expert. Provide systematic diagnosis and resolution for GraphQL errors.");
        prompt.AppendLine();
        prompt.AppendLine("## Error Information:");
        prompt.AppendLine("```");
        prompt.AppendLine(errorInput);
        prompt.AppendLine("```");
        prompt.AppendLine();
        prompt.AppendLine($"**Category:** {category}");
        prompt.AppendLine($"**Severity:** {severity}");
        if (!string.IsNullOrEmpty(context))
        {
            prompt.AppendLine($"**Context:** {context}");
        }
        prompt.AppendLine();

        prompt.AppendLine("## Diagnostic Approach:");
        
        switch (category.ToLower())
        {
            case "syntax":
                prompt.AppendLine("### Syntax Error Diagnosis:");
                prompt.AppendLine("1. **Query Structure Validation**:");
                prompt.AppendLine("   - Check balanced braces and parentheses");
                prompt.AppendLine("   - Verify proper field selection syntax");
                prompt.AppendLine("   - Validate fragment definitions and usage");
                prompt.AppendLine("   - Confirm variable declarations");
                prompt.AppendLine();
                prompt.AppendLine("2. **Common Syntax Issues**:");
                prompt.AppendLine("   - Missing commas between fields");
                prompt.AppendLine("   - Incorrect argument syntax");
                prompt.AppendLine("   - Invalid characters in names");
                prompt.AppendLine("   - Malformed string literals");
                break;

            case "schema":
                prompt.AppendLine("### Schema Error Diagnosis:");
                prompt.AppendLine("1. **Schema Compliance Check**:");
                prompt.AppendLine("   - Verify field existence in target types");
                prompt.AppendLine("   - Check argument requirements and types");
                prompt.AppendLine("   - Validate enum values and input types");
                prompt.AppendLine("   - Confirm interface implementations");
                prompt.AppendLine();
                prompt.AppendLine("2. **Schema Evolution Issues**:");
                prompt.AppendLine("   - Check for deprecated field usage");
                prompt.AppendLine("   - Verify schema version compatibility");
                prompt.AppendLine("   - Look for recent breaking changes");
                break;

            case "resolver":
                prompt.AppendLine("### Resolver Error Diagnosis:");
                prompt.AppendLine("1. **Resolver Implementation**:");
                prompt.AppendLine("   - Check resolver function existence");
                prompt.AppendLine("   - Verify return type compatibility");
                prompt.AppendLine("   - Validate context and argument usage");
                prompt.AppendLine("   - Review error handling patterns");
                prompt.AppendLine();
                prompt.AppendLine("2. **Data Source Issues**:");
                prompt.AppendLine("   - Database connectivity and queries");
                prompt.AppendLine("   - External API integration problems");
                prompt.AppendLine("   - Permission and authorization failures");
                break;

            case "performance":
                prompt.AppendLine("### Performance Error Diagnosis:");
                prompt.AppendLine("1. **Query Complexity Analysis**:");
                prompt.AppendLine("   - Calculate query complexity score");
                prompt.AppendLine("   - Identify deep nesting issues");
                prompt.AppendLine("   - Check for N+1 query problems");
                prompt.AppendLine("   - Analyze resolver execution time");
                prompt.AppendLine();
                prompt.AppendLine("2. **Resource Constraints**:");
                prompt.AppendLine("   - Memory usage and limits");
                prompt.AppendLine("   - Database connection pooling");
                prompt.AppendLine("   - Network latency and timeouts");
                break;

            default:
                prompt.AppendLine("### General Error Diagnosis:");
                prompt.AppendLine("1. **Error Pattern Analysis**:");
                prompt.AppendLine("   - Classify error type and source");
                prompt.AppendLine("   - Identify error frequency and conditions");
                prompt.AppendLine("   - Check for environmental factors");
                prompt.AppendLine("   - Review recent changes and deployments");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Tools to Use:");
        prompt.AppendLine("1. **ExplainError** (DevelopmentTools) - Detailed error analysis");
        prompt.AppendLine("2. **DebugGraphQL** (DevelopmentTools) - Comprehensive debugging");
        if (!string.IsNullOrEmpty(context) && context.Contains("query"))
        {
            prompt.AppendLine("3. **AnalyzeQuery** (QueryTools) - Query-specific analysis");
        }

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Performance monitoring and profiling for GraphQL operations")]
    public static string PerformanceMonitoring(
        [Description("Monitoring focus: setup, analysis, optimization, alerts")]
        string focus = "analysis",
        [Description("Target endpoint name")]
        string? endpointName = null,
        [Description("Performance metrics: latency, throughput, errors, complexity")]
        string metrics = "latency",
        [Description("Environment: development, staging, production")]
        string environment = "production")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Performance Monitoring Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL performance expert. Provide comprehensive performance monitoring and optimization guidance.");
        prompt.AppendLine();
        prompt.AppendLine($"**Monitoring Focus:** {focus}");
        if (!string.IsNullOrEmpty(endpointName))
            prompt.AppendLine($"**Target Endpoint:** {endpointName}");
        prompt.AppendLine($"**Performance Metrics:** {metrics}");
        prompt.AppendLine($"**Environment:** {environment}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "setup":
                prompt.AppendLine("## Performance Monitoring Setup:");
                prompt.AppendLine("### Metrics Collection:");
                prompt.AppendLine("- Query execution time tracking");
                prompt.AppendLine("- Resolver-level performance monitoring");
                prompt.AppendLine("- Database query performance");
                prompt.AppendLine("- Memory and CPU utilization");
                prompt.AppendLine();
                prompt.AppendLine("### Monitoring Tools Integration:");
                prompt.AppendLine("- Application Performance Monitoring (APM)");
                prompt.AppendLine("- Custom metrics and dashboards");
                prompt.AppendLine("- Real-time alerting systems");
                prompt.AppendLine("- Log aggregation and analysis");
                break;

            case "analysis":
                prompt.AppendLine("## Performance Analysis Framework:");
                prompt.AppendLine("### Query Performance Analysis:");
                prompt.AppendLine("- Identify slow-performing queries");
                prompt.AppendLine("- Analyze query complexity patterns");
                prompt.AppendLine("- Track performance trends over time");
                prompt.AppendLine("- Compare performance across environments");
                prompt.AppendLine();
                prompt.AppendLine("### Bottleneck Identification:");
                prompt.AppendLine("- Resolver performance profiling");
                prompt.AppendLine("- Database query optimization opportunities");
                prompt.AppendLine("- Network latency impact assessment");
                prompt.AppendLine("- Caching effectiveness analysis");
                break;

            case "optimization":
                prompt.AppendLine("## Performance Optimization Strategy:");
                prompt.AppendLine("### Query Optimization:");
                prompt.AppendLine("- Query complexity reduction techniques");
                prompt.AppendLine("- Field selection optimization");
                prompt.AppendLine("- Pagination implementation");
                prompt.AppendLine("- Fragment usage for efficiency");
                prompt.AppendLine();
                prompt.AppendLine("### System Optimization:");
                prompt.AppendLine("- DataLoader implementation for N+1 prevention");
                prompt.AppendLine("- Response caching strategies");
                prompt.AppendLine("- Database query optimization");
                prompt.AppendLine("- Resource scaling recommendations");
                break;

            case "alerts":
                prompt.AppendLine("## Performance Alerting Strategy:");
                prompt.AppendLine("### Alert Thresholds:");
                prompt.AppendLine("- Response time SLA violations");
                prompt.AppendLine("- Error rate threshold breaches");
                prompt.AppendLine("- Query complexity limit warnings");
                prompt.AppendLine("- Resource utilization alerts");
                prompt.AppendLine();
                prompt.AppendLine("### Incident Response:");
                prompt.AppendLine("- Automated scaling triggers");
                prompt.AppendLine("- Performance degradation workflows");
                prompt.AppendLine("- Escalation procedures");
                prompt.AppendLine("- Post-incident analysis processes");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Tools to Use:");
        if (!string.IsNullOrEmpty(endpointName))
        {
            prompt.AppendLine($"1. **ExecuteQuery** (QueryTools) - Query performance testing");
            prompt.AppendLine($"2. **AnalyzeQuery** (QueryTools) - Performance analysis");
        }
        prompt.AppendLine("3. **DebugGraphQL** (DevelopmentTools) - Performance debugging");
        prompt.AppendLine("4. **CreateTestSuite** (DevelopmentTools) - Performance test scenarios");

        return prompt.ToString();
    }
}