import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { 
  CallToolRequestSchema,
  ListToolsRequestSchema 
} from '@modelcontextprotocol/sdk/types.js';
import dotenv from 'dotenv';

// Import all tool modules
import { QueryGraphQLTool } from './tools/QueryGraphQLTool.js';
import { SchemaIntrospectionTools } from './tools/SchemaIntrospectionTools.js';
import { QueryValidationTools } from './tools/QueryValidationTools.js';
import { QueryAnalyzerTools } from './tools/QueryAnalyzerTools.js';
import { SecurityAnalysisTools } from './tools/SecurityAnalysisTools.js';
import { PerformanceMonitoringTools } from './tools/PerformanceMonitoringTools.js';
import { FieldUsageAnalyticsTools } from './tools/FieldUsageAnalyticsTools.js';
import { UtilityTools } from './tools/UtilityTools.js';
import { DynamicToolRegistry } from './tools/DynamicToolRegistry.js';

// Load environment variables
dotenv.config();

export interface McpServerConfig {
  endpoint: string;
  headers: Record<string, string>;
  allowMutations: boolean;
  name: string;
  schemaPath?: string;
}

class GraphQLMcpServer {
  private server: Server;
  private config: McpServerConfig;
  private tools: Map<string, any> = new Map();

  constructor() {
    this.config = this.loadConfig();
    this.server = new Server(
      {
        name: this.config.name,
        version: '1.0.0',
      },
      {
        capabilities: {
          tools: {},
        },
      }
    );

    this.initializeTools();
    this.setupHandlers();
  }

  private loadConfig(): McpServerConfig {
    const headersJson = process.env.HEADERS || '{}';
    let headers: Record<string, string>;
    
    try {
      headers = JSON.parse(headersJson);
    } catch {
      headers = {};
    }

    return {
      endpoint: process.env.ENDPOINT || 'http://localhost:4000/graphql',
      headers,
      allowMutations: (process.env.ALLOW_MUTATIONS || 'false').toLowerCase() === 'true',
      name: process.env.NAME || 'mcp-graphql',
      schemaPath: process.env.SCHEMA,
    };
  }

  private initializeTools() {
    // Initialize all tool instances
    const queryTool = new QueryGraphQLTool(
      this.config.endpoint,
      this.config.headers,
      this.config.allowMutations
    );

    const schemaTools = new SchemaIntrospectionTools();
    const validationTools = new QueryValidationTools();
    const analyzerTools = new QueryAnalyzerTools();
    const securityTools = new SecurityAnalysisTools();
    const performanceTools = new PerformanceMonitoringTools();
    const analyticsTools = new FieldUsageAnalyticsTools();
    const utilityTools = new UtilityTools();
    const dynamicRegistry = new DynamicToolRegistry();

    // Register tools
    this.registerToolsFromInstance('query', queryTool);
    this.registerToolsFromInstance('schema', schemaTools);
    this.registerToolsFromInstance('validation', validationTools);
    this.registerToolsFromInstance('analyzer', analyzerTools);
    this.registerToolsFromInstance('security', securityTools);
    this.registerToolsFromInstance('performance', performanceTools);
    this.registerToolsFromInstance('analytics', analyticsTools);
    this.registerToolsFromInstance('utility', utilityTools);
    this.registerToolsFromInstance('dynamic', dynamicRegistry);
  }

  private registerToolsFromInstance(category: string, instance: any) {
    // Use reflection to find all methods marked as tools
    const methods = Object.getOwnPropertyNames(Object.getPrototypeOf(instance))
      .filter(method => method !== 'constructor');

    methods.forEach(method => {
      if (typeof instance[method] === 'function') {
        const toolName = `${category}_${method}`;
        this.tools.set(toolName, {
          instance,
          method,
          description: this.getToolDescription(instance, method),
          inputSchema: this.getToolInputSchema(instance, method)
        });
      }
    });
  }

  private getToolDescription(instance: any, method: string): string {
    // In real implementation, you'd use decorators or metadata
    // For now, return a default description
    return `${method} tool from ${instance.constructor.name}`;
  }

  private getToolInputSchema(instance: any, method: string): any {
    // In real implementation, you'd extract schema from decorators
    // For now, return a basic schema
    return {
      type: 'object',
      properties: {},
    };
  }

  private setupHandlers() {
    this.server.setRequestHandler(ListToolsRequestSchema, async () => {
      const tools = Array.from(this.tools.entries()).map(([name, tool]) => ({
        name,
        description: tool.description,
        inputSchema: tool.inputSchema,
      }));

      return { tools };
    });

    this.server.setRequestHandler(CallToolRequestSchema, async (request) => {
      const { name, arguments: args } = request.params;
      
      const tool = this.tools.get(name);
      if (!tool) {
        throw new Error(`Tool ${name} not found`);
      }

      try {
        const result = await tool.instance[tool.method](args);
        return {
          content: [
            {
              type: 'text',
              text: typeof result === 'string' ? result : JSON.stringify(result, null, 2),
            },
          ],
        };
      } catch (error) {
        throw new Error(`Tool execution failed: ${error}`);
      }
    });
  }

  async run() {
    const transport = new StdioServerTransport();
    await this.server.connect(transport);
    console.error(`${this.config.name} server running on stdio`);
  }
}

// Start the server
async function main() {
  try {
    const server = new GraphQLMcpServer();
    await server.run();
  } catch (error) {
    console.error('Failed to start GraphQL MCP server:', error);
    process.exit(1);
  }
}

if (require.main === module) {
  main().catch(console.error);
}

export { GraphQLMcpServer };
