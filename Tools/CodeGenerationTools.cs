using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class CodeGenerationTools
{
    [McpServerTool, Description("Generate strongly-typed C# classes and models from GraphQL schema types")]
    public static async Task<string> GenerateTypes(
        [Description("Name of the registered GraphQL endpoint")] string endpointName,
        [Description("Namespace for generated classes")]
        string namespaceName = "Generated.GraphQL",
        [Description("HTTP headers as JSON object (optional - will override endpoint headers)")]
        string? headers = null)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        // Use provided headers or fall back to endpoint headers
        var requestHeaders = !string.IsNullOrEmpty(headers) ? headers : 
            (endpointInfo.Headers.Count > 0 ? JsonSerializer.Serialize(endpointInfo.Headers) : null);

        var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointName, requestHeaders);
        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

        if (!schemaData.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("__schema", out var schema) ||
            !schema.TryGetProperty("types", out var types))
        {
            return "Failed to parse schema data for type generation";
        }

        var generatedCode = new StringBuilder();
        generatedCode.AppendLine("using System;");
        generatedCode.AppendLine("using System.Collections.Generic;");
        generatedCode.AppendLine("using System.ComponentModel.DataAnnotations;");
        generatedCode.AppendLine("using System.Text.Json.Serialization;");
        generatedCode.AppendLine();
        generatedCode.AppendLine($"namespace {namespaceName}");
        generatedCode.AppendLine("{");

        foreach (var type in types.EnumerateArray())
        {
            if (!type.TryGetProperty("name", out var nameElement) ||
                nameElement.GetString()
                    ?.StartsWith("__") == true)
                continue;

            var typeName = nameElement.GetString() ?? "";
            var kind = type.GetProperty("kind")
                .GetString();

            switch (kind)
            {
                case "OBJECT":
                case "INPUT_OBJECT":
                    generatedCode.AppendLine(GenerateClass(type, kind == "INPUT_OBJECT"));
                    break;
                case "ENUM":
                    generatedCode.AppendLine(GenerateEnum(type));
                    break;
                case "INTERFACE":
                    generatedCode.AppendLine(GenerateInterface(type));
                    break;
            }
        }

        generatedCode.AppendLine("}");
        return generatedCode.ToString();
    }

    [McpServerTool, Description("Create strongly-typed client classes with methods for executing specific GraphQL queries")]
    public static string GenerateClientCode(
        [Description("GraphQL query to generate client for")]
        string query,
        [Description("Class name for the generated client")]
        string className = "GraphQLClient",
        [Description("Namespace for generated client")]
        string namespaceName = "Generated.GraphQL")
    {
        var generatedCode = new StringBuilder();
        generatedCode.AppendLine("using System;");
        generatedCode.AppendLine("using System.Net.Http;");
        generatedCode.AppendLine("using System.Text;");
        generatedCode.AppendLine("using System.Text.Json;");
        generatedCode.AppendLine("using System.Threading.Tasks;");
        generatedCode.AppendLine("using System.Collections.Generic;");
        generatedCode.AppendLine();
        generatedCode.AppendLine($"namespace {namespaceName}");
        generatedCode.AppendLine("{");

        var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)\s+(\w+)?", RegexOptions.IgnoreCase);
        var operationType = operationMatch.Success
            ? operationMatch.Groups[1]
                .Value.ToLower()
            : "query";
        var operationName = operationMatch.Success && operationMatch.Groups[2].Success ? operationMatch.Groups[2].Value : "AnonymousOperation";

        generatedCode.AppendLine($"    public class {className}");
        generatedCode.AppendLine("    {");
        generatedCode.AppendLine("        private readonly HttpClient _httpClient;");
        generatedCode.AppendLine("        private readonly string _endpoint;");
        generatedCode.AppendLine();
        generatedCode.AppendLine($"        public {className}(HttpClient httpClient, string endpoint)");
        generatedCode.AppendLine("        {");
        generatedCode.AppendLine("            _httpClient = httpClient;");
        generatedCode.AppendLine("            _endpoint = endpoint;");
        generatedCode.AppendLine("        }");
        generatedCode.AppendLine();

        var variableMatches = Regex.Matches(query, @"\$(\w+):\s*([^,\)]+)", RegexOptions.IgnoreCase);
        var methodParams = new List<string>();
        var variableDict = new Dictionary<string, string>();

        foreach (Match match in variableMatches)
        {
            var varName = match.Groups[1].Value;
            var varType = GraphQlTypeHelpers.ConvertGraphQlTypeToCSharp(match.Groups[2]
                .Value.Trim());
            methodParams.Add($"{varType} {varName}");
            variableDict[varName] = varName;
        }

        var methodParamString = methodParams.Count > 0 ? string.Join(", ", methodParams) : "";

        generatedCode.AppendLine($"        public async Task<JsonElement> {operationName}Async({methodParamString})");
        generatedCode.AppendLine("        {");
        generatedCode.AppendLine("            var query = @\"");
        generatedCode.AppendLine($"                {query.Replace("\"", "\"\"")}");
        generatedCode.AppendLine("            \";");
        generatedCode.AppendLine();

        if (variableDict.Count > 0)
        {
            generatedCode.AppendLine("            var variables = new Dictionary<string, object>");
            generatedCode.AppendLine("            {");
            foreach (var kvp in variableDict)
            {
                generatedCode.AppendLine($"                [\"{kvp.Key}\"] = {kvp.Value},");
            }

            generatedCode.AppendLine("            };");
        }
        else
        {
            generatedCode.AppendLine("            var variables = (object?)null;");
        }

        generatedCode.AppendLine();
        generatedCode.AppendLine("            var request = new");
        generatedCode.AppendLine("            {");
        generatedCode.AppendLine("                query,");
        generatedCode.AppendLine("                variables");
        generatedCode.AppendLine("            };");
        generatedCode.AppendLine();
        generatedCode.AppendLine("            var json = JsonSerializer.Serialize(request);");
        generatedCode.AppendLine("            var content = new StringContent(json, Encoding.UTF8, \"application/json\");");
        generatedCode.AppendLine();
        generatedCode.AppendLine("            var response = await _httpClient.PostAsync(_endpoint, content);");
        generatedCode.AppendLine("            response.EnsureSuccessStatusCode();");
        generatedCode.AppendLine();
        generatedCode.AppendLine("            var responseContent = await response.Content.ReadAsStringAsync();");
        generatedCode.AppendLine("            return JsonSerializer.Deserialize<JsonElement>(responseContent);");
        generatedCode.AppendLine("        }");
        generatedCode.AppendLine("    }");
        generatedCode.AppendLine("}");

        return generatedCode.ToString();
    }

    [McpServerTool, Description("Generate fluent API builders for constructing GraphQL queries programmatically")]
    public static async Task<string> GenerateQueryBuilder(
        [Description("Name of the registered GraphQL endpoint")] string endpointName,
        [Description("Root type to generate builder for (Query/Mutation)")]
        string rootType = "Query",
        [Description("HTTP headers as JSON object (optional - will override endpoint headers)")]
        string? headers = null)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found. Please register the endpoint first using RegisterEndpoint.";
        }

        // Use provided headers or fall back to endpoint headers
        var requestHeaders = !string.IsNullOrEmpty(headers) ? headers : 
            (endpointInfo.Headers.Count > 0 ? JsonSerializer.Serialize(endpointInfo.Headers) : null);

        // Get schema to understand available fields
        var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpointName, requestHeaders);
        var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

        var generatedCode = new StringBuilder();
        generatedCode.AppendLine("using System;");
        generatedCode.AppendLine("using System.Collections.Generic;");
        generatedCode.AppendLine("using System.Linq;");
        generatedCode.AppendLine("using System.Text;");
        generatedCode.AppendLine();
        generatedCode.AppendLine("namespace Generated.GraphQL");
        generatedCode.AppendLine("{");

        // Generate base query builder
        generatedCode.AppendLine("    public class QueryBuilder");
        generatedCode.AppendLine("    {");
        generatedCode.AppendLine("        private readonly List<string> _selections = new();");
        generatedCode.AppendLine("        private readonly List<string> _fragments = new();");
        generatedCode.AppendLine("        private readonly Dictionary<string, object> _variables = new();");
        generatedCode.AppendLine();

        // Add field selection methods
        if (schemaData.TryGetProperty("data", out var data) &&
            data.TryGetProperty("__schema", out var schema) &&
            schema.TryGetProperty("types", out var types))
        {
            foreach (var type in types.EnumerateArray())
            {
                if (!type.TryGetProperty("name", out var nameElement) ||
                    nameElement.GetString() != rootType)
                    continue;

                if (type.TryGetProperty("fields", out var fields))
                {
                    foreach (var field in fields.EnumerateArray())
                    {
                        if (field.TryGetProperty("name", out var fieldName))
                        {
                            var fieldNameStr = fieldName.GetString() ?? "";
                            var methodName = ToPascalCase(fieldNameStr);

                            generatedCode.AppendLine($"        public QueryBuilder {methodName}(Action<{methodName}Builder>? configure = null)");
                            generatedCode.AppendLine("        {");
                            generatedCode.AppendLine($"            var builder = new {methodName}Builder();");
                            generatedCode.AppendLine("            configure?.Invoke(builder);");
                            generatedCode.AppendLine($"            _selections.Add($\"{fieldNameStr} {{{{ {{builder.Build()}} }}}}\");");
                            generatedCode.AppendLine("            return this;");
                            generatedCode.AppendLine("        }");
                            generatedCode.AppendLine();
                        }
                    }
                }

                break;
            }
        }

        // Add utility methods
        generatedCode.AppendLine("        public QueryBuilder AddVariable<T>(string name, T value)");
        generatedCode.AppendLine("        {");
        generatedCode.AppendLine("            _variables[name] = value;");
        generatedCode.AppendLine("            return this;");
        generatedCode.AppendLine("        }");
        generatedCode.AppendLine();

        generatedCode.AppendLine("        public QueryBuilder AddFragment(string fragmentName, string typeName, string fields)");
        generatedCode.AppendLine("        {");
        generatedCode.AppendLine("            _fragments.Add($\"fragment {fragmentName} on {typeName} {{ {fields} }}\");");
        generatedCode.AppendLine("            return this;");
        generatedCode.AppendLine("        }");
        generatedCode.AppendLine();

        generatedCode.AppendLine("        public string Build(string? operationName = null)");
        generatedCode.AppendLine("        {");
        generatedCode.AppendLine("            var query = new StringBuilder();");
        generatedCode.AppendLine();
        generatedCode.AppendLine("            // Add fragments");
        generatedCode.AppendLine("            foreach (var fragment in _fragments)");
        generatedCode.AppendLine("            {");
        generatedCode.AppendLine("                query.AppendLine(fragment);");
        generatedCode.AppendLine("            }");
        generatedCode.AppendLine();
        generatedCode.AppendLine("            // Add operation");
        generatedCode.AppendLine("            if (!string.IsNullOrEmpty(operationName))");
        generatedCode.AppendLine("            {");
        generatedCode.AppendLine($"                query.AppendLine($\"query {{operationName}} {{\");");
        generatedCode.AppendLine("            }");
        generatedCode.AppendLine("            else");
        generatedCode.AppendLine("            {");
        generatedCode.AppendLine("                query.AppendLine(\"{\");");
        generatedCode.AppendLine("            }");
        generatedCode.AppendLine();
        generatedCode.AppendLine("            foreach (var selection in _selections)");
        generatedCode.AppendLine("            {");
        generatedCode.AppendLine("                query.AppendLine($\"  {selection}\");");
        generatedCode.AppendLine("            }");
        generatedCode.AppendLine();
        generatedCode.AppendLine("            query.AppendLine(\"}\");");
        generatedCode.AppendLine("            return query.ToString();");
        generatedCode.AppendLine("        }");
        generatedCode.AppendLine("    }");
        generatedCode.AppendLine();

        // Generate field builders
        generatedCode.AppendLine("    public class FieldBuilder");
        generatedCode.AppendLine("    {");
        generatedCode.AppendLine("        protected readonly List<string> _selections = new();");
        generatedCode.AppendLine();
        generatedCode.AppendLine("        public virtual string Build()");
        generatedCode.AppendLine("        {");
        generatedCode.AppendLine("            return string.Join(\" \", _selections);");
        generatedCode.AppendLine("        }");
        generatedCode.AppendLine("    }");

        generatedCode.AppendLine("}");

        return generatedCode.ToString();
    }

    private static string GenerateClass(JsonElement type, bool isInput)
    {
        var sb = new StringBuilder();
        var typeName = type.GetProperty("name")
            .GetString() ?? "";
        var description = type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String
            ? desc.GetString()
            : null;

        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {description}");
            sb.AppendLine($"    /// </summary>");
        }

        sb.AppendLine($"    public class {typeName}");
        sb.AppendLine("    {");

        if (type.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
        {
            foreach (var field in fields.EnumerateArray())
            {
                GenerateProperty(sb, field);
            }
        }
        else if (type.TryGetProperty("inputFields", out var inputFields) && inputFields.ValueKind == JsonValueKind.Array)
        {
            foreach (var field in inputFields.EnumerateArray())
            {
                GenerateInputProperty(sb, field);
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GenerateEnum(JsonElement type)
    {
        var sb = new StringBuilder();
        var typeName = type.GetProperty("name")
            .GetString() ?? "";
        var description = type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String
            ? desc.GetString()
            : null;

        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {description}");
            sb.AppendLine($"    /// </summary>");
        }

        sb.AppendLine($"    public enum {typeName}");
        sb.AppendLine("    {");

        if (type.TryGetProperty("enumValues", out var enumValues) && enumValues.ValueKind == JsonValueKind.Array)
        {
            foreach (var enumValue in enumValues.EnumerateArray())
            {
                if (enumValue.TryGetProperty("name", out var name))
                {
                    var enumName = name.GetString() ?? "";
                    var enumDesc = enumValue.TryGetProperty("description", out var enumDescription) &&
                                   enumDescription.ValueKind == JsonValueKind.String
                        ? enumDescription.GetString()
                        : null;

                    if (!string.IsNullOrEmpty(enumDesc))
                    {
                        sb.AppendLine($"        /// <summary>{enumDesc}</summary>");
                    }

                    sb.AppendLine($"        {enumName},");
                }
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GenerateInterface(JsonElement type)
    {
        var sb = new StringBuilder();
        var typeName = type.GetProperty("name")
            .GetString() ?? "";
        var description = type.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String
            ? desc.GetString()
            : null;

        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {description}");
            sb.AppendLine($"    /// </summary>");
        }

        sb.AppendLine($"    public interface I{typeName}");
        sb.AppendLine("    {");

        if (type.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
        {
            foreach (var field in fields.EnumerateArray())
            {
                GenerateInterfaceProperty(sb, field);
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        return sb.ToString();
    }

    private static void GenerateProperty(StringBuilder sb, JsonElement field)
    {
        var fieldName = field.GetProperty("name")
            .GetString() ?? "";
        var propertyName = ToPascalCase(fieldName);
        var fieldType = GraphQlTypeHelpers.ConvertGraphQlTypeToCSharp(FormatGraphQlType(field.GetProperty("type")));
        var description = field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String
            ? desc.GetString()
            : null;

        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"        /// <summary>{description}</summary>");
        }

        sb.AppendLine($"        [JsonPropertyName(\"{fieldName}\")]");
        sb.AppendLine($"        public {fieldType} {propertyName} {{ get; set; }}");
        sb.AppendLine();
    }

    private static void GenerateInputProperty(StringBuilder sb, JsonElement field)
    {
        var fieldName = field.GetProperty("name")
            .GetString() ?? "";
        var propertyName = ToPascalCase(fieldName);
        var fieldType = GraphQlTypeHelpers.ConvertGraphQlTypeToCSharp(FormatGraphQlType(field.GetProperty("type")));
        var description = field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String
            ? desc.GetString()
            : null;

        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"        /// <summary>{description}</summary>");
        }

        sb.AppendLine($"        [JsonPropertyName(\"{fieldName}\")]");
        sb.AppendLine($"        public {fieldType} {propertyName} {{ get; set; }}");
        sb.AppendLine();
    }

    private static void GenerateInterfaceProperty(StringBuilder sb, JsonElement field)
    {
        var fieldName = field.GetProperty("name")
            .GetString() ?? "";
        var propertyName = ToPascalCase(fieldName);
        var fieldType = GraphQlTypeHelpers.ConvertGraphQlTypeToCSharp(FormatGraphQlType(field.GetProperty("type")));
        var description = field.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String
            ? desc.GetString()
            : null;

        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine($"        /// <summary>{description}</summary>");
        }

        sb.AppendLine($"        {fieldType} {propertyName} {{ get; set; }}");
        sb.AppendLine();
    }

    private static string FormatGraphQlType(JsonElement typeElement)
    {
        if (!typeElement.TryGetProperty("kind", out var kind))
            return "object";

        var kindStr = kind.GetString();

        switch (kindStr)
        {
            case "NON_NULL":
                if (typeElement.TryGetProperty("ofType", out var ofType))
                {
                    return FormatGraphQlType(ofType) + "!";
                }

                break;
            case "LIST":
                if (typeElement.TryGetProperty("ofType", out var listOfType))
                {
                    return "[" + FormatGraphQlType(listOfType) + "]";
                }

                break;
            default:
                if (typeElement.TryGetProperty("name", out var name))
                {
                    return name.GetString() ?? "object";
                }

                break;
        }

        return "object";
    }


    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input[1..];
    }
}