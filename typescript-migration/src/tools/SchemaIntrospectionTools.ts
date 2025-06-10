import axios from 'axios';

export class SchemaIntrospectionTools {
  /**
   * Query the GraphQL schema to understand available types, fields, and operations
   */
  async introspectSchema(params: {
    endpoint: string;
    headers?: string;
  }): Promise<string> {
    const { endpoint, headers } = params;

    try {
      const httpHeaders: Record<string, string> = {};
      
      if (headers) {
        try {
          Object.assign(httpHeaders, JSON.parse(headers));
        } catch (error) {
          return `Invalid headers JSON: ${error}`;
        }
      }

      const introspectionQuery = `
        query IntrospectionQuery {
          __schema {
            queryType { name }
            mutationType { name }
            subscriptionType { name }
            types {
              ...FullType
            }
            directives {
              name
              description
              locations
              args {
                ...InputValue
              }
            }
          }
        }
        
        fragment FullType on __Type {
          kind
          name
          description
          fields(includeDeprecated: true) {
            name
            description
            args {
              ...InputValue
            }
            type {
              ...TypeRef
            }
            isDeprecated
            deprecationReason
          }
          inputFields {
            ...InputValue
          }
          interfaces {
            ...TypeRef
          }
          enumValues(includeDeprecated: true) {
            name
            description
            isDeprecated
            deprecationReason
          }
          possibleTypes {
            ...TypeRef
          }
        }
        
        fragment InputValue on __InputValue {
          name
          description
          type { ...TypeRef }
          defaultValue
        }
        
        fragment TypeRef on __Type {
          kind
          name
          ofType {
            kind
            name
            ofType {
              kind
              name
              ofType {
                kind
                name
                ofType {
                  kind
                  name
                  ofType {
                    kind
                    name
                    ofType {
                      kind
                      name
                      ofType {
                        kind
                        name
                      }
                    }
                  }
                }
              }
            }
          }
        }
      `;

      const response = await axios.post(
        endpoint,
        {
          query: introspectionQuery,
        },
        {
          headers: {
            'Content-Type': 'application/json',
            ...httpHeaders,
          },
          timeout: 30000,
        }
      );

      if (response.data.errors) {
        return `GraphQL introspection errors: ${JSON.stringify(response.data.errors, null, 2)}`;
      }

      return JSON.stringify(response.data, null, 2);

    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response) {
          return `HTTP ${error.response.status}: ${JSON.stringify(error.response.data)}`;
        } else if (error.request) {
          return `Network error: Could not reach ${endpoint}`;
        } else {
          return `Request setup error: ${error.message}`;
        }
      }
      
      return `Error during schema introspection: ${error}`;
    }
  }

  /**
   * Get schema type information for a specific type
   */
  async getTypeInfo(params: {
    endpoint: string;
    typeName: string;
    headers?: string;
  }): Promise<string> {
    const { endpoint, typeName, headers } = params;

    try {
      const httpHeaders: Record<string, string> = {};
      
      if (headers) {
        try {
          Object.assign(httpHeaders, JSON.parse(headers));
        } catch (error) {
          return `Invalid headers JSON: ${error}`;
        }
      }

      const typeQuery = `
        query GetTypeInfo($typeName: String!) {
          __type(name: $typeName) {
            kind
            name
            description
            fields(includeDeprecated: true) {
              name
              description
              args {
                name
                description
                type {
                  name
                  kind
                  ofType {
                    name
                    kind
                  }
                }
                defaultValue
              }
              type {
                name
                kind
                ofType {
                  name
                  kind
                  ofType {
                    name
                    kind
                  }
                }
              }
              isDeprecated
              deprecationReason
            }
            inputFields {
              name
              description
              type {
                name
                kind
                ofType {
                  name
                  kind
                }
              }
              defaultValue
            }
            interfaces {
              name
              kind
            }
            enumValues(includeDeprecated: true) {
              name
              description
              isDeprecated
              deprecationReason
            }
            possibleTypes {
              name
              kind
            }
          }
        }
      `;

      const response = await axios.post(
        endpoint,
        {
          query: typeQuery,
          variables: { typeName },
        },
        {
          headers: {
            'Content-Type': 'application/json',
            ...httpHeaders,
          },
          timeout: 30000,
        }
      );

      if (response.data.errors) {
        return `GraphQL type query errors: ${JSON.stringify(response.data.errors, null, 2)}`;
      }

      if (!response.data.data.__type) {
        return `Type '${typeName}' not found in schema`;
      }

      return JSON.stringify(response.data.data.__type, null, 2);

    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response) {
          return `HTTP ${error.response.status}: ${JSON.stringify(error.response.data)}`;
        } else if (error.request) {
          return `Network error: Could not reach ${endpoint}`;
        } else {
          return `Request setup error: ${error.message}`;
        }
      }
      
      return `Error getting type info: ${error}`;
    }
  }
}
