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

    [McpServerTool, Description("Generate comprehensive test suites for GraphQL queries and mutations")]
    public static async Task<string> GenerateTestSuite(
        [Description("GraphQL endpoint URL")] string endpoint,
        [Description("Query or mutation to test")] string query,
        [Description("Test framework (jest, mocha, xunit, nunit)")] string framework = "jest",
        [Description("Include integration tests")] bool includeIntegration = true,
        [Description("Include error case tests")] bool includeErrorCases = true,
        [Description("Include performance tests")] bool includePerformance = false,
        [Description("HTTP headers as JSON object (optional)")] string? headers = null)
    {
        try
        {
            var testSuite = new StringBuilder();
            testSuite.AppendLine("# Generated Test Suite\n");

            // Analyze the query to understand what we're testing
            var operationInfo = AnalyzeOperation(query);
            var operationType = operationInfo.Type;
            var operationName = operationInfo.Name;

            testSuite.AppendLine($"## Test Suite for {operationName} ({operationType})\n");

            switch (framework.ToLower())
            {
                case "jest":
                    testSuite.AppendLine(GenerateJestTests(query, operationInfo, includeIntegration, includeErrorCases, includePerformance));
                    break;
                case "mocha":
                    testSuite.AppendLine(GenerateMochaTests(query, operationInfo, includeIntegration, includeErrorCases, includePerformance));
                    break;
                case "xunit":
                    testSuite.AppendLine(GenerateXUnitTests(query, operationInfo, includeIntegration, includeErrorCases, includePerformance));
                    break;
                case "nunit":
                    testSuite.AppendLine(GenerateNUnitTests(query, operationInfo, includeIntegration, includeErrorCases, includePerformance));
                    break;
                default:
                    return $"Framework '{framework}' not supported. Supported: jest, mocha, xunit, nunit";
            }

            return testSuite.ToString();
        }
        catch (Exception ex)
        {
            return $"Error generating test suite: {ex.Message}";
        }
    }

    [McpServerTool, Description("Generate load testing scenarios for GraphQL endpoints")]
    public static string GenerateLoadTests(
        [Description("GraphQL query for load testing")] string query,
        [Description("Load testing tool (k6, artillery, jmeter)")] string tool = "k6",
        [Description("Number of virtual users")] int virtualUsers = 10,
        [Description("Test duration in seconds")] int duration = 60,
        [Description("Target requests per second")] int rps = 100)
    {
        try
        {
            var loadTest = new StringBuilder();
            loadTest.AppendLine("# GraphQL Load Test Configuration\n");

            switch (tool.ToLower())
            {
                case "k6":
                    loadTest.AppendLine(GenerateK6LoadTest(query, virtualUsers, duration, rps));
                    break;
                case "artillery":
                    loadTest.AppendLine(GenerateArtilleryLoadTest(query, virtualUsers, duration, rps));
                    break;
                case "jmeter":
                    loadTest.AppendLine("## JMeter Configuration");
                    loadTest.AppendLine("*JMeter configuration requires GUI setup. See JMeter documentation for GraphQL testing.*");
                    break;
                default:
                    return $"Load testing tool '{tool}' not supported. Supported: k6, artillery, jmeter";
            }

            return loadTest.ToString();
        }
        catch (Exception ex)
        {
            return $"Error generating load tests: {ex.Message}";
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
                testCode.AppendLine("        public GraphQLQueryTests(IHttpClientFactory httpClientFactory)");
                testCode.AppendLine("        {");
                testCode.AppendLine("            _httpClient = httpClientFactory.CreateClient();");
                testCode.AppendLine("        }");
                break;
            case "nunit":
                testCode.AppendLine("        private HttpClient _httpClient;");
                testCode.AppendLine("        private string _endpoint = \"https://api.example.com/graphql\";");
                testCode.AppendLine("        private IHttpClientFactory _httpClientFactory;");
                testCode.AppendLine();
                testCode.AppendLine("        [SetUp]");
                testCode.AppendLine("        public void Setup()");
                testCode.AppendLine("        {");
                testCode.AppendLine("            // In real test, inject IHttpClientFactory through DI");
                testCode.AppendLine("            _httpClient = _httpClientFactory.CreateClient();");
                testCode.AppendLine("        }");
                break;
            case "mstest":
                testCode.AppendLine("        private HttpClient _httpClient;");
                testCode.AppendLine("        private string _endpoint = \"https://api.example.com/graphql\";");
                testCode.AppendLine("        private IHttpClientFactory _httpClientFactory;");
                testCode.AppendLine();
                testCode.AppendLine("        [TestInitialize]");
                testCode.AppendLine("        public void TestInitialize()");
                testCode.AppendLine("        {");
                testCode.AppendLine("            // In real test, inject IHttpClientFactory through DI");
                testCode.AppendLine("            _httpClient = _httpClientFactory.CreateClient();");
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

    private static (string Type, string Name) AnalyzeOperation(string query)
    {
        // Basic analysis to determine operation type and name
        var operationMatch = Regex.Match(query, @"^\s*(query|mutation|subscription)\s+(\w+)?", RegexOptions.IgnoreCase);
        var operationType = operationMatch.Success ? operationMatch.Groups[1].Value.ToLower() : "query";
        var operationName = operationMatch.Success && operationMatch.Groups[2].Success ? 
            operationMatch.Groups[2].Value : "AnonymousOperation";

        return (operationType, operationName);
    }

    private static string GenerateJestTests(string query, (string Type, string Name) operationInfo, bool includeIntegration, bool includeErrorCases, bool includePerformance)
    {
        var tests = new StringBuilder();

        // Basic Jest test structure
        tests.AppendLine("import { request, gql } from 'graphql-request';");
        tests.AppendLine("import { createServer } from 'http';");
        tests.AppendLine();
        tests.AppendLine("const endpoint = 'https://api.example.com/graphql';");
        tests.AppendLine();
        tests.AppendLine($"describe('GraphQL {operationInfo.Type} - {operationInfo.Name}', () => {{");
        tests.AppendLine("  let server;");
        tests.AppendLine();
        tests.AppendLine("  beforeAll(async () => {");
        tests.AppendLine("    server = createServer();");
        tests.AppendLine("    await server.start();");
        tests.AppendLine("  });");
        tests.AppendLine();
        tests.AppendLine("  afterAll(async () => {");
        tests.AppendLine("    await server.stop();");
        tests.AppendLine("  });");
        tests.AppendLine();
        tests.AppendLine("  it('should return a successful response', async () => {");
        tests.AppendLine("    const query = `");
        tests.AppendLine(query.Replace("`", "\\`").Replace("${", "\\${"));
        tests.AppendLine("    `;");
        tests.AppendLine("    const response = await request(endpoint, query);");
        tests.AppendLine();
        tests.AppendLine("    expect(response).toHaveProperty('data');");
        tests.AppendLine("  });");
        tests.AppendLine();
        
        // Error case test
        if (includeErrorCases)
        {
            tests.AppendLine("  it('should handle errors gracefully', async () => {");
            tests.AppendLine("    const query = `query { invalidField }`;");
            tests.AppendLine("    try {");
            tests.AppendLine("      await request(endpoint, query);");
            tests.AppendLine("      fail('Expected an error to be thrown');");
            tests.AppendLine("    } catch (error) {");
            tests.AppendLine("      expect(error).toBeDefined();");
            tests.AppendLine("      expect(error.response.errors).toBeDefined();");
            tests.AppendLine("    }");
            tests.AppendLine("  });");
            tests.AppendLine();
        }

        // Performance test
        if (includePerformance)
        {
            tests.AppendLine("  it('should complete within acceptable time', async () => {");
            tests.AppendLine("    const query = `");
            tests.AppendLine(query.Replace("`", "\\`").Replace("${", "\\${"));
            tests.AppendLine("    `;");
            tests.AppendLine("    const start = Date.now();");
            tests.AppendLine("    await request(endpoint, query);");
            tests.AppendLine("    const duration = Date.now() - start;");
            tests.AppendLine();
            tests.AppendLine("    expect(duration).toBeLessThan(1000); // 1 second");
            tests.AppendLine("  });");
            tests.AppendLine();
        }

        tests.AppendLine("});");
        return tests.ToString();
    }

    private static string GenerateMochaTests(string query, (string Type, string Name) operationInfo, bool includeIntegration, bool includeErrorCases, bool includePerformance)
    {
        var tests = new StringBuilder();
        
        tests.AppendLine("const { request, gql } = require('graphql-request');");
        tests.AppendLine("const { expect } = require('chai');");
        tests.AppendLine();
        tests.AppendLine("const endpoint = 'https://api.example.com/graphql';");
        tests.AppendLine();
        tests.AppendLine($"describe('GraphQL {operationInfo.Type} - {operationInfo.Name}', () => {{");
        tests.AppendLine("  it('should return a successful response', async () => {");
        tests.AppendLine("    const query = `");
        tests.AppendLine(query.Replace("`", "\\`"));
        tests.AppendLine("    `;");
        tests.AppendLine("    const response = await request(endpoint, query);");
        tests.AppendLine();
        tests.AppendLine("    expect(response).to.have.property('data');");
        tests.AppendLine("  });");

        if (includeErrorCases)
        {
            tests.AppendLine();
            tests.AppendLine("  it('should handle errors gracefully', async () => {");
            tests.AppendLine("    const query = `query { invalidField }`;");
            tests.AppendLine("    try {");
            tests.AppendLine("      await request(endpoint, query);");
            tests.AppendLine("      throw new Error('Expected an error');");
            tests.AppendLine("    } catch (error) {");
            tests.AppendLine("      expect(error).to.exist;");
            tests.AppendLine("    }");
            tests.AppendLine("  });");
        }

        tests.AppendLine("});");
        return tests.ToString();
    }

    private static string GenerateXUnitTests(string query, (string Type, string Name) operationInfo, bool includeIntegration, bool includeErrorCases, bool includePerformance)
    {
        var tests = new StringBuilder();
        
        tests.AppendLine("using System;");
        tests.AppendLine("using System.Net.Http;");
        tests.AppendLine("using System.Text;");
        tests.AppendLine("using System.Text.Json;");
        tests.AppendLine("using System.Threading.Tasks;");
        tests.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        tests.AppendLine("using Xunit;");
        tests.AppendLine();
        tests.AppendLine($"public class {operationInfo.Name}Tests");
        tests.AppendLine("{");
        tests.AppendLine("    private readonly HttpClient _httpClient;");
        tests.AppendLine("    private readonly string _endpoint = \"https://api.example.com/graphql\";");
        tests.AppendLine();
        tests.AppendLine($"    public {operationInfo.Name}Tests(IHttpClientFactory httpClientFactory)");
        tests.AppendLine("    {");
        tests.AppendLine("        _httpClient = httpClientFactory.CreateClient();");
        tests.AppendLine("    }");
        tests.AppendLine();
        tests.AppendLine("    [Fact]");
        tests.AppendLine("    public async Task Should_Return_Successful_Response()");
        tests.AppendLine("    {");
        tests.AppendLine("        // Arrange");
        tests.AppendLine($"        var query = @\"{query.Replace("\"", "\"\"")}\";");
        tests.AppendLine("        var request = new { query };");
        tests.AppendLine("        var json = JsonSerializer.Serialize(request);");
        tests.AppendLine("        var content = new StringContent(json, Encoding.UTF8, \"application/json\");");
        tests.AppendLine();
        tests.AppendLine("        // Act");
        tests.AppendLine("        var response = await _httpClient.PostAsync(_endpoint, content);");
        tests.AppendLine();
        tests.AppendLine("        // Assert");
        tests.AppendLine("        response.EnsureSuccessStatusCode();");
        tests.AppendLine("        var responseContent = await response.Content.ReadAsStringAsync();");
        tests.AppendLine("        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);");
        tests.AppendLine("        Assert.True(result.TryGetProperty(\"data\", out _));");
        tests.AppendLine("    }");

        if (includeErrorCases)
        {
            tests.AppendLine();
            tests.AppendLine("    [Fact]");
            tests.AppendLine("    public async Task Should_Handle_Errors_Gracefully()");
            tests.AppendLine("    {");
            tests.AppendLine("        // Arrange");
            tests.AppendLine("        var query = \"query { invalidField }\";");
            tests.AppendLine("        var request = new { query };");
            tests.AppendLine("        var json = JsonSerializer.Serialize(request);");
            tests.AppendLine("        var content = new StringContent(json, Encoding.UTF8, \"application/json\");");
            tests.AppendLine();
            tests.AppendLine("        // Act");
            tests.AppendLine("        var response = await _httpClient.PostAsync(_endpoint, content);");
            tests.AppendLine("        var responseContent = await response.Content.ReadAsStringAsync();");
            tests.AppendLine("        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);");
            tests.AppendLine();
            tests.AppendLine("        // Assert");
            tests.AppendLine("        Assert.True(result.TryGetProperty(\"errors\", out _));");
            tests.AppendLine("    }");
        }

        tests.AppendLine("}");
        return tests.ToString();
    }

    private static string GenerateNUnitTests(string query, (string Type, string Name) operationInfo, bool includeIntegration, bool includeErrorCases, bool includePerformance)
    {
        var tests = new StringBuilder();
        
        tests.AppendLine("using System;");
        tests.AppendLine("using System.Net.Http;");
        tests.AppendLine("using System.Text;");
        tests.AppendLine("using System.Text.Json;");
        tests.AppendLine("using System.Threading.Tasks;");
        tests.AppendLine("using NUnit.Framework;");
        tests.AppendLine();
        tests.AppendLine("[TestFixture]");
        tests.AppendLine($"public class {operationInfo.Name}Tests");
        tests.AppendLine("{");
        tests.AppendLine("    private HttpClient _httpClient;");
        tests.AppendLine("    private readonly string _endpoint = \"https://api.example.com/graphql\";");
        tests.AppendLine("    private IHttpClientFactory _httpClientFactory;");
        tests.AppendLine();
        tests.AppendLine("    [SetUp]");
        tests.AppendLine("    public void SetUp()");
        tests.AppendLine("    {");
        tests.AppendLine("        // In real test, inject IHttpClientFactory through DI");
        tests.AppendLine("        _httpClient = _httpClientFactory.CreateClient();");
        tests.AppendLine("    }");
        tests.AppendLine();
        tests.AppendLine("    [TearDown]");
        tests.AppendLine("    public void TearDown()");
        tests.AppendLine("    {");
        tests.AppendLine("        _httpClient?.Dispose();");
        tests.AppendLine("    }");
        tests.AppendLine();
        tests.AppendLine("    [Test]");
        tests.AppendLine("    public async Task Should_Return_Successful_Response()");
        tests.AppendLine("    {");
        tests.AppendLine("        // Arrange");
        tests.AppendLine($"        var query = @\"{query.Replace("\"", "\"\"")}\";");
        tests.AppendLine("        var request = new { query };");
        tests.AppendLine("        var json = JsonSerializer.Serialize(request);");
        tests.AppendLine("        var content = new StringContent(json, Encoding.UTF8, \"application/json\");");
        tests.AppendLine();
        tests.AppendLine("        // Act");
        tests.AppendLine("        var response = await _httpClient.PostAsync(_endpoint, content);");
        tests.AppendLine();
        tests.AppendLine("        // Assert");
        tests.AppendLine("        Assert.That(response.IsSuccessStatusCode, Is.True);");
        tests.AppendLine("        var responseContent = await response.Content.ReadAsStringAsync();");
        tests.AppendLine("        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);");
        tests.AppendLine("        Assert.That(result.TryGetProperty(\"data\", out _), Is.True);");
        tests.AppendLine("    }");
        tests.AppendLine("}");
        
        return tests.ToString();
    }

    private static string GenerateK6LoadTest(string query, int virtualUsers, int duration, int rps)
    {
        var loadTest = new StringBuilder();
        
        loadTest.AppendLine("## K6 Load Test Script");
        loadTest.AppendLine();
        loadTest.AppendLine("```javascript");
        loadTest.AppendLine("import http from 'k6/http';");
        loadTest.AppendLine("import { check, sleep } from 'k6';");
        loadTest.AppendLine();
        loadTest.AppendLine("export let options = {");
        loadTest.AppendLine($"  vus: {virtualUsers},");
        loadTest.AppendLine($"  duration: '{duration}s',");
        loadTest.AppendLine("  thresholds: {");
        loadTest.AppendLine("    http_req_duration: ['p(95)<500'],");
        loadTest.AppendLine("    http_req_failed: ['rate<0.1'],");
        loadTest.AppendLine("  },");
        loadTest.AppendLine("};");
        loadTest.AppendLine();
        loadTest.AppendLine("export default function () {");
        loadTest.AppendLine("  const url = 'https://api.example.com/graphql';");
        loadTest.AppendLine("  const payload = JSON.stringify({");
        loadTest.AppendLine($"    query: `{query.Replace("`", "\\`")}`,");
        loadTest.AppendLine("  });");
        loadTest.AppendLine();
        loadTest.AppendLine("  const params = {");
        loadTest.AppendLine("    headers: {");
        loadTest.AppendLine("      'Content-Type': 'application/json',");
        loadTest.AppendLine("    },");
        loadTest.AppendLine("  };");
        loadTest.AppendLine();
        loadTest.AppendLine("  const response = http.post(url, payload, params);");
        loadTest.AppendLine();
        loadTest.AppendLine("  check(response, {");
        loadTest.AppendLine("    'status is 200': (r) => r.status === 200,");
        loadTest.AppendLine("    'response has data': (r) => JSON.parse(r.body).data !== undefined,");
        loadTest.AppendLine("  });");
        loadTest.AppendLine();
        loadTest.AppendLine($"  sleep(1 / {rps}); // Rate limiting");
        loadTest.AppendLine("}");
        loadTest.AppendLine("```");
        
        return loadTest.ToString();
    }

    private static string GenerateArtilleryLoadTest(string query, int virtualUsers, int duration, int rps)
    {
        var loadTest = new StringBuilder();
        
        loadTest.AppendLine("## Artillery Load Test Configuration");
        loadTest.AppendLine();
        loadTest.AppendLine("```yaml");
        loadTest.AppendLine("config:");
        loadTest.AppendLine("  target: 'https://api.example.com'");
        loadTest.AppendLine("  phases:");
        loadTest.AppendLine("    - duration: 60");
        loadTest.AppendLine($"      arrivalRate: {rps}");
        loadTest.AppendLine($"      maxVusers: {virtualUsers}");
        loadTest.AppendLine("  defaults:");
        loadTest.AppendLine("    headers:");
        loadTest.AppendLine("      Content-Type: 'application/json'");
        loadTest.AppendLine();
        loadTest.AppendLine("scenarios:");
        loadTest.AppendLine("  - name: 'GraphQL Load Test'");
        loadTest.AppendLine("    requests:");
        loadTest.AppendLine("      - post:");
        loadTest.AppendLine("          url: '/graphql'");
        loadTest.AppendLine("          json:");
        loadTest.AppendLine($"            query: |");
        foreach (var line in query.Split('\n'))
        {
            loadTest.AppendLine($"              {line}");
        }
        loadTest.AppendLine("```");
        
        return loadTest.ToString();
    }
}
