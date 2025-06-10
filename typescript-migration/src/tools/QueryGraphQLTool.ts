import axios, { AxiosInstance } from 'axios';

export interface GraphQLRequest {
  query: string;
  variables?: Record<string, any>;
  operationName?: string;
}

export interface GraphQLResponse {
  data?: any;
  errors?: Array<{
    message: string;
    locations?: Array<{ line: number; column: number }>;
    path?: Array<string | number>;
  }>;
}

export class QueryGraphQLTool {
  private httpClient: AxiosInstance;
  private endpoint: string;
  private headers: Record<string, string>;
  private allowMutations: boolean;

  constructor(
    endpoint: string,
    headers: Record<string, string> = {},
    allowMutations: boolean = false
  ) {
    this.endpoint = endpoint;
    this.headers = headers;
    this.allowMutations = allowMutations;
    
    this.httpClient = axios.create({
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
        ...headers,
      },
    });
  }

  /**
   * Execute GraphQL queries and mutations with full error handling
   */
  async queryGraphql(params: {
    query: string;
    variables?: string;
  }): Promise<string> {
    const { query, variables } = params;

    // Basic mutation detection
    if (this.isMutation(query) && !this.allowMutations) {
      return JSON.stringify({
        isError: true,
        content: [
          {
            type: 'text',
            text: 'Mutations are not allowed unless you enable them in the configuration. Please use a query operation instead.',
          },
        ],
      });
    }

    try {
      let parsedVariables: Record<string, any> = {};
      
      if (variables) {
        try {
          parsedVariables = JSON.parse(variables);
        } catch (error) {
          return JSON.stringify({
            isError: true,
            content: [
              {
                type: 'text',
                text: `Invalid variables JSON: ${error}`,
              },
            ],
          });
        }
      }

      const requestBody: GraphQLRequest = {
        query,
        ...(Object.keys(parsedVariables).length > 0 && { variables: parsedVariables }),
      };

      const response = await this.httpClient.post<GraphQLResponse>(
        this.endpoint,
        requestBody
      );

      if (response.data.errors) {
        return JSON.stringify({
          isError: true,
          content: [
            {
              type: 'text',
              text: `GraphQL errors: ${JSON.stringify(response.data.errors, null, 2)}`,
            },
          ],
        });
      }

      return JSON.stringify({
        isError: false,
        content: [
          {
            type: 'text',
            text: JSON.stringify(response.data, null, 2),
          },
        ],
      });

    } catch (error) {
      let errorMessage = 'Unknown error occurred';
      
      if (axios.isAxiosError(error)) {
        if (error.response) {
          errorMessage = `HTTP ${error.response.status}: ${JSON.stringify(error.response.data)}`;
        } else if (error.request) {
          errorMessage = `Network error: Could not reach ${this.endpoint}`;
        } else {
          errorMessage = `Request setup error: ${error.message}`;
        }
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }

      return JSON.stringify({
        isError: true,
        content: [
          {
            type: 'text',
            text: `Error executing GraphQL query: ${errorMessage}`,
          },
        ],
      });
    }
  }

  private isMutation(query: string): boolean {
    const trimmed = query.trim();
    return /^mutation\s/i.test(trimmed) || 
           (!trimmed.startsWith('query') && 
            !trimmed.startsWith('subscription') && 
            /\b(create|update|delete|add|remove|set)\b/i.test(trimmed));
  }
}
