using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Prompts;

[McpServerPromptType]
public class OrganizationalPrompts
{
    [McpServerPrompt, Description("GraphQL governance and team standards")]
    public static string GraphQLGovernance(
        [Description("Governance focus: standards, review, quality, compliance")]
        string focus = "standards",
        [Description("Team size: small, medium, large, enterprise")]
        string teamSize = "medium",
        [Description("Organization type: startup, corporate, agency, open-source")]
        string orgType = "corporate")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Governance Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL governance expert. Provide guidance for establishing effective GraphQL governance practices.");
        prompt.AppendLine();
        prompt.AppendLine($"**Governance Focus:** {focus}");
        prompt.AppendLine($"**Team Size:** {teamSize}");
        prompt.AppendLine($"**Organization Type:** {orgType}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "standards":
                prompt.AppendLine("## GraphQL Standards Framework:");
                prompt.AppendLine("### Schema Design Standards:");
                prompt.AppendLine("- Naming conventions for types, fields, and arguments");
                prompt.AppendLine("- Documentation requirements and templates");
                prompt.AppendLine("- Type system patterns and best practices");
                prompt.AppendLine("- Error handling and response structures");
                prompt.AppendLine("- Deprecation policies and migration paths");
                prompt.AppendLine();
                prompt.AppendLine("### Development Standards:");
                prompt.AppendLine("- Code organization and file structure");
                prompt.AppendLine("- Resolver implementation patterns");
                prompt.AppendLine("- Testing requirements and coverage");
                prompt.AppendLine("- Performance and security guidelines");
                prompt.AppendLine("- Integration testing standards");
                break;

            case "review":
                prompt.AppendLine("## Code Review Process:");
                prompt.AppendLine("### Schema Review Guidelines:");
                prompt.AppendLine("- Schema change impact assessment");
                prompt.AppendLine("- Breaking change identification");
                prompt.AppendLine("- Performance impact evaluation");
                prompt.AppendLine("- Security vulnerability checks");
                prompt.AppendLine("- Documentation completeness review");
                prompt.AppendLine();
                prompt.AppendLine("### Review Automation:");
                prompt.AppendLine("- Automated schema validation");
                prompt.AppendLine("- Linting and code quality checks");
                prompt.AppendLine("- Performance regression testing");
                prompt.AppendLine("- Security scanning integration");
                break;

            case "quality":
                prompt.AppendLine("## Quality Assurance Framework:");
                prompt.AppendLine("### Quality Metrics:");
                prompt.AppendLine("- Schema quality scoring");
                prompt.AppendLine("- Query performance benchmarks");
                prompt.AppendLine("- Error rate monitoring");
                prompt.AppendLine("- Documentation coverage metrics");
                prompt.AppendLine("- Client satisfaction indicators");
                prompt.AppendLine();
                prompt.AppendLine("### Quality Gates:");
                prompt.AppendLine("- Pre-deployment validation checks");
                prompt.AppendLine("- Performance threshold enforcement");
                prompt.AppendLine("- Security compliance verification");
                prompt.AppendLine("- Breaking change approval process");
                break;

            case "compliance":
                prompt.AppendLine("## Compliance Management:");
                prompt.AppendLine("### Regulatory Compliance:");
                prompt.AppendLine("- Data privacy regulation adherence");
                prompt.AppendLine("- Industry-specific compliance requirements");
                prompt.AppendLine("- Audit trail and documentation");
                prompt.AppendLine("- Change management processes");
                prompt.AppendLine();
                prompt.AppendLine("### Internal Compliance:");
                prompt.AppendLine("- Corporate policy alignment");
                prompt.AppendLine("- Security policy enforcement");
                prompt.AppendLine("- Development standard compliance");
                prompt.AppendLine("- Documentation requirements");
                break;
        }

        switch (teamSize.ToLower())
        {
            case "large":
            case "enterprise":
                prompt.AppendLine();
                prompt.AppendLine("## Enterprise Governance Considerations:");
                prompt.AppendLine("- Cross-team coordination mechanisms");
                prompt.AppendLine("- Centralized schema registry management");
                prompt.AppendLine("- Federated governance across business units");
                prompt.AppendLine("- Escalation procedures for conflicts");
                prompt.AppendLine("- Executive reporting and metrics");
                break;

            case "small":
                prompt.AppendLine();
                prompt.AppendLine("## Small Team Governance:");
                prompt.AppendLine("- Lightweight process implementation");
                prompt.AppendLine("- Automated governance where possible");
                prompt.AppendLine("- Flexible standards adaptation");
                prompt.AppendLine("- Growth-oriented governance planning");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Governance Tools:");
        prompt.AppendLine("1. **IntrospectSchema** - Schema compliance analysis");
        prompt.AppendLine("2. **CompareSchemas** - Change impact assessment");
        prompt.AppendLine("3. **CreateTestSuite** - Quality assurance testing");
        prompt.AppendLine("4. **AnalyzeQuery** - Query quality assessment");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("GraphQL code review guidelines and best practices")]
    public static string CodeReviewGraphQL(
        [Description("Review focus: schema, resolvers, queries, performance")]
        string focus = "schema",
        [Description("Review depth: basic, standard, comprehensive")]
        string depth = "standard",
        [Description("Team experience: junior, mixed, senior")]
        string experience = "mixed")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Code Review Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL code review expert. Provide comprehensive review guidelines for GraphQL code.");
        prompt.AppendLine();
        prompt.AppendLine($"**Review Focus:** {focus}");
        prompt.AppendLine($"**Review Depth:** {depth}");
        prompt.AppendLine($"**Team Experience:** {experience}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "schema":
                prompt.AppendLine("## Schema Review Checklist:");
                prompt.AppendLine("### Design Quality:");
                prompt.AppendLine("- Type and field naming consistency");
                prompt.AppendLine("- Appropriate nullability declarations");
                prompt.AppendLine("- Proper input type design");
                prompt.AppendLine("- Interface and union usage appropriateness");
                prompt.AppendLine("- Enum value completeness and naming");
                prompt.AppendLine();
                prompt.AppendLine("### Breaking Changes:");
                prompt.AppendLine("- Field removal or type changes");
                prompt.AppendLine("- Argument requirement changes");
                prompt.AppendLine("- Enum value modifications");
                prompt.AppendLine("- Interface implementation changes");
                prompt.AppendLine();
                prompt.AppendLine("### Documentation:");
                prompt.AppendLine("- Type and field descriptions");
                prompt.AppendLine("- Deprecation notices and migration guides");
                prompt.AppendLine("- Example usage documentation");
                prompt.AppendLine("- Complex business logic explanations");
                break;

            case "resolvers":
                prompt.AppendLine("## Resolver Review Checklist:");
                prompt.AppendLine("### Implementation Quality:");
                prompt.AppendLine("- Proper error handling and propagation");
                prompt.AppendLine("- Input validation and sanitization");
                prompt.AppendLine("- Appropriate async/await usage");
                prompt.AppendLine("- Context object utilization");
                prompt.AppendLine("- Return type compliance with schema");
                prompt.AppendLine();
                prompt.AppendLine("### Performance Considerations:");
                prompt.AppendLine("- N+1 query prevention strategies");
                prompt.AppendLine("- DataLoader implementation");
                prompt.AppendLine("- Database query optimization");
                prompt.AppendLine("- Caching strategy implementation");
                prompt.AppendLine();
                prompt.AppendLine("### Security Reviews:");
                prompt.AppendLine("- Authorization check implementation");
                prompt.AppendLine("- Data access permission validation");
                prompt.AppendLine("- Input injection prevention");
                prompt.AppendLine("- Sensitive data exposure protection");
                break;

            case "queries":
                prompt.AppendLine("## Query Review Checklist:");
                prompt.AppendLine("### Query Quality:");
                prompt.AppendLine("- Field selection optimization");
                prompt.AppendLine("- Fragment usage and reusability");
                prompt.AppendLine("- Variable usage and typing");
                prompt.AppendLine("- Query complexity and depth");
                prompt.AppendLine();
                prompt.AppendLine("### Best Practices:");
                prompt.AppendLine("- Proper alias usage for conflicts");
                prompt.AppendLine("- Pagination implementation");
                prompt.AppendLine("- Error handling strategies");
                prompt.AppendLine("- Cache optimization considerations");
                break;

            case "performance":
                prompt.AppendLine("## Performance Review Checklist:");
                prompt.AppendLine("### Query Performance:");
                prompt.AppendLine("- Query complexity analysis");
                prompt.AppendLine("- Execution time optimization");
                prompt.AppendLine("- Resource usage assessment");
                prompt.AppendLine("- Scalability considerations");
                prompt.AppendLine();
                prompt.AppendLine("### System Performance:");
                prompt.AppendLine("- Memory usage patterns");
                prompt.AppendLine("- Database query efficiency");
                prompt.AppendLine("- Caching strategy effectiveness");
                prompt.AppendLine("- Connection pooling optimization");
                break;
        }

        switch (experience.ToLower())
        {
            case "junior":
                prompt.AppendLine();
                prompt.AppendLine("## Junior Developer Guidance:");
                prompt.AppendLine("- Focus on fundamental GraphQL concepts");
                prompt.AppendLine("- Provide detailed explanations for feedback");
                prompt.AppendLine("- Include learning resources and references");
                prompt.AppendLine("- Emphasize best practice adoption");
                break;

            case "senior":
                prompt.AppendLine();
                prompt.AppendLine("## Senior Developer Review:");
                prompt.AppendLine("- Focus on architectural and design decisions");
                prompt.AppendLine("- Discuss trade-offs and alternatives");
                prompt.AppendLine("- Consider long-term maintenance implications");
                prompt.AppendLine("- Evaluate innovation and optimization opportunities");
                break;

            default:
                prompt.AppendLine();
                prompt.AppendLine("## Mixed Experience Team:");
                prompt.AppendLine("- Provide context for different experience levels");
                prompt.AppendLine("- Include both fundamental and advanced considerations");
                prompt.AppendLine("- Encourage knowledge sharing opportunities");
                prompt.AppendLine("- Balance thoroughness with practicality");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Code Review Tools:");
        prompt.AppendLine("1. **AnalyzeQuery** - Automated query quality analysis");
        prompt.AppendLine("2. **IntrospectSchema** - Schema structure validation");
        prompt.AppendLine("3. **DebugGraphQL** - Issue identification and debugging");
        prompt.AppendLine("4. **CompareSchemas** - Breaking change detection");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("GraphQL team onboarding and knowledge transfer")]
    public static string GraphQLOnboarding(
        [Description("Role: developer, qa, devops, manager, designer")]
        string role = "developer",
        [Description("Experience level: beginner, intermediate, advanced")]
        string level = "beginner",
        [Description("Timeline: week, month, quarter")]
        string timeline = "month",
        [Description("Focus areas: concepts, tools, practices, architecture")]
        string focus = "concepts")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Team Onboarding Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL onboarding expert. Create comprehensive onboarding plans for team members joining GraphQL projects.");
        prompt.AppendLine();
        prompt.AppendLine($"**Role:** {role}");
        prompt.AppendLine($"**Experience Level:** {level}");
        prompt.AppendLine($"**Timeline:** {timeline}");
        prompt.AppendLine($"**Focus Areas:** {focus}");
        prompt.AppendLine();

        switch (role.ToLower())
        {
            case "developer":
                prompt.AppendLine("## Developer Onboarding Path:");
                prompt.AppendLine("### Week 1 - Fundamentals:");
                prompt.AppendLine("- GraphQL concepts and terminology");
                prompt.AppendLine("- Schema definition language (SDL)");
                prompt.AppendLine("- Query, mutation, and subscription basics");
                prompt.AppendLine("- GraphQL vs REST comparison");
                prompt.AppendLine("- Hands-on playground exploration");
                prompt.AppendLine();
                prompt.AppendLine("### Week 2-3 - Implementation:");
                prompt.AppendLine("- Server-side GraphQL implementation");
                prompt.AppendLine("- Resolver patterns and best practices");
                prompt.AppendLine("- Client-side integration techniques");
                prompt.AppendLine("- Error handling and debugging");
                prompt.AppendLine("- Testing strategies and tools");
                prompt.AppendLine();
                prompt.AppendLine("### Week 4+ - Advanced Topics:");
                prompt.AppendLine("- Performance optimization techniques");
                prompt.AppendLine("- Security considerations and implementation");
                prompt.AppendLine("- Caching strategies and patterns");
                prompt.AppendLine("- Schema evolution and migration");
                break;

            case "qa":
                prompt.AppendLine("## QA Onboarding Path:");
                prompt.AppendLine("### Testing Fundamentals:");
                prompt.AppendLine("- GraphQL API testing concepts");
                prompt.AppendLine("- Query validation and verification");
                prompt.AppendLine("- Schema testing and validation");
                prompt.AppendLine("- Error scenario testing");
                prompt.AppendLine();
                prompt.AppendLine("### Testing Tools and Techniques:");
                prompt.AppendLine("- GraphQL testing frameworks");
                prompt.AppendLine("- Automated test suite creation");
                prompt.AppendLine("- Performance testing strategies");
                prompt.AppendLine("- Security testing approaches");
                prompt.AppendLine();
                prompt.AppendLine("### Quality Assurance:");
                prompt.AppendLine("- Quality metrics and measurement");
                prompt.AppendLine("- Regression testing procedures");
                prompt.AppendLine("- Integration testing patterns");
                break;

            case "devops":
                prompt.AppendLine("## DevOps Onboarding Path:");
                prompt.AppendLine("### Infrastructure Basics:");
                prompt.AppendLine("- GraphQL server deployment patterns");
                prompt.AppendLine("- Container orchestration for GraphQL");
                prompt.AppendLine("- Load balancing and scaling strategies");
                prompt.AppendLine("- Monitoring and observability setup");
                prompt.AppendLine();
                prompt.AppendLine("### Operations Management:");
                prompt.AppendLine("- CI/CD pipeline configuration");
                prompt.AppendLine("- Schema deployment strategies");
                prompt.AppendLine("- Performance monitoring tools");
                prompt.AppendLine("- Incident response procedures");
                break;

            case "manager":
                prompt.AppendLine("## Management Onboarding Path:");
                prompt.AppendLine("### Business Understanding:");
                prompt.AppendLine("- GraphQL business value proposition");
                prompt.AppendLine("- ROI metrics and measurement");
                prompt.AppendLine("- Risk assessment and mitigation");
                prompt.AppendLine("- Team productivity implications");
                prompt.AppendLine();
                prompt.AppendLine("### Team Management:");
                prompt.AppendLine("- GraphQL team structure and roles");
                prompt.AppendLine("- Skill development planning");
                prompt.AppendLine("- Resource allocation strategies");
                prompt.AppendLine("- Success metrics and KPIs");
                break;

            case "designer":
                prompt.AppendLine("## Designer Onboarding Path:");
                prompt.AppendLine("### Design Considerations:");
                prompt.AppendLine("- GraphQL impact on user experience");
                prompt.AppendLine("- API design and user interface alignment");
                prompt.AppendLine("- Real-time feature design patterns");
                prompt.AppendLine("- Performance implications for UX");
                prompt.AppendLine();
                prompt.AppendLine("### Collaboration:");
                prompt.AppendLine("- Designer-developer collaboration");
                prompt.AppendLine("- Schema design input and feedback");
                prompt.AppendLine("- User story to GraphQL mapping");
                break;
        }

        switch (level.ToLower())
        {
            case "beginner":
                prompt.AppendLine();
                prompt.AppendLine("## Beginner Learning Approach:");
                prompt.AppendLine("- Start with fundamental concepts");
                prompt.AppendLine("- Use interactive learning tools");
                prompt.AppendLine("- Provide extensive documentation");
                prompt.AppendLine("- Include practical exercises");
                prompt.AppendLine("- Assign mentor for guidance");
                break;

            case "advanced":
                prompt.AppendLine();
                prompt.AppendLine("## Advanced Learning Approach:");
                prompt.AppendLine("- Focus on architectural patterns");
                prompt.AppendLine("- Discuss advanced optimization techniques");
                prompt.AppendLine("- Explore cutting-edge features");
                prompt.AppendLine("- Provide complex problem-solving scenarios");
                prompt.AppendLine("- Encourage innovation and experimentation");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Onboarding Tools:");
        prompt.AppendLine("1. **IntrospectSchema** - Explore project schemas");
        prompt.AppendLine("2. **GenerateTestData** - Practice with realistic data");
        prompt.AppendLine("3. **CreateTestSuite** - Learn testing approaches");
        prompt.AppendLine("4. **DebugGraphQL** - Develop troubleshooting skills");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("GraphQL KPIs and success metrics measurement")]
    public static string GraphQLMetrics(
        [Description("Metric focus: performance, adoption, quality, business")]
        string focus = "performance",
        [Description("Stakeholder: technical, business, executive")]
        string stakeholder = "technical",
        [Description("Reporting frequency: daily, weekly, monthly, quarterly")]
        string frequency = "weekly")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# GraphQL Metrics and KPIs Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL metrics expert. Provide guidance for measuring GraphQL success and performance.");
        prompt.AppendLine();
        prompt.AppendLine($"**Metric Focus:** {focus}");
        prompt.AppendLine($"**Stakeholder:** {stakeholder}");
        prompt.AppendLine($"**Reporting Frequency:** {frequency}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "performance":
                prompt.AppendLine("## Performance Metrics:");
                prompt.AppendLine("### Response Time Metrics:");
                prompt.AppendLine("- Average query execution time");
                prompt.AppendLine("- 95th percentile response times");
                prompt.AppendLine("- Resolver-level performance breakdown");
                prompt.AppendLine("- Database query execution times");
                prompt.AppendLine();
                prompt.AppendLine("### Throughput Metrics:");
                prompt.AppendLine("- Queries per second (QPS)");
                prompt.AppendLine("- Concurrent user capacity");
                prompt.AppendLine("- Peak load handling capability");
                prompt.AppendLine("- Resource utilization efficiency");
                prompt.AppendLine();
                prompt.AppendLine("### Quality Metrics:");
                prompt.AppendLine("- Error rate and error types");
                prompt.AppendLine("- Query complexity distribution");
                prompt.AppendLine("- Cache hit ratio and effectiveness");
                prompt.AppendLine("- N+1 query detection and prevention");
                break;

            case "adoption":
                prompt.AppendLine("## Adoption Metrics:");
                prompt.AppendLine("### Usage Metrics:");
                prompt.AppendLine("- Active API consumers and applications");
                prompt.AppendLine("- Query volume and growth trends");
                prompt.AppendLine("- Feature adoption rates");
                prompt.AppendLine("- Developer onboarding success rate");
                prompt.AppendLine();
                prompt.AppendLine("### Engagement Metrics:");
                prompt.AppendLine("- API documentation page views");
                prompt.AppendLine("- Developer community participation");
                prompt.AppendLine("- Support ticket volume and resolution");
                prompt.AppendLine("- Training completion rates");
                break;

            case "quality":
                prompt.AppendLine("## Quality Metrics:");
                prompt.AppendLine("### Code Quality:");
                prompt.AppendLine("- Schema design consistency scores");
                prompt.AppendLine("- Code review feedback trends");
                prompt.AppendLine("- Technical debt accumulation");
                prompt.AppendLine("- Test coverage and quality");
                prompt.AppendLine();
                prompt.AppendLine("### Operational Quality:");
                prompt.AppendLine("- Deployment success rate");
                prompt.AppendLine("- Incident frequency and severity");
                prompt.AppendLine("- Mean time to recovery (MTTR)");
                prompt.AppendLine("- Security vulnerability trends");
                break;

            case "business":
                prompt.AppendLine("## Business Metrics:");
                prompt.AppendLine("### Value Delivery:");
                prompt.AppendLine("- Development velocity improvements");
                prompt.AppendLine("- Time to market for new features");
                prompt.AppendLine("- API integration success rate");
                prompt.AppendLine("- Customer satisfaction scores");
                prompt.AppendLine();
                prompt.AppendLine("### Cost Efficiency:");
                prompt.AppendLine("- Infrastructure cost optimization");
                prompt.AppendLine("- Development resource efficiency");
                prompt.AppendLine("- Maintenance effort reduction");
                prompt.AppendLine("- ROI on GraphQL investment");
                break;
        }

        switch (stakeholder.ToLower())
        {
            case "technical":
                prompt.AppendLine();
                prompt.AppendLine("## Technical Stakeholder Reports:");
                prompt.AppendLine("- Detailed performance analytics");
                prompt.AppendLine("- Technical debt and improvement opportunities");
                prompt.AppendLine("- System health and reliability metrics");
                prompt.AppendLine("- Development productivity indicators");
                break;

            case "business":
                prompt.AppendLine();
                prompt.AppendLine("## Business Stakeholder Reports:");
                prompt.AppendLine("- Feature delivery velocity");
                prompt.AppendLine("- Customer impact and satisfaction");
                prompt.AppendLine("- Cost-benefit analysis");
                prompt.AppendLine("- Market competitiveness indicators");
                break;

            case "executive":
                prompt.AppendLine();
                prompt.AppendLine("## Executive Stakeholder Reports:");
                prompt.AppendLine("- High-level ROI and business value");
                prompt.AppendLine("- Strategic initiative progress");
                prompt.AppendLine("- Risk assessment and mitigation");
                prompt.AppendLine("- Competitive advantage indicators");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Metrics Collection Tools:");
        prompt.AppendLine("1. **ExecuteQuery** - Performance benchmarking");
        prompt.AppendLine("2. **AnalyzeQuery** - Quality assessment metrics");
        prompt.AppendLine("3. **IntrospectSchema** - Schema complexity analysis");
        prompt.AppendLine("4. **CreateTestSuite** - Quality validation metrics");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Legacy system integration with GraphQL")]
    public static string LegacySystemIntegration(
        [Description("Legacy system type: database, rest-api, soap, mainframe")]
        string legacyType = "rest-api",
        [Description("Integration approach: wrapper, federation, migration")]
        string approach = "wrapper",
        [Description("Complexity level: simple, moderate, complex")]
        string complexity = "moderate",
        [Description("Timeline constraints: tight, moderate, flexible")]
        string timeline = "moderate")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# Legacy System GraphQL Integration Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a legacy system integration expert. Provide guidance for integrating GraphQL with existing legacy systems.");
        prompt.AppendLine();
        prompt.AppendLine($"**Legacy System Type:** {legacyType}");
        prompt.AppendLine($"**Integration Approach:** {approach}");
        prompt.AppendLine($"**Complexity Level:** {complexity}");
        prompt.AppendLine($"**Timeline Constraints:** {timeline}");
        prompt.AppendLine();

        switch (legacyType.ToLower())
        {
            case "database":
                prompt.AppendLine("## Database Integration:");
                prompt.AppendLine("### Direct Database Access:");
                prompt.AppendLine("- Schema-to-GraphQL type mapping");
                prompt.AppendLine("- Query optimization for GraphQL patterns");
                prompt.AppendLine("- Connection pooling and performance");
                prompt.AppendLine("- Transaction management strategies");
                prompt.AppendLine();
                prompt.AppendLine("### Data Layer Patterns:");
                prompt.AppendLine("- ORM integration and configuration");
                prompt.AppendLine("- Custom data source implementations");
                prompt.AppendLine("- Caching strategies for database queries");
                prompt.AppendLine("- Migration and schema evolution");
                break;

            case "rest-api":
                prompt.AppendLine("## REST API Integration:");
                prompt.AppendLine("### API Wrapping Strategies:");
                prompt.AppendLine("- REST endpoint to GraphQL field mapping");
                prompt.AppendLine("- Data fetching optimization");
                prompt.AppendLine("- Error handling and transformation");
                prompt.AppendLine("- Authentication and authorization bridging");
                prompt.AppendLine();
                prompt.AppendLine("### Performance Considerations:");
                prompt.AppendLine("- Request batching and deduplication");
                prompt.AppendLine("- Response caching strategies");
                prompt.AppendLine("- Timeout and retry mechanisms");
                prompt.AppendLine("- Circuit breaker patterns");
                break;

            case "soap":
                prompt.AppendLine("## SOAP Service Integration:");
                prompt.AppendLine("### SOAP-to-GraphQL Mapping:");
                prompt.AppendLine("- WSDL analysis and schema generation");
                prompt.AppendLine("- Complex type transformation");
                prompt.AppendLine("- Fault handling and error mapping");
                prompt.AppendLine("- Security header and authentication");
                prompt.AppendLine();
                prompt.AppendLine("### Protocol Bridge:");
                prompt.AppendLine("- HTTP/SOAP protocol adaptation");
                prompt.AppendLine("- XML-to-JSON transformation");
                prompt.AppendLine("- Namespace and schema management");
                break;

            case "mainframe":
                prompt.AppendLine("## Mainframe Integration:");
                prompt.AppendLine("### Connectivity Patterns:");
                prompt.AppendLine("- Mainframe connector configuration");
                prompt.AppendLine("- Data format transformation (EBCDIC/ASCII)");
                prompt.AppendLine("- Transaction processing integration");
                prompt.AppendLine("- Connection pooling and management");
                prompt.AppendLine();
                prompt.AppendLine("### Performance Optimization:");
                prompt.AppendLine("- Batch operation strategies");
                prompt.AppendLine("- Connection reuse patterns");
                prompt.AppendLine("- Data caching for frequent queries");
                break;
        }

        switch (approach.ToLower())
        {
            case "wrapper":
                prompt.AppendLine();
                prompt.AppendLine("## Wrapper Approach:");
                prompt.AppendLine("- Minimal changes to existing systems");
                prompt.AppendLine("- GraphQL layer as API gateway");
                prompt.AppendLine("- Rapid implementation and deployment");
                prompt.AppendLine("- Legacy system preservation");
                break;

            case "federation":
                prompt.AppendLine();
                prompt.AppendLine("## Federation Approach:");
                prompt.AppendLine("- Gradual service decomposition");
                prompt.AppendLine("- Schema federation across systems");
                prompt.AppendLine("- Independent service evolution");
                prompt.AppendLine("- Distributed ownership model");
                break;

            case "migration":
                prompt.AppendLine();
                prompt.AppendLine("## Migration Approach:");
                prompt.AppendLine("- Phased legacy system replacement");
                prompt.AppendLine("- Data migration strategies");
                prompt.AppendLine("- Parallel system operation");
                prompt.AppendLine("- Risk mitigation and rollback plans");
                break;
        }

        switch (timeline.ToLower())
        {
            case "tight":
                prompt.AppendLine();
                prompt.AppendLine("## Tight Timeline Strategy:");
                prompt.AppendLine("- Focus on wrapper approach for quick wins");
                prompt.AppendLine("- Automate integration code generation");
                prompt.AppendLine("- Prioritize core functionality first");
                prompt.AppendLine("- Plan for iterative improvements");
                break;

            case "flexible":
                prompt.AppendLine();
                prompt.AppendLine("## Flexible Timeline Strategy:");
                prompt.AppendLine("- Consider comprehensive migration approach");
                prompt.AppendLine("- Invest in proper architecture and design");
                prompt.AppendLine("- Include extensive testing and validation");
                prompt.AppendLine("- Plan for long-term maintainability");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Integration Tools:");
        prompt.AppendLine("1. **ExtractGraphQLFromCSharpCode** - Analyze existing code patterns");
        prompt.AppendLine("2. **GenerateOptimizedGraphQLQueries** - Create GraphQL equivalents");
        prompt.AppendLine("3. **IntrospectSchema** - Analyze target GraphQL structure");
        prompt.AppendLine("4. **CreateTestSuite** - Validate integration scenarios");

        return prompt.ToString();
    }
}