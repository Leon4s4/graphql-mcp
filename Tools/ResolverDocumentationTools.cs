using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Tools;

[McpServerToolType]
public static class ResolverDocumentationTools
{
    [McpServerTool, Description("Generate documentation for GraphQL resolvers based on schema analysis")]
    public static async Task<string> GenerateResolverDocs(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Type name to document resolvers for (optional)")] string? typeName = null,
        [Description("Include field descriptions")] bool includeDescriptions = true,
        [Description("Include argument details")] bool includeArguments = true,
        [Description("Include return type information")] bool includeReturnTypes = true,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null)
    {
      
            // Get schema introspection
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) || 
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to retrieve schema data";
            }

            var documentation = new StringBuilder();
            documentation.AppendLine("# GraphQL Resolver Documentation\n");

            // Get root types
            var queryType = schema.TryGetProperty("queryType", out var qt) ? qt.GetProperty("name").GetString() : null;
            var mutationType = schema.TryGetProperty("mutationType", out var mt) ? mt.GetProperty("name").GetString() : null;
            var subscriptionType = schema.TryGetProperty("subscriptionType", out var st) ? st.GetProperty("name").GetString() : null;

            if (!schema.TryGetProperty("types", out var types))
            {
                return documentation.ToString() + "No types found in schema";
            }

            // Filter types to document
            var typesToDocument = new List<JsonElement>();
            foreach (var type in types.EnumerateArray())
            {
                if (!type.TryGetProperty("name", out var nameElement))
                    continue;

                var currentTypeName = nameElement.GetString();
                if (currentTypeName?.StartsWith("__") == true)
                    continue;

                // Include specific type or root types
                if (!string.IsNullOrEmpty(typeName))
                {
                    if (currentTypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                    {
                        typesToDocument.Add(type);
                    }
                }
                else if (currentTypeName == queryType || currentTypeName == mutationType || currentTypeName == subscriptionType)
                {
                    typesToDocument.Add(type);
                }
            }

            foreach (var type in typesToDocument)
            {
                var typeDoc = GenerateTypeResolverDoc(type, includeDescriptions, includeArguments, includeReturnTypes);
                documentation.AppendLine(typeDoc);
                documentation.AppendLine();
            }

            // Add implementation guidance
            documentation.AppendLine(GenerateImplementationGuidance());

            return documentation.ToString();
    }

    [McpServerTool, Description("Generate resolver implementation templates for specific types")]
    public static async Task<string> GenerateResolverTemplates(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Type name to generate templates for")] string typeName,
        [Description("Programming language for templates")] string language = "csharp",
        [Description("Include error handling")] bool includeErrorHandling = true,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null)
    {
      
            // Get schema introspection
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) || 
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to retrieve schema data";
            }

            // Find the specified type
            var targetType = FindTypeInSchema(schema, typeName);
            if (targetType == null)
            {
                return $"Type '{typeName}' not found in schema";
            }

            var templates = new StringBuilder();
            templates.AppendLine($"# Resolver Templates for {typeName}\n");

            switch (language.ToLower())
            {
                case "csharp":
                case "c#":
                    templates.AppendLine(GenerateCSharpResolverTemplate(targetType.Value, includeErrorHandling));
                    break;
                case "javascript":
                case "js":
                    templates.AppendLine(GenerateJavaScriptResolverTemplate(targetType.Value, includeErrorHandling));
                    break;
                case "typescript":
                case "ts":
                    templates.AppendLine(GenerateTypeScriptResolverTemplate(targetType.Value, includeErrorHandling));
                    break;
                case "python":
                case "py":
                    templates.AppendLine(GeneratePythonResolverTemplate(targetType.Value, includeErrorHandling));
                    break;
                default:
                    return $"Language '{language}' not supported. Supported languages: csharp, javascript, typescript, python";
            }

            return templates.ToString();
    }

    [McpServerTool, Description("Document resolver performance characteristics and optimization tips")]
    public static async Task<string> DocumentResolverPerformance(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Type name to analyze (optional)")] string? typeName = null,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null)
    {
       
            var performanceDoc = new StringBuilder();
            performanceDoc.AppendLine("# GraphQL Resolver Performance Guide\n");

            // Get schema for analysis
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) || 
                !data.TryGetProperty("__schema", out var schema))
            {
                return "Failed to retrieve schema data";
            }

            performanceDoc.AppendLine("## Performance Best Practices\n");
            performanceDoc.AppendLine("### 1. N+1 Query Problem Prevention");
            performanceDoc.AppendLine("- **Use DataLoader pattern**: Batch related database queries");
            performanceDoc.AppendLine("- **Implement field-level batching**: Group similar field resolutions");
            performanceDoc.AppendLine("- **Consider query complexity**: Limit nested query depth\n");

            performanceDoc.AppendLine("### 2. Caching Strategies");
            performanceDoc.AppendLine("- **Field-level caching**: Cache expensive computations");
            performanceDoc.AppendLine("- **Query result caching**: Cache complete query results when appropriate");
            performanceDoc.AppendLine("- **Cache invalidation**: Implement proper cache invalidation strategies\n");

            performanceDoc.AppendLine("### 3. Database Optimization");
            performanceDoc.AppendLine("- **Eager loading**: Load related data in single queries");
            performanceDoc.AppendLine("- **Projection**: Only select fields that are requested");
            performanceDoc.AppendLine("- **Indexing**: Ensure proper database indexes for common queries\n");

            // Analyze specific type if provided
            if (!string.IsNullOrEmpty(typeName))
            {
                var typeAnalysis = AnalyzeTypePerformance(schema, typeName);
                performanceDoc.AppendLine($"## Performance Analysis for {typeName}\n");
                performanceDoc.AppendLine(typeAnalysis);
            }

            // Add monitoring guidance
            performanceDoc.AppendLine("## Monitoring and Profiling\n");
            performanceDoc.AppendLine("### Key Metrics to Track");
            performanceDoc.AppendLine("- **Resolver execution time**: Time spent in individual resolvers");
            performanceDoc.AppendLine("- **Query complexity**: Number of fields and nesting depth");
            performanceDoc.AppendLine("- **Database query count**: Number of database calls per GraphQL query");
            performanceDoc.AppendLine("- **Memory usage**: Peak memory usage during query execution\n");

            performanceDoc.AppendLine("### Profiling Tools");
            performanceDoc.AppendLine("- Use GraphQL-specific profiling tools");
            performanceDoc.AppendLine("- Monitor database query logs");
            performanceDoc.AppendLine("- Implement custom performance logging");
            performanceDoc.AppendLine("- Use APM tools for production monitoring");

            return performanceDoc.ToString();
       
    }

    private static string GenerateTypeResolverDoc(JsonElement type, bool includeDescriptions, 
        bool includeArguments, bool includeReturnTypes)
    {
        var typeName = type.GetProperty("name").GetString();
        var typeKind = type.GetProperty("kind").GetString();
        var description = type.TryGetProperty("description", out var desc) ? desc.GetString() : null;

        var doc = new StringBuilder();
        doc.AppendLine($"## {typeName} Resolvers\n");

        if (includeDescriptions && !string.IsNullOrEmpty(description))
        {
            doc.AppendLine($"**Description:** {description}\n");
        }

        if (type.TryGetProperty("fields", out var fields))
        {
            doc.AppendLine("### Fields\n");
            foreach (var field in fields.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString();
                var fieldDesc = field.TryGetProperty("description", out var fd) ? fd.GetString() : null;
                var fieldType = GraphQLTypeHelpers.GetTypeName(field.GetProperty("type"));

                doc.AppendLine($"#### `{fieldName}`");
                
                if (includeDescriptions && !string.IsNullOrEmpty(fieldDesc))
                {
                    doc.AppendLine($"*{fieldDesc}*");
                }

                if (includeReturnTypes)
                {
                    doc.AppendLine($"**Returns:** `{fieldType}`");
                }

                if (includeArguments && field.TryGetProperty("args", out var args) && args.GetArrayLength() > 0)
                {
                    doc.AppendLine("**Arguments:**");
                    foreach (var arg in args.EnumerateArray())
                    {
                        var argName = arg.GetProperty("name").GetString();
                        var argType = GraphQLTypeHelpers.GetTypeName(arg.GetProperty("type"));
                        var argDesc = arg.TryGetProperty("description", out var ad) ? ad.GetString() : "";
                        
                        doc.AppendLine($"- `{argName}`: {argType}" + 
                            (!string.IsNullOrEmpty(argDesc) ? $" - {argDesc}" : ""));
                    }
                }

                doc.AppendLine($"**Resolver Implementation:**");
                doc.AppendLine("```csharp");
                doc.AppendLine($"public async Task<{GraphQLTypeHelpers.ConvertGraphQLTypeToCSharp(fieldType, true)}> {fieldName}Async(");
                
                if (field.TryGetProperty("args", out var resolverArgs) && resolverArgs.GetArrayLength() > 0)
                {
                    var argsList = new List<string>();
                    foreach (var arg in resolverArgs.EnumerateArray())
                    {
                        var argName = arg.GetProperty("name").GetString();
                        var argType = GraphQLTypeHelpers.GetTypeName(arg.GetProperty("type"));
                        argsList.Add($"    {GraphQLTypeHelpers.ConvertGraphQLTypeToCSharp(argType, true)} {argName}");
                    }
                    doc.AppendLine(string.Join(",\n", argsList));
                }
                
                doc.AppendLine(")");
                doc.AppendLine("{");
                doc.AppendLine("    // TODO: Implement resolver logic");
                doc.AppendLine("    throw new NotImplementedException();");
                doc.AppendLine("}");
                doc.AppendLine("```\n");
            }
        }

        return doc.ToString();
    }

    private static string GenerateCSharpResolverTemplate(JsonElement type, bool includeErrorHandling)
    {
        var typeName = type.GetProperty("name").GetString();
        var template = new StringBuilder();
        
        template.AppendLine("## C# Resolver Template\n");
        template.AppendLine("```csharp");
        template.AppendLine("using System;");
        template.AppendLine("using System.Threading.Tasks;");
        if (includeErrorHandling)
        {
            template.AppendLine("using Microsoft.Extensions.Logging;");
        }
        template.AppendLine();
        template.AppendLine($"public class {typeName}Resolver");
        template.AppendLine("{");
        
        if (includeErrorHandling)
        {
            template.AppendLine($"    private readonly ILogger<{typeName}Resolver> _logger;");
            template.AppendLine();
            template.AppendLine($"    public {typeName}Resolver(ILogger<{typeName}Resolver> logger)");
            template.AppendLine("    {");
            template.AppendLine("        _logger = logger;");
            template.AppendLine("    }");
            template.AppendLine();
        }

        if (type.TryGetProperty("fields", out var fields))
        {
            foreach (var field in fields.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString();
                var fieldType = GraphQLTypeHelpers.GetTypeName(field.GetProperty("type"));
                var csharpType = GraphQLTypeHelpers.ConvertGraphQLTypeToCSharp(fieldType, true);

                template.AppendLine($"    public async Task<{csharpType}> Get{fieldName}Async()");
                template.AppendLine("    {");
                
                if (includeErrorHandling)
                {
                    template.AppendLine("        try");
                    template.AppendLine("        {");
                    template.AppendLine($"            _logger.LogInformation(\"Resolving {fieldName}\");");
                    template.AppendLine("            // TODO: Implement resolver logic");
                    template.AppendLine("            throw new NotImplementedException();");
                    template.AppendLine("        }");
                    template.AppendLine("        catch (Exception ex)");
                    template.AppendLine("        {");
                    template.AppendLine($"            _logger.LogError(ex, \"Error resolving {fieldName}\");");
                    template.AppendLine("            throw;");
                    template.AppendLine("        }");
                }
                else
                {
                    template.AppendLine("        // TODO: Implement resolver logic");
                    template.AppendLine("        throw new NotImplementedException();");
                }
                
                template.AppendLine("    }");
                template.AppendLine();
            }
        }

        template.AppendLine("}");
        template.AppendLine("```");

        return template.ToString();
    }

    private static string GenerateJavaScriptResolverTemplate(JsonElement type, bool includeErrorHandling)
    {
        var typeName = type.GetProperty("name").GetString();
        var template = new StringBuilder();
        
        template.AppendLine("## JavaScript Resolver Template\n");
        template.AppendLine("```javascript");
        template.AppendLine($"const {typeName.ToLower()}Resolvers = {{");

        if (type.TryGetProperty("fields", out var fields))
        {
            var fieldList = new List<string>();
            foreach (var field in fields.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString();
                var resolver = new StringBuilder();
                
                if (includeErrorHandling)
                {
                    resolver.AppendLine($"  {fieldName}: async (parent, args, context) => {{");
                    resolver.AppendLine("    try {");
                    resolver.AppendLine($"      console.log('Resolving {fieldName}');");
                    resolver.AppendLine("      // TODO: Implement resolver logic");
                    resolver.AppendLine("      throw new Error('Not implemented');");
                    resolver.AppendLine("    } catch (error) {");
                    resolver.AppendLine($"      console.error('Error resolving {fieldName}:', error);");
                    resolver.AppendLine("      throw error;");
                    resolver.AppendLine("    }");
                    resolver.Append("  }");
                }
                else
                {
                    resolver.AppendLine($"  {fieldName}: async (parent, args, context) => {{");
                    resolver.AppendLine("    // TODO: Implement resolver logic");
                    resolver.AppendLine("    throw new Error('Not implemented');");
                    resolver.Append("  }");
                }
                
                fieldList.Add(resolver.ToString());
            }
            template.AppendLine(string.Join(",\n", fieldList));
        }

        template.AppendLine("};");
        template.AppendLine();
        template.AppendLine($"module.exports = {typeName.ToLower()}Resolvers;");
        template.AppendLine("```");

        return template.ToString();
    }

    private static string GenerateTypeScriptResolverTemplate(JsonElement type, bool includeErrorHandling)
    {
        var typeName = type.GetProperty("name").GetString();
        var template = new StringBuilder();
        
        template.AppendLine("## TypeScript Resolver Template\n");
        template.AppendLine("```typescript");
        template.AppendLine("import { Resolver, ResolverContext } from './types';");
        template.AppendLine();
        template.AppendLine($"interface {typeName}Resolvers {{");
        
        if (type.TryGetProperty("fields", out var fields))
        {
            foreach (var field in fields.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString();
                var fieldType = GraphQLTypeHelpers.GetTypeName(field.GetProperty("type"));
                var tsType = ConvertGraphQLTypeToTypeScript(fieldType);
                
                template.AppendLine($"  {fieldName}: Resolver<{tsType}>;");
            }
        }
        
        template.AppendLine("}");
        template.AppendLine();
        template.AppendLine($"const {typeName.ToLower()}Resolvers: {typeName}Resolvers = {{");
        
        // Implementation would continue similar to JavaScript version
        template.AppendLine("  // TODO: Implement resolvers");
        template.AppendLine("};");
        template.AppendLine();
        template.AppendLine($"export default {typeName.ToLower()}Resolvers;");
        template.AppendLine("```");

        return template.ToString();
    }

    private static string GeneratePythonResolverTemplate(JsonElement type, bool includeErrorHandling)
    {
        var typeName = type.GetProperty("name").GetString();
        var template = new StringBuilder();
        
        template.AppendLine("## Python Resolver Template\n");
        template.AppendLine("```python");
        
        if (includeErrorHandling)
        {
            template.AppendLine("import logging");
            template.AppendLine();
            template.AppendLine("logger = logging.getLogger(__name__)");
            template.AppendLine();
        }
        
        template.AppendLine($"class {typeName}Resolver:");
        
        if (type.TryGetProperty("fields", out var fields))
        {
            foreach (var field in fields.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString();
                
                template.AppendLine($"    async def resolve_{fieldName}(self, info, **kwargs):");
                
                if (includeErrorHandling)
                {
                    template.AppendLine("        try:");
                    template.AppendLine($"            logger.info(f'Resolving {fieldName}')");
                    template.AppendLine("            # TODO: Implement resolver logic");
                    template.AppendLine("            raise NotImplementedError()");
                    template.AppendLine("        except Exception as e:");
                    template.AppendLine($"            logger.error(f'Error resolving {fieldName}: {{e}}')");
                    template.AppendLine("            raise");
                }
                else
                {
                    template.AppendLine("        # TODO: Implement resolver logic");
                    template.AppendLine("        raise NotImplementedError()");
                }
                
                template.AppendLine();
            }
        }
        
        template.AppendLine("```");

        return template.ToString();
    }

    private static string GenerateImplementationGuidance()
    {
        return @"## Implementation Guidance

### General Principles
1. **Keep resolvers focused**: Each resolver should have a single responsibility
2. **Handle errors gracefully**: Always implement proper error handling
3. **Use async/await**: Make resolvers asynchronous for better performance
4. **Validate inputs**: Always validate arguments and context

### Common Patterns
- **Repository Pattern**: Separate data access logic from resolver logic
- **Service Layer**: Use services for complex business logic
- **Dependency Injection**: Inject dependencies rather than creating them
- **Caching**: Implement caching for expensive operations

### Testing Recommendations
- **Unit tests**: Test each resolver in isolation
- **Integration tests**: Test resolver behavior with real data sources
- **Performance tests**: Measure resolver execution time
- **Error scenarios**: Test error handling and edge cases";
    }

    private static string AnalyzeTypePerformance(JsonElement schema, string typeName)
    {
        var analysis = new StringBuilder();
        
        var targetType = FindTypeInSchema(schema, typeName);
        if (targetType == null)
        {
            return $"Type '{typeName}' not found in schema";
        }

        analysis.AppendLine($"### Performance Characteristics");
        
        if (targetType.Value.TryGetProperty("fields", out var fields))
        {
            var fieldCount = fields.GetArrayLength();
            analysis.AppendLine($"- **Field count**: {fieldCount}");
            
            var complexFields = 0;
            var listFields = 0;
            
            foreach (var field in fields.EnumerateArray())
            {
                var fieldType = GraphQLTypeHelpers.GetTypeName(field.GetProperty("type"));
                
                if (fieldType.Contains("["))
                {
                    listFields++;
                }
                
                if (field.TryGetProperty("args", out var args) && args.GetArrayLength() > 0)
                {
                    complexFields++;
                }
            }
            
            analysis.AppendLine($"- **List fields**: {listFields} (potential N+1 risk)");
            analysis.AppendLine($"- **Fields with arguments**: {complexFields} (may require optimization)");
            
            if (listFields > 0)
            {
                analysis.AppendLine();
                analysis.AppendLine("**Recommendations:**");
                analysis.AppendLine("- Implement DataLoader for list field batching");
                analysis.AppendLine("- Consider pagination for large lists");
                analysis.AppendLine("- Monitor database query counts");
            }
        }

        return analysis.ToString();
    }

    private static JsonElement? FindTypeInSchema(JsonElement schema, string typeName)
    {
        if (!schema.TryGetProperty("types", out var types))
            return null;

        foreach (var type in types.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var name) && 
                name.GetString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true)
            {
                return type;
            }
        }

        return null;
    }


    private static string ConvertGraphQLTypeToTypeScript(string graphqlType)
    {
        var isNonNull = graphqlType.EndsWith("!");
        var isList = graphqlType.Contains("[");
        
        var baseType = graphqlType.Replace("!", "").Replace("[", "").Replace("]", "");
        
        var tsType = baseType switch
        {
            "String" => "string",
            "Int" => "number",
            "Float" => "number",
            "Boolean" => "boolean",
            "ID" => "string",
            _ => baseType
        };

        if (isList)
        {
            tsType = $"Array<{tsType}>";
        }

        if (!isNonNull)
        {
            tsType += " | null";
        }

        return tsType;
    }
}
