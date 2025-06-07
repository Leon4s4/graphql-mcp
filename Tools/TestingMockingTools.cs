using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Tools;

[McpServerToolType]
public static class TestingMockingTools
{
    [McpServerTool, Description("Generate mock data based on GraphQL schema types")]
    public static async Task<string> GenerateMockData(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Type name to generate mock data for")] string typeName,
        [Description("Number of mock instances to generate")] int count = 1,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null)
    {
        try
        {
            // Get schema introspection
            var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(endpoint, headers);
            var schemaData = JsonSerializer.Deserialize<JsonElement>(schemaJson);

            if (!schemaData.TryGetProperty("data", out var data) || 
                !data.TryGetProperty("__schema", out var schema) ||
                !schema.TryGetProperty("types", out var types))
            {
                return "Failed to parse schema data for mock generation";
            }

            // Find the specified type
            JsonElement? targetType = null;
            foreach (var type in types.EnumerateArray())
            {
                if (type.TryGetProperty("name", out var nameElement) && 
                    nameElement.GetString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    targetType = type;
                    break;
                }
            }

            if (!targetType.HasValue)
            {
                return $"Type '{typeName}' not found in schema";
            }

            var mockData = new List<object>();
            for (int i = 0; i < count; i++)
            {
                var mockInstance = GenerateMockInstance(targetType.Value, types, i);
                mockData.Add(mockInstance);
            }

            var result = new
            {
                type = typeName,
                count = count,
                data = mockData
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error generating mock data: {ex.Message}";
        }
    }

    [McpServerTool, Description("Generate unit tests for GraphQL queries")]
    public static string GenerateQueryTests(
        [Description("GraphQL query to generate tests for")] string query,
        [Description("Test class name")] string testClassName = "GraphQLQueryTests",
        [Description("Testing framework (NUnit/xUnit/MSTest)")] string framework = "xUnit")
    {
        try
        {
            var testCode = new StringBuilder();
            
            // Add using statements based on framework
            switch (framework.ToLower())
            {
                case "xunit":
                    testCode.AppendLine("using Xunit;");
                    testCode.AppendLine("using System.Threading.Tasks;");
                    testCode.AppendLine("using System.Net.Http;");
                    testCode.AppendLine("using System.Text;");
                    testCode.AppendLine("using System.Text.Json;");
                    testCode.AppendLine("using Moq;");
                    break;
                case "nunit":
                    testCode.AppendLine("using NUnit.Framework;");
                    testCode.AppendLine("using System.Threading.Tasks;");
                    testCode.AppendLine("using System.Net.Http;");
                    testCode.AppendLine("using System.Text;");
                    testCode.AppendLine("using System.Text.Json;");
                    testCode.AppendLine("using Moq;");
                    break;
                case "mstest":
                    testCode.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
                    testCode.AppendLine("using System.Threading.Tasks;");
                    testCode.AppendLine("using System.Net.Http;");
                    testCode.AppendLine("using System.Text;");
                    testCode.AppendLine("using System.Text.Json;");
                    testCode.AppendLine("using Moq;");
                    break;
            }

            testCode.AppendLine();
            testCode.AppendLine("namespace Generated.Tests");
            testCode.AppendLine("{");

            // Add test class attribute for MSTest
            if (framework.ToLower() == "mstest")
            {
                testCode.AppendLine("    [TestClass]");
            }

            testCode.AppendLine($"    public class {testClassName}");
            testCode.AppendLine("    {");

            // Extract operation details
            var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)\s+(\w+)?", RegexOptions.IgnoreCase);
            var operationType = operationMatch.Success ? operationMatch.Groups[1].Value.ToLower() : "query";
            var operationName = operationMatch.Success && operationMatch.Groups[2].Success ? 
                operationMatch.Groups[2].Value : "AnonymousOperation";

            // Generate setup method
            GenerateSetupMethod(testCode, framework);

            // Generate success test
            GenerateSuccessTest(testCode, framework, operationName, query, operationType);

            // Generate error handling test
            GenerateErrorTest(testCode, framework, operationName, query);

            // Generate validation test
            GenerateValidationTest(testCode, framework, operationName, query);

            // Generate variable test if query has variables
            var variableMatches = Regex.Matches(query, @"\$(\w+):\s*([^,\)]+)", RegexOptions.IgnoreCase);
            if (variableMatches.Count > 0)
            {
                GenerateVariableTest(testCode, framework, operationName, query, variableMatches);
            }

            testCode.AppendLine("    }");
            testCode.AppendLine("}");

            return testCode.ToString();
        }
        catch (Exception ex)
        {
            return $"Error generating tests: {ex.Message}";
        }
    }

    [McpServerTool, Description("Compare different versions of GraphQL schemas for breaking changes")]
    public static async Task<string> CompareSchemas(
        [Description("Original GraphQL endpoint URL")] string originalEndpoint,
        [Description("New GraphQL endpoint URL")] string newEndpoint,
        [Description("HTTP headers for original endpoint (optional)")] string? originalHeaders = null,
        [Description("HTTP headers for new endpoint (optional)")] string? newHeaders = null)
    {
        try
        {
            // Get both schemas
            var originalSchemaJson = await SchemaIntrospectionTools.IntrospectSchema(originalEndpoint, originalHeaders);
            var newSchemaJson = await SchemaIntrospectionTools.IntrospectSchema(newEndpoint, newHeaders);

            var originalSchema = JsonSerializer.Deserialize<JsonElement>(originalSchemaJson);
            var newSchema = JsonSerializer.Deserialize<JsonElement>(newSchemaJson);

            var comparison = new StringBuilder();
            comparison.AppendLine("# GraphQL Schema Comparison Report\n");

            // Compare types
            var originalTypes = ExtractTypesFromSchema(originalSchema);
            var newTypes = ExtractTypesFromSchema(newSchema);

            // Breaking changes
            var breakingChanges = new List<string>();
            var nonBreakingChanges = new List<string>();

            // Removed types (breaking)
            var removedTypes = originalTypes.Keys.Except(newTypes.Keys);
            foreach (var removedType in removedTypes)
            {
                breakingChanges.Add($"🔴 Type `{removedType}` was removed");
            }

            // Added types (non-breaking)
            var addedTypes = newTypes.Keys.Except(originalTypes.Keys);
            foreach (var addedType in addedTypes)
            {
                nonBreakingChanges.Add($"✅ Type `{addedType}` was added");
            }

            // Changed types
            var commonTypes = originalTypes.Keys.Intersect(newTypes.Keys);
            foreach (var typeName in commonTypes)
            {
                var originalType = originalTypes[typeName];
                var newType = newTypes[typeName];

                CompareTypeChanges(typeName, originalType, newType, breakingChanges, nonBreakingChanges);
            }

            // Generate report
            if (breakingChanges.Count > 0)
            {
                comparison.AppendLine("## 🚨 Breaking Changes");
                foreach (var change in breakingChanges)
                {
                    comparison.AppendLine($"- {change}");
                }
                comparison.AppendLine();
            }

            if (nonBreakingChanges.Count > 0)
            {
                comparison.AppendLine("## ✅ Non-Breaking Changes");
                foreach (var change in nonBreakingChanges)
                {
                    comparison.AppendLine($"- {change}");
                }
                comparison.AppendLine();
            }

            if (breakingChanges.Count == 0 && nonBreakingChanges.Count == 0)
            {
                comparison.AppendLine("## ✅ No Changes Detected");
                comparison.AppendLine("The schemas are identical.");
            }
            else
            {
                comparison.AppendLine("## Summary");
                comparison.AppendLine($"- Breaking changes: {breakingChanges.Count}");
                comparison.AppendLine($"- Non-breaking changes: {nonBreakingChanges.Count}");
                
                if (breakingChanges.Count > 0)
                {
                    comparison.AppendLine("\n⚠️ **This schema change contains breaking changes and requires careful deployment planning.**");
                }
            }

            return comparison.ToString();
        }
        catch (Exception ex)
        {
            return $"Error comparing schemas: {ex.Message}";
        }
    }

    private static object GenerateMockInstance(JsonElement type, JsonElement allTypes, int index)
    {
        var mockData = new Dictionary<string, object?>();
        var typeName = type.GetProperty("name").GetString() ?? "";
        var kind = type.GetProperty("kind").GetString();

        if (kind == "OBJECT" || kind == "INPUT_OBJECT")
        {
            var fieldsProperty = kind == "OBJECT" ? "fields" : "inputFields";
            if (type.TryGetProperty(fieldsProperty, out var fields) && fields.ValueKind == JsonValueKind.Array)
            {
                foreach (var field in fields.EnumerateArray())
                {
                    if (field.TryGetProperty("name", out var fieldName) && 
                        field.TryGetProperty("type", out var fieldType))
                    {
                        var fieldNameStr = fieldName.GetString() ?? "";
                        var mockValue = GenerateMockValue(fieldType, allTypes, fieldNameStr, index);
                        mockData[fieldNameStr] = mockValue;
                    }
                }
            }
        }

        return mockData;
    }

    private static object? GenerateMockValue(JsonElement typeElement, JsonElement allTypes, string fieldName, int index)
    {
        var typeInfo = ParseTypeInfo(typeElement);
        
        return typeInfo.BaseType switch
        {
            "String" => GenerateMockString(fieldName, index),
            "Int" => new Random().Next(1, 1000),
            "Float" => Math.Round(new Random().NextDouble() * 1000, 2),
            "Boolean" => index % 2 == 0,
            "ID" => $"id_{index}_{fieldName}",
            _ => GenerateComplexMockValue(typeInfo.BaseType, allTypes, index)
        };
    }

    private static string GenerateMockString(string fieldName, int index)
    {
        return fieldName.ToLower() switch
        {
            var name when name.Contains("email") => $"user{index}@example.com",
            var name when name.Contains("name") => $"MockName{index}",
            var name when name.Contains("title") => $"Mock Title {index}",
            var name when name.Contains("description") => $"This is a mock description for item {index}",
            var name when name.Contains("url") => $"https://example.com/mock{index}",
            var name when name.Contains("phone") => $"+1-555-{index:D4}",
            _ => $"Mock{fieldName}{index}"
        };
    }

    private static object? GenerateComplexMockValue(string typeName, JsonElement allTypes, int index)
    {
        // Find the type in the schema
        foreach (var type in allTypes.EnumerateArray())
        {
            if (type.TryGetProperty("name", out var nameElement) && 
                nameElement.GetString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true)
            {
                if (type.TryGetProperty("kind", out var kind) && kind.GetString() == "ENUM")
                {
                    if (type.TryGetProperty("enumValues", out var enumValues) && enumValues.ValueKind == JsonValueKind.Array)
                    {
                        var values = enumValues.EnumerateArray().ToList();
                        if (values.Count > 0)
                        {
                            var selectedValue = values[index % values.Count];
                            return selectedValue.GetProperty("name").GetString();
                        }
                    }
                }
                else
                {
                    return GenerateMockInstance(type, allTypes, index);
                }
            }
        }

        return null;
    }

    private static (string BaseType, bool IsNonNull, bool IsList) ParseTypeInfo(JsonElement typeElement)
    {
        var isNonNull = false;
        var isList = false;
        var currentType = typeElement;

        if (currentType.TryGetProperty("kind", out var kind) && kind.GetString() == "NON_NULL")
        {
            isNonNull = true;
            currentType = currentType.GetProperty("ofType");
        }

        if (currentType.TryGetProperty("kind", out kind) && kind.GetString() == "LIST")
        {
            isList = true;
            currentType = currentType.GetProperty("ofType");
            
            if (currentType.TryGetProperty("kind", out kind) && kind.GetString() == "NON_NULL")
            {
                currentType = currentType.GetProperty("ofType");
            }
        }

        var baseType = currentType.TryGetProperty("name", out var name) ? name.GetString() ?? "String" : "String";
        
        return (baseType, isNonNull, isList);
    }

    private static void GenerateSetupMethod(StringBuilder testCode, string framework)
    {
        switch (framework.ToLower())
        {
            case "xunit":
                testCode.AppendLine("        private readonly HttpClient _httpClient;");
                testCode.AppendLine("        private readonly string _endpoint = \"https://api.example.com/graphql\";");
                testCode.AppendLine();
                testCode.AppendLine("        public GraphQLQueryTests()");
                testCode.AppendLine("        {");
                testCode.AppendLine("            _httpClient = new HttpClient();");
                testCode.AppendLine("        }");
                break;
            case "nunit":
                testCode.AppendLine("        private HttpClient _httpClient;");
                testCode.AppendLine("        private string _endpoint = \"https://api.example.com/graphql\";");
                testCode.AppendLine();
                testCode.AppendLine("        [SetUp]");
                testCode.AppendLine("        public void Setup()");
                testCode.AppendLine("        {");
                testCode.AppendLine("            _httpClient = new HttpClient();");
                testCode.AppendLine("        }");
                break;
            case "mstest":
                testCode.AppendLine("        private HttpClient _httpClient;");
                testCode.AppendLine("        private string _endpoint = \"https://api.example.com/graphql\";");
                testCode.AppendLine();
                testCode.AppendLine("        [TestInitialize]");
                testCode.AppendLine("        public void TestInitialize()");
                testCode.AppendLine("        {");
                testCode.AppendLine("            _httpClient = new HttpClient();");
                testCode.AppendLine("        }");
                break;
        }
        testCode.AppendLine();
    }

    private static void GenerateSuccessTest(StringBuilder testCode, string framework, string operationName, string query, string operationType)
    {
        var testAttribute = framework.ToLower() switch
        {
            "xunit" => "[Fact]",
            "nunit" => "[Test]",
            "mstest" => "[TestMethod]",
            _ => "[Test]"
        };

        testCode.AppendLine($"        {testAttribute}");
        testCode.AppendLine($"        public async Task {operationName}_ShouldReturnSuccessfulResponse()");
        testCode.AppendLine("        {");
        testCode.AppendLine("            // Arrange");
        testCode.AppendLine($"            var query = @\"{query.Replace("\"", "\"\"")}\";");
        testCode.AppendLine("            var request = new { query };");
        testCode.AppendLine("            var json = JsonSerializer.Serialize(request);");
        testCode.AppendLine("            var content = new StringContent(json, Encoding.UTF8, \"application/json\");");
        testCode.AppendLine();
        testCode.AppendLine("            // Act");
        testCode.AppendLine("            var response = await _httpClient.PostAsync(_endpoint, content);");
        testCode.AppendLine();
        testCode.AppendLine("            // Assert");
        testCode.AppendLine("            response.EnsureSuccessStatusCode();");
        testCode.AppendLine("            var responseContent = await response.Content.ReadAsStringAsync();");
        testCode.AppendLine("            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);");
        testCode.AppendLine();

        var assertion = framework.ToLower() switch
        {
            "xunit" => "            Assert.True(result.TryGetProperty(\"data\", out _));",
            "nunit" => "            Assert.That(result.TryGetProperty(\"data\", out _), Is.True);",
            "mstest" => "            Assert.IsTrue(result.TryGetProperty(\"data\", out _));",
            _ => "            Assert.That(result.TryGetProperty(\"data\", out _), Is.True);"
        };

        testCode.AppendLine(assertion);
        testCode.AppendLine("        }");
        testCode.AppendLine();
    }

    private static void GenerateErrorTest(StringBuilder testCode, string framework, string operationName, string query)
    {
        var testAttribute = framework.ToLower() switch
        {
            "xunit" => "[Fact]",
            "nunit" => "[Test]", 
            "mstest" => "[TestMethod]",
            _ => "[Test]"
        };

        testCode.AppendLine($"        {testAttribute}");
        testCode.AppendLine($"        public async Task {operationName}_WithInvalidQuery_ShouldReturnErrors()");
        testCode.AppendLine("        {");
        testCode.AppendLine("            // Arrange");
        testCode.AppendLine("            var invalidQuery = \"query { invalidField }\";");
        testCode.AppendLine("            var request = new { query = invalidQuery };");
        testCode.AppendLine("            var json = JsonSerializer.Serialize(request);");
        testCode.AppendLine("            var content = new StringContent(json, Encoding.UTF8, \"application/json\");");
        testCode.AppendLine();
        testCode.AppendLine("            // Act");
        testCode.AppendLine("            var response = await _httpClient.PostAsync(_endpoint, content);");
        testCode.AppendLine();
        testCode.AppendLine("            // Assert");
        testCode.AppendLine("            var responseContent = await response.Content.ReadAsStringAsync();");
        testCode.AppendLine("            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);");

        var assertion = framework.ToLower() switch
        {
            "xunit" => "            Assert.True(result.TryGetProperty(\"errors\", out _));",
            "nunit" => "            Assert.That(result.TryGetProperty(\"errors\", out _), Is.True);",
            "mstest" => "            Assert.IsTrue(result.TryGetProperty(\"errors\", out _));",
            _ => "            Assert.That(result.TryGetProperty(\"errors\", out _), Is.True);"
        };

        testCode.AppendLine(assertion);
        testCode.AppendLine("        }");
        testCode.AppendLine();
    }

    private static void GenerateValidationTest(StringBuilder testCode, string framework, string operationName, string query)
    {
        var testAttribute = framework.ToLower() switch
        {
            "xunit" => "[Fact]",
            "nunit" => "[Test]",
            "mstest" => "[TestMethod]",
            _ => "[Test]"
        };

        testCode.AppendLine($"        {testAttribute}");
        testCode.AppendLine($"        public void {operationName}_QuerySyntax_ShouldBeValid()");
        testCode.AppendLine("        {");
        testCode.AppendLine("            // Arrange");
        testCode.AppendLine($"            var query = @\"{query.Replace("\"", "\"\"")}\";");
        testCode.AppendLine();
        testCode.AppendLine("            // Act & Assert");
        testCode.AppendLine("            var openBraces = query.Count(c => c == '{');");
        testCode.AppendLine("            var closeBraces = query.Count(c => c == '}');");

        var assertion = framework.ToLower() switch
        {
            "xunit" => "            Assert.Equal(openBraces, closeBraces);",
            "nunit" => "            Assert.That(openBraces, Is.EqualTo(closeBraces));",
            "mstest" => "            Assert.AreEqual(openBraces, closeBraces);",
            _ => "            Assert.That(openBraces, Is.EqualTo(closeBraces));"
        };

        testCode.AppendLine(assertion);
        testCode.AppendLine("        }");
        testCode.AppendLine();
    }

    private static void GenerateVariableTest(StringBuilder testCode, string framework, string operationName, string query, MatchCollection variableMatches)
    {
        var testAttribute = framework.ToLower() switch
        {
            "xunit" => "[Fact]",
            "nunit" => "[Test]",
            "mstest" => "[TestMethod]",
            _ => "[Test]"
        };

        testCode.AppendLine($"        {testAttribute}");
        testCode.AppendLine($"        public async Task {operationName}_WithVariables_ShouldExecuteSuccessfully()");
        testCode.AppendLine("        {");
        testCode.AppendLine("            // Arrange");
        testCode.AppendLine($"            var query = @\"{query.Replace("\"", "\"\"")}\";");
        testCode.AppendLine("            var variables = new Dictionary<string, object>");
        testCode.AppendLine("            {");

        foreach (Match match in variableMatches)
        {
            var varName = match.Groups[1].Value;
            var varType = match.Groups[2].Value.Trim();
            var mockValue = GenerateMockVariableValue(varType);
            testCode.AppendLine($"                [\"{varName}\"] = {mockValue},");
        }

        testCode.AppendLine("            };");
        testCode.AppendLine("            var request = new { query, variables };");
        testCode.AppendLine("            var json = JsonSerializer.Serialize(request);");
        testCode.AppendLine("            var content = new StringContent(json, Encoding.UTF8, \"application/json\");");
        testCode.AppendLine();
        testCode.AppendLine("            // Act");
        testCode.AppendLine("            var response = await _httpClient.PostAsync(_endpoint, content);");
        testCode.AppendLine();
        testCode.AppendLine("            // Assert");
        testCode.AppendLine("            response.EnsureSuccessStatusCode();");
        testCode.AppendLine("        }");
        testCode.AppendLine();
    }

    private static string GenerateMockVariableValue(string graphqlType)
    {
        var baseType = graphqlType.TrimEnd('!').Replace("[", "").Replace("]", "");
        
        return baseType switch
        {
            "String" => "\"test_value\"",
            "Int" => "42",
            "Float" => "3.14",
            "Boolean" => "true",
            "ID" => "\"test_id\"",
            _ => "\"mock_value\""
        };
    }

    private static Dictionary<string, JsonElement> ExtractTypesFromSchema(JsonElement schema)
    {
        var types = new Dictionary<string, JsonElement>();
        
        if (schema.TryGetProperty("data", out var data) && 
            data.TryGetProperty("__schema", out var schemaNode) &&
            schemaNode.TryGetProperty("types", out var typesArray))
        {
            foreach (var type in typesArray.EnumerateArray())
            {
                if (type.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
                {
                    var typeName = name.GetString();
                    if (typeName != null && !typeName.StartsWith("__"))
                    {
                        types[typeName] = type;
                    }
                }
            }
        }
        
        return types;
    }

    private static void CompareTypeChanges(string typeName, JsonElement originalType, JsonElement newType, 
        List<string> breakingChanges, List<string> nonBreakingChanges)
    {
        // Compare fields for OBJECT types
        if (originalType.TryGetProperty("fields", out var originalFields) && 
            newType.TryGetProperty("fields", out var newFields))
        {
            var originalFieldNames = ExtractFieldNames(originalFields);
            var newFieldNames = ExtractFieldNames(newFields);

            // Removed fields (breaking)
            var removedFields = originalFieldNames.Except(newFieldNames);
            foreach (var removedField in removedFields)
            {
                breakingChanges.Add($"🔴 Field `{typeName}.{removedField}` was removed");
            }

            // Added fields (non-breaking)
            var addedFields = newFieldNames.Except(originalFieldNames);
            foreach (var addedField in addedFields)
            {
                nonBreakingChanges.Add($"✅ Field `{typeName}.{addedField}` was added");
            }
        }

        // Compare enum values for ENUM types
        if (originalType.TryGetProperty("enumValues", out var originalEnumValues) && 
            newType.TryGetProperty("enumValues", out var newEnumValues))
        {
            var originalValueNames = ExtractEnumValueNames(originalEnumValues);
            var newValueNames = ExtractEnumValueNames(newEnumValues);

            // Removed enum values (breaking)
            var removedValues = originalValueNames.Except(newValueNames);
            foreach (var removedValue in removedValues)
            {
                breakingChanges.Add($"🔴 Enum value `{typeName}.{removedValue}` was removed");
            }

            // Added enum values (non-breaking)
            var addedValues = newValueNames.Except(originalValueNames);
            foreach (var addedValue in addedValues)
            {
                nonBreakingChanges.Add($"✅ Enum value `{typeName}.{addedValue}` was added");
            }
        }
    }

    private static List<string> ExtractFieldNames(JsonElement fields)
    {
        var fieldNames = new List<string>();
        foreach (var field in fields.EnumerateArray())
        {
            if (field.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                fieldNames.Add(name.GetString() ?? "");
            }
        }
        return fieldNames;
    }

    private static List<string> ExtractEnumValueNames(JsonElement enumValues)
    {
        var valueNames = new List<string>();
        foreach (var enumValue in enumValues.EnumerateArray())
        {
            if (enumValue.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                valueNames.Add(name.GetString() ?? "");
            }
        }
        return valueNames;
    }
}
