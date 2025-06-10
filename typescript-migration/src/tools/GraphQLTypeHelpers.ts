export interface GraphQLType {
  kind: string;
  name?: string;
  ofType?: GraphQLType;
}

export class GraphQLTypeHelpers {
  /**
   * Get the human-readable type name from a GraphQL type object
   */
  static getTypeName(type: GraphQLType): string {
    if (type.kind === 'NON_NULL') {
      return `${this.getTypeName(type.ofType!)}!`;
    }
    
    if (type.kind === 'LIST') {
      return `[${this.getTypeName(type.ofType!)}]`;
    }
    
    return type.name || 'Unknown';
  }

  /**
   * Check if a type is nullable
   */
  static isNullable(type: GraphQLType): boolean {
    return type.kind !== 'NON_NULL';
  }

  /**
   * Check if a type is a list
   */
  static isList(type: GraphQLType): boolean {
    if (type.kind === 'LIST') {
      return true;
    }
    
    if (type.kind === 'NON_NULL' && type.ofType) {
      return type.ofType.kind === 'LIST';
    }
    
    return false;
  }

  /**
   * Get the base type (unwrapping NON_NULL and LIST wrappers)
   */
  static getBaseType(type: GraphQLType): GraphQLType {
    if (type.kind === 'NON_NULL' || type.kind === 'LIST') {
      return this.getBaseType(type.ofType!);
    }
    
    return type;
  }

  /**
   * Check if a type is a scalar type
   */
  static isScalar(type: GraphQLType): boolean {
    const baseType = this.getBaseType(type);
    return baseType.kind === 'SCALAR';
  }

  /**
   * Check if a type is an enum
   */
  static isEnum(type: GraphQLType): boolean {
    const baseType = this.getBaseType(type);
    return baseType.kind === 'ENUM';
  }

  /**
   * Check if a type is an object type
   */
  static isObject(type: GraphQLType): boolean {
    const baseType = this.getBaseType(type);
    return baseType.kind === 'OBJECT';
  }

  /**
   * Check if a type is an interface
   */
  static isInterface(type: GraphQLType): boolean {
    const baseType = this.getBaseType(type);
    return baseType.kind === 'INTERFACE';
  }

  /**
   * Check if a type is a union
   */
  static isUnion(type: GraphQLType): boolean {
    const baseType = this.getBaseType(type);
    return baseType.kind === 'UNION';
  }

  /**
   * Check if a type is an input type
   */
  static isInputType(type: GraphQLType): boolean {
    const baseType = this.getBaseType(type);
    return baseType.kind === 'INPUT_OBJECT';
  }

  /**
   * Generate a mock value for a given type
   */
  static generateMockValue(type: GraphQLType): any {
    const baseType = this.getBaseType(type);
    
    switch (baseType.kind) {
      case 'SCALAR':
        return this.generateScalarMock(baseType.name || 'String');
      case 'ENUM':
        return 'ENUM_VALUE';
      case 'OBJECT':
        return {};
      case 'INTERFACE':
        return {};
      case 'UNION':
        return {};
      case 'INPUT_OBJECT':
        return {};
      default:
        return null;
    }
  }

  private static generateScalarMock(scalarName: string): any {
    switch (scalarName) {
      case 'String':
        return 'example string';
      case 'Int':
        return 42;
      case 'Float':
        return 3.14;
      case 'Boolean':
        return true;
      case 'ID':
        return 'example-id';
      case 'Date':
      case 'DateTime':
        return new Date().toISOString();
      default:
        return 'mock value';
    }
  }

  /**
   * Convert a GraphQL type to TypeScript type annotation
   */
  static toTypeScriptType(type: GraphQLType): string {
    if (type.kind === 'NON_NULL') {
      return this.toTypeScriptType(type.ofType!);
    }
    
    if (type.kind === 'LIST') {
      const innerType = this.toTypeScriptType(type.ofType!);
      return `${innerType}[]`;
    }
    
    const baseType = type.name || 'unknown';
    
    switch (type.kind) {
      case 'SCALAR':
        return this.scalarToTypeScript(baseType);
      case 'ENUM':
        return baseType;
      case 'OBJECT':
      case 'INTERFACE':
      case 'UNION':
      case 'INPUT_OBJECT':
        return baseType;
      default:
        return 'unknown';
    }
  }

  private static scalarToTypeScript(scalarName: string): string {
    switch (scalarName) {
      case 'String':
      case 'ID':
        return 'string';
      case 'Int':
      case 'Float':
        return 'number';
      case 'Boolean':
        return 'boolean';
      case 'Date':
      case 'DateTime':
        return 'Date | string';
      default:
        return 'any';
    }
  }
}
