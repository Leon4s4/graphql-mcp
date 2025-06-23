using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Prompts;

[McpServerPromptType]
public class DomainSpecificPrompts
{
    [McpServerPrompt, Description("E-commerce specific GraphQL patterns and best practices")]
    public static string ECommerceGraphQL(
        [Description("E-commerce focus: catalog, orders, payments, inventory, search")]
        string focus = "catalog",
        [Description("Scale: startup, medium, enterprise")]
        string scale = "medium",
        [Description("Integration requirements: payment, shipping, analytics")]
        string? integrations = null)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# E-commerce GraphQL Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL expert specializing in e-commerce platforms. Provide domain-specific guidance for e-commerce GraphQL implementation.");
        prompt.AppendLine();
        prompt.AppendLine($"**E-commerce Focus:** {focus}");
        prompt.AppendLine($"**Scale:** {scale}");
        if (!string.IsNullOrEmpty(integrations))
            prompt.AppendLine($"**Integrations:** {integrations}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "catalog":
                prompt.AppendLine("## Product Catalog Design:");
                prompt.AppendLine("### Schema Patterns:");
                prompt.AppendLine("- Product hierarchy and categorization");
                prompt.AppendLine("- Variant management (size, color, configuration)");
                prompt.AppendLine("- Inventory tracking and availability");
                prompt.AppendLine("- Pricing and promotional structures");
                prompt.AppendLine("- Product search and filtering capabilities");
                prompt.AppendLine();
                prompt.AppendLine("### Common Queries:");
                prompt.AppendLine("- Product listing with pagination and filters");
                prompt.AppendLine("- Product detail with variants and inventory");
                prompt.AppendLine("- Category browsing and navigation");
                prompt.AppendLine("- Search with faceted results");
                prompt.AppendLine("- Related and recommended products");
                break;

            case "orders":
                prompt.AppendLine("## Order Management Design:");
                prompt.AppendLine("### Schema Patterns:");
                prompt.AppendLine("- Order lifecycle and status tracking");
                prompt.AppendLine("- Shopping cart and checkout process");
                prompt.AppendLine("- Payment and shipping information");
                prompt.AppendLine("- Order history and customer accounts");
                prompt.AppendLine("- Returns and refund processing");
                prompt.AppendLine();
                prompt.AppendLine("### Common Operations:");
                prompt.AppendLine("- Cart management (add, update, remove items)");
                prompt.AppendLine("- Checkout process with validation");
                prompt.AppendLine("- Order placement and confirmation");
                prompt.AppendLine("- Order tracking and status updates");
                prompt.AppendLine("- Order history and reordering");
                break;

            case "payments":
                prompt.AppendLine("## Payment Integration Design:");
                prompt.AppendLine("### Security Considerations:");
                prompt.AppendLine("- PCI DSS compliance requirements");
                prompt.AppendLine("- Tokenization and secure payment processing");
                prompt.AppendLine("- Payment method validation");
                prompt.AppendLine("- Transaction logging and audit trails");
                prompt.AppendLine();
                prompt.AppendLine("### Integration Patterns:");
                prompt.AppendLine("- Payment gateway abstraction layers");
                prompt.AppendLine("- Multiple payment method support");
                prompt.AppendLine("- Subscription and recurring payments");
                prompt.AppendLine("- Refund and chargeback handling");
                break;

            case "inventory":
                prompt.AppendLine("## Inventory Management Design:");
                prompt.AppendLine("### Real-time Updates:");
                prompt.AppendLine("- Stock level synchronization");
                prompt.AppendLine("- Reservation and allocation systems");
                prompt.AppendLine("- Multi-warehouse inventory tracking");
                prompt.AppendLine("- Backorder and pre-order management");
                prompt.AppendLine();
                prompt.AppendLine("### Performance Considerations:");
                prompt.AppendLine("- Caching strategies for inventory data");
                prompt.AppendLine("- Event-driven inventory updates");
                prompt.AppendLine("- Inventory projection and forecasting");
                break;

            case "search":
                prompt.AppendLine("## Search and Discovery Design:");
                prompt.AppendLine("### Search Features:");
                prompt.AppendLine("- Full-text product search");
                prompt.AppendLine("- Faceted search and filtering");
                prompt.AppendLine("- Auto-complete and suggestions");
                prompt.AppendLine("- Visual search and image recognition");
                prompt.AppendLine();
                prompt.AppendLine("### Performance Optimization:");
                prompt.AppendLine("- Search result caching strategies");
                prompt.AppendLine("- Elasticsearch integration patterns");
                prompt.AppendLine("- Search analytics and optimization");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## E-commerce Specific Tools:");
        prompt.AppendLine("1. **IntrospectSchema** - Analyze e-commerce schema structure");
        prompt.AppendLine("2. **BuildQuery** - Generate e-commerce specific queries");
        prompt.AppendLine("3. **GenerateTestData** - Create realistic e-commerce test data");
        prompt.AppendLine("4. **AnalyzeQuery** - Optimize e-commerce query performance");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Social media platform GraphQL design patterns")]
    public static string SocialMediaGraphQL(
        [Description("Social media focus: feeds, messaging, social, content, notifications")]
        string focus = "feeds",
        [Description("User scale: thousands, millions, billions")]
        string userScale = "millions",
        [Description("Real-time requirements: basic, moderate, extensive")]
        string realTime = "moderate")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# Social Media GraphQL Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL expert specializing in social media platforms. Provide guidance for social networking GraphQL APIs.");
        prompt.AppendLine();
        prompt.AppendLine($"**Social Media Focus:** {focus}");
        prompt.AppendLine($"**User Scale:** {userScale}");
        prompt.AppendLine($"**Real-time Requirements:** {realTime}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "feeds":
                prompt.AppendLine("## Activity Feed Design:");
                prompt.AppendLine("### Schema Patterns:");
                prompt.AppendLine("- Post types and content structures");
                prompt.AppendLine("- User timeline and feed algorithms");
                prompt.AppendLine("- Engagement metrics (likes, shares, comments)");
                prompt.AppendLine("- Content moderation and filtering");
                prompt.AppendLine("- Personalization and recommendation engines");
                prompt.AppendLine();
                prompt.AppendLine("### Performance Considerations:");
                prompt.AppendLine("- Feed generation and caching strategies");
                prompt.AppendLine("- Pagination for infinite scroll");
                prompt.AppendLine("- Real-time feed updates");
                prompt.AppendLine("- Content preloading and optimization");
                break;

            case "messaging":
                prompt.AppendLine("## Messaging System Design:");
                prompt.AppendLine("### Schema Patterns:");
                prompt.AppendLine("- Conversation and thread management");
                prompt.AppendLine("- Message types (text, media, reactions)");
                prompt.AppendLine("- Read receipts and delivery status");
                prompt.AppendLine("- Group messaging and channels");
                prompt.AppendLine("- Message encryption and privacy");
                prompt.AppendLine();
                prompt.AppendLine("### Real-time Features:");
                prompt.AppendLine("- GraphQL subscriptions for live messaging");
                prompt.AppendLine("- Typing indicators and presence");
                prompt.AppendLine("- Message synchronization across devices");
                break;

            case "social":
                prompt.AppendLine("## Social Graph Design:");
                prompt.AppendLine("### Relationship Management:");
                prompt.AppendLine("- Friend/follower connection models");
                prompt.AppendLine("- Privacy settings and visibility controls");
                prompt.AppendLine("- Social graph traversal and discovery");
                prompt.AppendLine("- Community and group management");
                prompt.AppendLine();
                prompt.AppendLine("### Social Features:");
                prompt.AppendLine("- User profiles and social signals");
                prompt.AppendLine("- Social recommendations and suggestions");
                prompt.AppendLine("- Activity tracking and social proof");
                break;

            case "content":
                prompt.AppendLine("## Content Management Design:");
                prompt.AppendLine("### Content Types:");
                prompt.AppendLine("- Rich media support (images, videos, stories)");
                prompt.AppendLine("- Content versioning and editing");
                prompt.AppendLine("- Content discovery and search");
                prompt.AppendLine("- Content moderation workflows");
                prompt.AppendLine();
                prompt.AppendLine("### Content Distribution:");
                prompt.AppendLine("- CDN integration for media delivery");
                prompt.AppendLine("- Content recommendation algorithms");
                prompt.AppendLine("- Viral content propagation patterns");
                break;

            case "notifications":
                prompt.AppendLine("## Notification System Design:");
                prompt.AppendLine("### Notification Types:");
                prompt.AppendLine("- Activity notifications (likes, comments, mentions)");
                prompt.AppendLine("- System notifications (updates, maintenance)");
                prompt.AppendLine("- Marketing and promotional notifications");
                prompt.AppendLine("- Real-time alerts and emergency broadcasts");
                prompt.AppendLine();
                prompt.AppendLine("### Delivery Mechanisms:");
                prompt.AppendLine("- Push notification integration");
                prompt.AppendLine("- In-app notification management");
                prompt.AppendLine("- Email and SMS fallback systems");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Social Media Specific Tools:");
        prompt.AppendLine("1. **GraphqlServiceManager** - Manage social media endpoints");
        prompt.AppendLine("2. **ExecuteQuery** - Test social media query performance");
        prompt.AppendLine("3. **CreateTestSuite** - Generate social media test scenarios");
        prompt.AppendLine("4. **DebugGraphQL** - Debug social media specific issues");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("IoT and real-time data GraphQL patterns")]
    public static string IoTGraphQL(
        [Description("IoT focus: devices, telemetry, control, analytics")]
        string focus = "telemetry",
        [Description("Device scale: hundreds, thousands, millions")]
        string deviceScale = "thousands",
        [Description("Data frequency: seconds, minutes, hours")]
        string dataFrequency = "minutes")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# IoT GraphQL Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL expert specializing in IoT and real-time data systems. Provide guidance for IoT GraphQL implementations.");
        prompt.AppendLine();
        prompt.AppendLine($"**IoT Focus:** {focus}");
        prompt.AppendLine($"**Device Scale:** {deviceScale}");
        prompt.AppendLine($"**Data Frequency:** {dataFrequency}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "devices":
                prompt.AppendLine("## Device Management Design:");
                prompt.AppendLine("### Device Schema Patterns:");
                prompt.AppendLine("- Device registration and provisioning");
                prompt.AppendLine("- Device hierarchy and grouping");
                prompt.AppendLine("- Device capabilities and configuration");
                prompt.AppendLine("- Firmware and software management");
                prompt.AppendLine("- Device lifecycle and maintenance");
                prompt.AppendLine();
                prompt.AppendLine("### Device Operations:");
                prompt.AppendLine("- Device discovery and onboarding");
                prompt.AppendLine("- Configuration updates and deployments");
                prompt.AppendLine("- Health monitoring and diagnostics");
                prompt.AppendLine("- Remote troubleshooting and support");
                break;

            case "telemetry":
                prompt.AppendLine("## Telemetry Data Design:");
                prompt.AppendLine("### Data Patterns:");
                prompt.AppendLine("- Time-series data structures");
                prompt.AppendLine("- Sensor data aggregation and rollups");
                prompt.AppendLine("- Real-time streaming data ingestion");
                prompt.AppendLine("- Historical data querying and analysis");
                prompt.AppendLine();
                prompt.AppendLine("### Performance Considerations:");
                prompt.AppendLine("- Efficient data pagination for time-series");
                prompt.AppendLine("- Data compression and storage optimization");
                prompt.AppendLine("- Real-time subscriptions for live data");
                prompt.AppendLine("- Caching strategies for aggregated data");
                break;

            case "control":
                prompt.AppendLine("## Device Control Design:");
                prompt.AppendLine("### Control Patterns:");
                prompt.AppendLine("- Command and control interfaces");
                prompt.AppendLine("- Device state management");
                prompt.AppendLine("- Batch operations and scheduling");
                prompt.AppendLine("- Safety and security controls");
                prompt.AppendLine();
                prompt.AppendLine("### Real-time Features:");
                prompt.AppendLine("- Immediate command execution");
                prompt.AppendLine("- Command status and feedback");
                prompt.AppendLine("- Emergency stop and safety protocols");
                break;

            case "analytics":
                prompt.AppendLine("## Analytics and Insights Design:");
                prompt.AppendLine("### Analytics Patterns:");
                prompt.AppendLine("- Real-time analytics and dashboards");
                prompt.AppendLine("- Predictive maintenance algorithms");
                prompt.AppendLine("- Anomaly detection and alerting");
                prompt.AppendLine("- Performance optimization insights");
                prompt.AppendLine();
                prompt.AppendLine("### Data Processing:");
                prompt.AppendLine("- Stream processing integration");
                prompt.AppendLine("- Machine learning model integration");
                prompt.AppendLine("- Complex event processing");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## IoT Specific Tools:");
        prompt.AppendLine("1. **IntrospectSchema** - Analyze IoT data schemas");
        prompt.AppendLine("2. **ExecuteQuery** - Test real-time data queries");
        prompt.AppendLine("3. **AnalyzeQuery** - Optimize time-series query performance");
        prompt.AppendLine("4. **GenerateTestData** - Create realistic IoT test data");

        return prompt.ToString();
    }

    [McpServerPrompt, Description("Financial services GraphQL compliance and patterns")]
    public static string FinanceGraphQL(
        [Description("Finance focus: trading, banking, payments, compliance")]
        string focus = "banking",
        [Description("Compliance requirements: PCI, SOX, GDPR, Basel")]
        string compliance = "PCI",
        [Description("Security level: standard, enhanced, maximum")]
        string securityLevel = "enhanced")
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("# Financial Services GraphQL Assistant");
        prompt.AppendLine();
        prompt.AppendLine("You are a GraphQL expert specializing in financial services. Provide guidance for compliant and secure financial GraphQL APIs.");
        prompt.AppendLine();
        prompt.AppendLine($"**Finance Focus:** {focus}");
        prompt.AppendLine($"**Compliance Requirements:** {compliance}");
        prompt.AppendLine($"**Security Level:** {securityLevel}");
        prompt.AppendLine();

        switch (focus.ToLower())
        {
            case "banking":
                prompt.AppendLine("## Banking System Design:");
                prompt.AppendLine("### Account Management:");
                prompt.AppendLine("- Account hierarchy and relationships");
                prompt.AppendLine("- Balance inquiries and transaction history");
                prompt.AppendLine("- Multi-currency support and conversion");
                prompt.AppendLine("- Account security and access controls");
                prompt.AppendLine();
                prompt.AppendLine("### Transaction Processing:");
                prompt.AppendLine("- Real-time transaction authorization");
                prompt.AppendLine("- Transaction categorization and tagging");
                prompt.AppendLine("- Fraud detection and prevention");
                prompt.AppendLine("- Reconciliation and settlement processes");
                break;

            case "trading":
                prompt.AppendLine("## Trading Platform Design:");
                prompt.AppendLine("### Market Data:");
                prompt.AppendLine("- Real-time price feeds and quotes");
                prompt.AppendLine("- Historical market data and analytics");
                prompt.AppendLine("- Order book and market depth");
                prompt.AppendLine("- Trading volume and liquidity metrics");
                prompt.AppendLine();
                prompt.AppendLine("### Order Management:");
                prompt.AppendLine("- Order placement and execution");
                prompt.AppendLine("- Portfolio management and tracking");
                prompt.AppendLine("- Risk management and position limits");
                prompt.AppendLine("- Trade reporting and compliance");
                break;

            case "payments":
                prompt.AppendLine("## Payment System Design:");
                prompt.AppendLine("### Payment Processing:");
                prompt.AppendLine("- Multi-payment method support");
                prompt.AppendLine("- Cross-border payment handling");
                prompt.AppendLine("- Payment routing and optimization");
                prompt.AppendLine("- Settlement and clearing processes");
                prompt.AppendLine();
                prompt.AppendLine("### Security Requirements:");
                prompt.AppendLine("- PCI DSS compliance implementation");
                prompt.AppendLine("- Tokenization and encryption");
                prompt.AppendLine("- Anti-money laundering (AML) checks");
                prompt.AppendLine("- Know Your Customer (KYC) integration");
                break;

            case "compliance":
                prompt.AppendLine("## Compliance Framework Design:");
                prompt.AppendLine("### Regulatory Reporting:");
                prompt.AppendLine("- Automated compliance reporting");
                prompt.AppendLine("- Audit trail and documentation");
                prompt.AppendLine("- Regulatory change management");
                prompt.AppendLine("- Cross-jurisdictional compliance");
                prompt.AppendLine();
                prompt.AppendLine("### Data Governance:");
                prompt.AppendLine("- Data retention and archival policies");
                prompt.AppendLine("- Data privacy and protection");
                prompt.AppendLine("- Data quality and integrity controls");
                break;
        }

        switch (compliance.ToUpper())
        {
            case "PCI":
                prompt.AppendLine();
                prompt.AppendLine("## PCI DSS Compliance:");
                prompt.AppendLine("- Secure cardholder data handling");
                prompt.AppendLine("- Network security and access controls");
                prompt.AppendLine("- Regular security testing and monitoring");
                prompt.AppendLine("- Vulnerability management programs");
                break;

            case "SOX":
                prompt.AppendLine();
                prompt.AppendLine("## SOX Compliance:");
                prompt.AppendLine("- Internal controls over financial reporting");
                prompt.AppendLine("- Data integrity and accuracy controls");
                prompt.AppendLine("- Change management and approval processes");
                prompt.AppendLine("- Management certification requirements");
                break;
        }

        prompt.AppendLine();
        prompt.AppendLine("## Financial Services Tools:");
        prompt.AppendLine("1. **GraphQLSecurity** - Comprehensive security assessment");
        prompt.AppendLine("2. **AnalyzeQuery** - Security and compliance analysis");
        prompt.AppendLine("3. **CreateTestSuite** - Compliance testing scenarios");
        prompt.AppendLine("4. **DebugGraphQL** - Security issue debugging");

        return prompt.ToString();
    }
}