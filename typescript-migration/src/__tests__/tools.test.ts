import { QueryGraphQLTool } from '../tools/QueryGraphQLTool';
import { SchemaIntrospectionTools } from '../tools/SchemaIntrospectionTools';
import { UtilityTools } from '../tools/UtilityTools';

describe('GraphQL MCP Tools', () => {
  describe('QueryGraphQLTool', () => {
    it('should create instance with proper configuration', () => {
      const tool = new QueryGraphQLTool(
        'http://localhost:4000/graphql',
        { 'Authorization': 'Bearer token' },
        false
      );
      expect(tool).toBeInstanceOf(QueryGraphQLTool);
    });

    it('should detect mutations correctly', () => {
      const tool = new QueryGraphQLTool('http://localhost:4000/graphql');
      
      // Access private method for testing (in real scenario, you'd test through public interface)
      const isMutationMethod = (tool as any).isMutation.bind(tool);
      
      expect(isMutationMethod('mutation { createUser { id } }')).toBe(true);
      expect(isMutationMethod('query { users { id } }')).toBe(false);
      expect(isMutationMethod('{ createUser { id } }')).toBe(true); // Implicit mutation
    });
  });

  describe('SchemaIntrospectionTools', () => {
    it('should create instance', () => {
      const tool = new SchemaIntrospectionTools();
      expect(tool).toBeInstanceOf(SchemaIntrospectionTools);
    });
  });

  describe('UtilityTools', () => {
    it('should format queries correctly', () => {
      const tool = new UtilityTools();
      const query = '{ user(id: 1) { name email } }';
      const formatted = tool.formatQuery({ query });
      
      expect(formatted).toContain('user(id: 1)');
      expect(formatted).toContain('name');
      expect(formatted).toContain('email');
    });

    it('should minify queries correctly', () => {
      const tool = new UtilityTools();
      const query = `
        query GetUser($id: ID!) {
          user(id: $id) {
            name
            email
          }
        }
      `;
      const minified = tool.minifyQuery({ query });
      
      expect(minified).not.toContain('\n');
      expect(minified.length).toBeLessThan(query.length);
      expect(minified).toContain('query GetUser($id:ID!)');
    });

    it('should extract variables from hardcoded values', () => {
      const tool = new UtilityTools();
      const query = '{ user(id: "123", active: true, age: 25) { name } }';
      const result = tool.extractVariables({ query });
      
      expect(result).toContain('var1: String');
      expect(result).toContain('var2: Boolean');
      expect(result).toContain('var3: Int');
      expect(result).toContain('$var1');
      expect(result).toContain('$var2');
      expect(result).toContain('$var3');
    });
  });
});
