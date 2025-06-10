import { SchemaIntrospectionTools } from './SchemaIntrospectionTools.js';

export interface DynamicToolInfo {
  toolName: string;
  endpointName: string;
  operationType: string;
  operationName: string;
  operation: string;
  description: string;
  field: any;
}

export interface GraphQLEndpointInfo {
  name: string;
  url: string;
  headers: Record<string, string>;
  allowMutations: boolean;
  toolPrefix: string;
}

export class DynamicToolRegistry {
  private static dynamicTools = new Map<string, DynamicToolInfo>();
  private static endpoints = new Map<string, GraphQLEndpointInfo>();
  private schemaTools = new SchemaIntrospectionTools();

  /**
   * Register a GraphQL endpoint for automatic tool generation
   */
  async registerEndpoint(params: {
    endpoint: string;
    endpointName: string;
    headers?: string;
    allowMutations?: boolean;
    toolPrefix?: string;
  }): Promise<string> {
    const { endpoint, endpointName, headers, allowMutations = false, toolPrefix = '' } = params;

    try {
      // Parse headers
      let headerDict: Record<string, string> = {};
      if (headers) {
        try {
          headerDict = JSON.parse(headers);
        } catch (error) {
          return `Error parsing headers JSON: ${error}`;
        }
      }

      // Store endpoint info
      const endpointInfo: GraphQLEndpointInfo = {
        name: endpointName,
        url: endpoint,
        headers: headerDict,
        allowMutations,
        toolPrefix,
      };

      DynamicToolRegistry.endpoints.set(endpointName, endpointInfo);

      // Generate tools from schema
      const result = await this.generateToolsFromSchema(endpointInfo);
      return result;

    } catch (error) {
      return `Error registering endpoint: ${error}`;
    }
  }

  /**
   * List all registered dynamic tools
   */
  listDynamicTools(): string {
    const result: string[] = [];

    if (DynamicToolRegistry.dynamicTools.size === 0) {
      return 'No dynamic tools registered. Use registerEndpoint to add GraphQL endpoints.';
    }

    result.push('# Registered Dynamic Tools\n');

    // Group by endpoint
    const toolsByEndpoint = new Map<string, DynamicToolInfo[]>();
    
    DynamicToolRegistry.dynamicTools.forEach(tool => {
      if (!toolsByEndpoint.has(tool.endpointName)) {
        toolsByEndpoint.set(tool.endpointName, []);
      }
      toolsByEndpoint.get(tool.endpointName)!.push(tool);
    });

    toolsByEndpoint.forEach((tools, endpointName) => {
      const endpoint = DynamicToolRegistry.endpoints.get(endpointName);
      result.push(`## ${endpointName} (${endpoint?.url})`);
      result.push(`**Tools:** ${tools.length}`);
      result.push('');

      const queries = tools.filter(t => t.operationType === 'query');
      const mutations = tools.filter(t => t.operationType === 'mutation');

      if (queries.length > 0) {
        result.push('### Queries');
        queries.forEach(query => {
          result.push(`- **${query.toolName}**: ${query.description}`);
        });
        result.push('');
      }

      if (mutations.length > 0) {
        result.push('### Mutations');
        mutations.forEach(mutation => {
          result.push(`- **${mutation.toolName}**: ${mutation.description}`);
        });
        result.push('');
      }
    });

    return result.join('\n');
  }

  /**
   * Execute a dynamically generated GraphQL operation
   */
  async executeDynamicOperation(params: {
    toolName: string;
    variables?: string;
  }): Promise<string> {
    const { toolName, variables } = params;

    try {
      const tool = DynamicToolRegistry.dynamicTools.get(toolName);
      if (!tool) {
        return `Dynamic tool '${toolName}' not found. Use listDynamicTools to see available tools.`;
      }

      const endpointInfo = DynamicToolRegistry.endpoints.get(tool.endpointName);
      if (!endpointInfo) {
        return `Endpoint '${tool.endpointName}' not found.`;
      }

      // Parse variables
      let variableDict: Record<string, any> = {};
      if (variables) {
        try {
          variableDict = JSON.parse(variables);
        } catch (error) {
          return `Error parsing variables JSON: ${error}`;
        }
      }

      // Execute the operation using QueryGraphQLTool
      const { QueryGraphQLTool } = await import('./QueryGraphQLTool.js');
      const queryTool = new QueryGraphQLTool(
        endpointInfo.url,
        endpointInfo.headers,
        endpointInfo.allowMutations
      );

      const result = await queryTool.queryGraphql({
        query: tool.operation,
        variables: Object.keys(variableDict).length > 0 ? JSON.stringify(variableDict) : undefined,
      });

      return this.formatGraphQLResponse(result);

    } catch (error) {
      return `Error executing dynamic operation: ${error}`;
    }
  }

  /**
   * Refresh tools for a registered endpoint (re-introspect schema)
   */
  async refreshEndpointTools(params: { endpointName: string }): Promise<string> {
    const { endpointName } = params;

    try {
      const endpointInfo = DynamicToolRegistry.endpoints.get(endpointName);
      if (!endpointInfo) {
        return `Endpoint '${endpointName}' not found.`;
      }

      // Remove existing tools for this endpoint
      const keysToRemove: string[] = [];
      DynamicToolRegistry.dynamicTools.forEach((tool, key) => {
        if (tool.endpointName === endpointName) {
          keysToRemove.push(key);
        }
      });

      keysToRemove.forEach(key => {
        DynamicToolRegistry.dynamicTools.delete(key);
      });

      // Regenerate tools
      const result = await this.generateToolsFromSchema(endpointInfo);
      return `Refreshed endpoint '${endpointName}'. ${result}`;

    } catch (error) {
      return `Error refreshing endpoint tools: ${error}`;
    }
  }

  /**
   * Remove all dynamic tools for an endpoint
   */
  unregisterEndpoint(params: { endpointName: string }): string {
    const { endpointName } = params;

    try {
      const endpointInfo = DynamicToolRegistry.endpoints.get(endpointName);
      if (!endpointInfo) {
        return `Endpoint '${endpointName}' not found.`;
      }

      // Remove all tools for this endpoint
      const keysToRemove: string[] = [];
      DynamicToolRegistry.dynamicTools.forEach((tool, key) => {
        if (tool.endpointName === endpointName) {
          keysToRemove.push(key);
        }
      });

      keysToRemove.forEach(key => {
        DynamicToolRegistry.dynamicTools.delete(key);
      });

      // Remove endpoint
      DynamicToolRegistry.endpoints.delete(endpointName);

      return `Unregistered endpoint '${endpointName}' and removed ${keysToRemove.length} dynamic tools.`;

    } catch (error) {
      return `Error unregistering endpoint: ${error}`;
    }
  }

  private async generateToolsFromSchema(endpointInfo: GraphQLEndpointInfo): Promise<string> {
    try {
      // Introspect the schema
      const headersJson = Object.keys(endpointInfo.headers).length > 0 
        ? JSON.stringify(endpointInfo.headers)
        : undefined;

      const schemaJson = await this.schemaTools.introspectSchema({
        endpoint: endpointInfo.url,
        headers: headersJson,
      });

      const schemaData = JSON.parse(schemaJson);

      if (!schemaData.data || !schemaData.data.__schema) {
        return 'Failed to parse schema introspection data';
      }

      const schema = schemaData.data.__schema;
      let toolsGenerated = 0;

      // Find Query type
      if (schema.queryType?.name) {
        const queryType = this.findTypeByName(schema, schema.queryType.name);
        if (queryType) {
          toolsGenerated += this.generateToolsForType(queryType, 'query', endpointInfo);
        }
      }

      // Find Mutation type (if allowed)
      if (endpointInfo.allowMutations && schema.mutationType?.name) {
        const mutationType = this.findTypeByName(schema, schema.mutationType.name);
        if (mutationType) {
          toolsGenerated += this.generateToolsForType(mutationType, 'mutation', endpointInfo);
        }
      }

      let result = `Generated ${toolsGenerated} dynamic tools for endpoint '${endpointInfo.name}'`;
      
      if (!endpointInfo.allowMutations) {
        result += '\nNote: Mutations were not enabled for this endpoint';
      }

      return result;

    } catch (error) {
      return `Error generating tools from schema: ${error}`;
    }
  }

  private findTypeByName(schema: any, typeName: string): any {
    return schema.types?.find((type: any) => type.name === typeName);
  }

  private generateToolsForType(type: any, operationType: string, endpointInfo: GraphQLEndpointInfo): number {
    let toolsGenerated = 0;

    if (type.fields) {
      type.fields.forEach((field: any) => {
        const toolName = this.generateToolName(endpointInfo.toolPrefix, operationType, field.name);
        const operation = this.generateOperationString(field, operationType, field.name);
        const description = this.getFieldDescription(field, operationType, field.name);

        const toolInfo: DynamicToolInfo = {
          toolName,
          endpointName: endpointInfo.name,
          operationType,
          operationName: `${operationType}_${field.name}`,
          operation,
          description,
          field,
        };

        DynamicToolRegistry.dynamicTools.set(toolName, toolInfo);
        toolsGenerated++;
      });
    }

    return toolsGenerated;
  }

  private generateToolName(prefix: string, operationType: string, fieldName: string): string {
    const parts = [prefix, operationType, this.toCamelCase(fieldName)].filter(Boolean);
    return parts.join('_');
  }

  private toCamelCase(input: string): string {
    return input.replace(/_([a-z])/g, (_, letter) => letter.toUpperCase());
  }

  private generateOperationString(field: any, operationType: string, fieldName: string): string {
    const lines: string[] = [];
    
    lines.push(`${operationType.toLowerCase()} ${operationType}_${fieldName}(`);
    
    // Add parameters
    if (field.args && field.args.length > 0) {
      const parameters = field.args.map((arg: any) => {
        const paramName = arg.name;
        const paramType = this.getGraphQLTypeName(arg.type);
        return `$${paramName}: ${paramType}`;
      });
      
      if (parameters.length > 0) {
        lines.push(parameters.join(',\n  '));
      }
    }
    
    lines.push(') {');
    lines.push(`  ${fieldName}`);
    
    // Add arguments if any
    if (field.args && field.args.length > 0) {
      const argList = field.args.map((arg: any) => `${arg.name}: $${arg.name}`);
      if (argList.length > 0) {
        lines[lines.length - 1] += `(${argList.join(', ')})`;
      }
    }
    
    // Add basic field selection
    lines.push('    # Add your field selections here');
    lines.push('    # This is a template - customize the fields you need');
    lines.push('  }');
    lines.push('}');
    
    return lines.join('\n');
  }

  private getFieldDescription(field: any, operationType: string, fieldName: string): string {
    if (field.description) {
      return field.description;
    }
    
    let desc = `Execute ${operationType.toLowerCase()} operation: ${fieldName}`;
    
    if (field.args && field.args.length > 0) {
      desc += ` (requires ${field.args.length} parameter${field.args.length === 1 ? '' : 's'})`;
    }
    
    return desc;
  }

  private getGraphQLTypeName(type: any): string {
    if (type.kind === 'NON_NULL') {
      return `${this.getGraphQLTypeName(type.ofType)}!`;
    }
    
    if (type.kind === 'LIST') {
      return `[${this.getGraphQLTypeName(type.ofType)}]`;
    }
    
    return type.name || 'Unknown';
  }

  private formatGraphQLResponse(responseContent: string): string {
    try {
      const parsed = JSON.parse(responseContent);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return responseContent;
    }
  }
}
