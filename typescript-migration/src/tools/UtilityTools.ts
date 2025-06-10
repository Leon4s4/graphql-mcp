export class UtilityTools {
  /**
   * Format and prettify GraphQL queries
   */
  formatQuery(params: { query: string }): string {
    const { query } = params;

    try {
      let formatted = query;
      let indentLevel = 0;
      const lines: string[] = [];
      let currentLine = '';
      let inString = false;
      let stringChar = '';

      for (let i = 0; i < formatted.length; i++) {
        const char = formatted[i];
        const prevChar = i > 0 ? formatted[i - 1] : '';

        // Handle string literals
        if ((char === '"' || char === "'") && prevChar !== '\\') {
          if (!inString) {
            inString = true;
            stringChar = char;
          } else if (char === stringChar) {
            inString = false;
            stringChar = '';
          }
        }

        if (!inString) {
          if (char === '{') {
            currentLine += char;
            lines.push('  '.repeat(indentLevel) + currentLine.trim());
            currentLine = '';
            indentLevel++;
          } else if (char === '}') {
            if (currentLine.trim()) {
              lines.push('  '.repeat(indentLevel) + currentLine.trim());
              currentLine = '';
            }
            indentLevel--;
            lines.push('  '.repeat(indentLevel) + char);
          } else if (char === '\n' || char === '\r') {
            if (currentLine.trim()) {
              lines.push('  '.repeat(indentLevel) + currentLine.trim());
              currentLine = '';
            }
          } else {
            currentLine += char;
          }
        } else {
          currentLine += char;
        }
      }

      if (currentLine.trim()) {
        lines.push('  '.repeat(indentLevel) + currentLine.trim());
      }

      return lines.filter(line => line.trim()).join('\n');

    } catch (error) {
      return `Error formatting query: ${error}`;
    }
  }

  /**
   * Minify GraphQL queries for production use
   */
  minifyQuery(params: { query: string }): string {
    const { query } = params;

    try {
      let result = '';
      let inString = false;
      let stringChar = '';
      let prevChar = '';

      for (const char of query) {
        // Handle string literals
        if ((char === '"' || char === "'") && prevChar !== '\\') {
          if (!inString) {
            inString = true;
            stringChar = char;
          } else if (char === stringChar) {
            inString = false;
            stringChar = '';
          }
        }

        if (inString) {
          result += char;
        } else {
          // Skip whitespace outside of strings
          if (!/\s/.test(char)) {
            result += char;
          } else if (prevChar && !/[{}(),:\[\]=]/.test(prevChar) && !/[{}(),:\[\]=]/.test(char)) {
            // Keep single space between words
            result += ' ';
          }
        }

        prevChar = char;
      }

      // Clean up extra spaces around punctuation
      result = result.replace(/\s*([{}(),:=\[\]])\s*/g, '$1');
      result = result.replace(/\s+/g, ' ');

      return result.trim();

    } catch (error) {
      return `Error minifying query: ${error}`;
    }
  }

  /**
   * Extract hardcoded values into variables
   */
  extractVariables(params: { query: string }): string {
    const { query } = params;

    try {
      const result: string[] = [];
      const variables: Array<{ name: string; type: string; value: string }> = [];
      let variableCounter = 1;

      // Find string literals
      const stringMatches = query.match(/:\s*"([^"]+)"/g) || [];
      stringMatches.forEach(match => {
        const value = match.match(/"([^"]+)"/)?.[1] || '';
        const variableName = `var${variableCounter++}`;
        variables.push({ name: variableName, type: 'String', value: `"${value}"` });
      });

      // Find number literals
      const numberMatches = query.match(/:\s*(\d+(?:\.\d+)?)(?!\w)/g) || [];
      numberMatches.forEach(match => {
        const value = match.match(/(\d+(?:\.\d+)?)/)?.[1] || '';
        const variableName = `var${variableCounter++}`;
        const type = value.includes('.') ? 'Float' : 'Int';
        variables.push({ name: variableName, type, value });
      });

      // Find boolean literals
      const boolMatches = query.match(/:\s*(true|false)(?!\w)/gi) || [];
      boolMatches.forEach(match => {
        const value = match.match(/(true|false)/i)?.[1]?.toLowerCase() || '';
        const variableName = `var${variableCounter++}`;
        variables.push({ name: variableName, type: 'Boolean', value });
      });

      if (variables.length === 0) {
        return 'No hardcoded values found to extract into variables.';
      }

      result.push('# Original Query with Variables\n');

      // Generate the query with variables
      let modifiedQuery = query;
      variableCounter = 1;

      stringMatches.forEach(() => {
        const variableName = `$var${variableCounter++}`;
        modifiedQuery = modifiedQuery.replace(/:\s*"[^"]+"/, `: ${variableName}`);
      });

      numberMatches.forEach(() => {
        const variableName = `$var${variableCounter++}`;
        modifiedQuery = modifiedQuery.replace(/:\s*\d+(?:\.\d+)?/, `: ${variableName}`);
      });

      boolMatches.forEach(() => {
        const variableName = `$var${variableCounter++}`;
        modifiedQuery = modifiedQuery.replace(/:\s*(true|false)/i, `: ${variableName}`);
      });

      // Add variable declarations to query
      const operationMatch = modifiedQuery.match(/^\s*(query|mutation|subscription)(\s+\w+)?/i);
      if (operationMatch) {
        const variableDeclarations = variables.map(v => `$${v.name}: ${v.type}`).join(', ');
        const replacement = `${operationMatch[1]}${operationMatch[2] || ''}(${variableDeclarations})`;
        modifiedQuery = modifiedQuery.replace(operationMatch[0], replacement);
      } else {
        // Anonymous query - need to add query wrapper
        const variableDeclarations = variables.map(v => `$${v.name}: ${v.type}`).join(', ');
        modifiedQuery = `query(${variableDeclarations}) {\n${modifiedQuery}\n}`;
      }

      result.push('```graphql');
      result.push(modifiedQuery);
      result.push('```\n');

      result.push('# Variables');
      result.push('```json');
      result.push('{');
      variables.forEach((variable, index) => {
        const comma = index < variables.length - 1 ? ',' : '';
        result.push(`  "${variable.name}": ${variable.value}${comma}`);
      });
      result.push('}');
      result.push('```');

      return result.join('\n');

    } catch (error) {
      return `Error extracting variables: ${error}`;
    }
  }

  /**
   * Generate field aliases to avoid conflicts in complex queries
   */
  generateAliases(params: { query: string }): string {
    const { query } = params;

    try {
      const result: string[] = [];
      result.push('# Query with Generated Aliases\n');

      // This is a simplified implementation
      // In a real implementation, you'd parse the GraphQL AST
      let modifiedQuery = query;
      let aliasCounter = 1;

      // Find field patterns and add aliases
      const fieldPattern = /(\w+)\s*(\([^)]*\))?\s*{/g;
      modifiedQuery = modifiedQuery.replace(fieldPattern, (match, fieldName, args) => {
        const alias = `${fieldName}Alias${aliasCounter++}`;
        return `${alias}: ${fieldName}${args || ''} {`;
      });

      result.push('```graphql');
      result.push(modifiedQuery);
      result.push('```\n');

      result.push('## Generated Aliases');
      result.push('The query has been modified to include aliases for all fields to avoid conflicts.');

      return result.join('\n');

    } catch (error) {
      return `Error generating aliases: ${error}`;
    }
  }

  private countChars(input: string, target: string): number {
    return (input.match(new RegExp(target, 'g')) || []).length;
  }

  private isGraphQLKeyword(word: string): boolean {
    const keywords = [
      'query', 'mutation', 'subscription', 'fragment', 'on', 'type', 'interface',
      'union', 'enum', 'scalar', 'input', 'extend', 'implements', 'directive',
      'schema', 'true', 'false', 'null'
    ];
    return keywords.includes(word.toLowerCase());
  }
}
