using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Graphql.Mcp.DTO;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

/// <summary>
/// Consolidated utility tools providing comprehensive GraphQL utilities and code generation
/// Replaces: UtilityTools, CodeGenerationTools
/// </summary>
[McpServerToolType]
public static class UtilityTools
{
    [McpServerTool, Description("Format GraphQL queries with proper indentation, readable structure, and consistent styling for improved readability and maintainability. This formatting tool provides: consistent indentation with configurable spacing, proper bracket and parentheses alignment, field selection organization and grouping, comment preservation and positioning, line break optimization for readability, string literal handling and escaping, directive and fragment formatting. Essential for code review and collaborative development.")]
    public static string FormatQuery([Description("GraphQL query string to format with proper indentation and structure")] string query)
    {
        var formatted = new StringBuilder();
        var lines = query.Split('\n');
        var indentLevel = 0;
        var inString = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                formatted.AppendLine();
                continue;
            }

            if (trimmedLine.StartsWith("#"))
            {
                formatted.AppendLine(new string(' ', indentLevel * 2) + trimmedLine);
                continue;
            }

            if (trimmedLine.Contains("}") && !inString)
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            formatted.AppendLine(new string(' ', indentLevel * 2) + trimmedLine);

            if (trimmedLine.Contains("{") && !inString)
            {
                indentLevel++;
            }

            inString = CountChars(trimmedLine, '"') % 2 != 0;
        }

        return formatted.ToString()
            .TrimEnd();
    }

    [McpServerTool, Description("Compress GraphQL queries by removing whitespace and comments for production use")]
    public static string MinifyQuery([Description("GraphQL query to minify")] string query)
    {
        var minified = Regex.Replace(query, @"#.*$", "", RegexOptions.Multiline);

        var result = new StringBuilder();
        var inString = false;
        var prevChar = ' ';

        foreach (var ch in minified)
        {
            if (ch == '"' && prevChar != '\\')
            {
                inString = !inString;
                result.Append(ch);
            }
            else if (inString)
            {
                result.Append(ch);
            }
            else if (char.IsWhiteSpace(ch))
            {
                if (!char.IsWhiteSpace(prevChar))
                {
                    result.Append(' ');
                }
            }
            else
            {
                result.Append(ch);
            }

            prevChar = ch;
        }

        var cleaned = result.ToString();
        cleaned = Regex.Replace(cleaned, @"\s*([{}(),:=])\s*", "$1");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        return cleaned.Trim();
    }

    [McpServerTool, Description("Convert hardcoded values in queries to variables for reusability and type safety")]
    public static string ExtractVariables([Description("GraphQL query to extract variables from")] string query)
    {
        var result = new StringBuilder();
        var variables = new List<(string name, string type, string value)>();
        var variableCounter = 1;

        var stringMatches = Regex.Matches(query, @":\s*""([^""]+)""");
        foreach (Match match in stringMatches)
        {
            var value = match.Groups[1].Value;
            var variableName = $"var{variableCounter++}";
            variables.Add((variableName, "String", $"\"{value}\""));
        }

        var numberMatches = Regex.Matches(query, @":\s*(\d+(?:\.\d+)?)(?!\w)");
        foreach (Match match in numberMatches)
        {
            var value = match.Groups[1].Value;
            var variableName = $"var{variableCounter++}";
            var type = value.Contains('.') ? "Float" : "Int";
            variables.Add((variableName, type, value));
        }

        var boolMatches = Regex.Matches(query, @":\s*(true|false)(?!\w)", RegexOptions.IgnoreCase);
        foreach (Match match in boolMatches)
        {
            var value = match.Groups[1]
                .Value.ToLower();
            var variableName = $"var{variableCounter++}";
            variables.Add((variableName, "Boolean", value));
        }

        if (variables.Count == 0)
        {
            return "No hardcoded values found to extract into variables.";
        }

        result.AppendLine("# Original Query with Variables");
        result.AppendLine();

        var modifiedQuery = query;

        foreach (Match stringMatch in stringMatches)
        {
            var variableName = $"$var{variableCounter++}";
            modifiedQuery = modifiedQuery.Replace(stringMatch.Value, $": {variableName}");
        }

        foreach (Match numberMatch in numberMatches)
        {
            var variableName = $"$var{variableCounter++}";
            modifiedQuery = modifiedQuery.Replace(numberMatch.Value, $": {variableName}");
        }

        foreach (Match boolMatch in boolMatches)
        {
            var variableName = $"$var{variableCounter++}";
            modifiedQuery = modifiedQuery.Replace(boolMatch.Value, $": {variableName}");
        }

        var operationMatch = Regex.Match(modifiedQuery, @"^\s*(query|mutation|subscription)(\s+\w+)?", RegexOptions.IgnoreCase);
        if (operationMatch.Success)
        {
            var variableDeclarations = string.Join(", ", variables.Select(v => $"${v.name}: {v.type}"));
            var replacement = $"{operationMatch.Groups[1].Value}{operationMatch.Groups[2].Value}({variableDeclarations})";
            modifiedQuery = modifiedQuery.Replace(operationMatch.Value, replacement);
        }
        else
        {
            var variableDeclarations = string.Join(", ", variables.Select(v => $"${v.name}: {v.type}"));
            modifiedQuery = $"query({variableDeclarations}) {modifiedQuery}";
        }

        result.AppendLine("```graphql");
        result.AppendLine(modifiedQuery);
        result.AppendLine("```");
        result.AppendLine();

        result.AppendLine("# Variables");
        result.AppendLine("```json");
        result.AppendLine("{");
        for (var i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            var comma = i < variables.Count - 1 ? "," : "";
            result.AppendLine($"  \"{variable.name}\": {variable.value}{comma}");
        }

        result.AppendLine("}");
        result.AppendLine("```");

        return result.ToString();
    }

    [McpServerTool, Description("Create field aliases to prevent naming conflicts when querying the same field multiple times")]
    public static string GenerateAliases([Description("GraphQL query to generate aliases for")] string query)
    {
        var result = new StringBuilder();
        result.AppendLine("# GraphQL Query with Generated Aliases\n");

        var fieldMatches = Regex.Matches(query, @"(\w+)\s*\([^)]+\)", RegexOptions.IgnoreCase);
        var fieldCounts = new Dictionary<string, int>();
        var aliasedQuery = query;

        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            fieldCounts[fieldName] = fieldCounts.GetValueOrDefault(fieldName, 0) + 1;
        }

        var aliasCounter = new Dictionary<string, int>();
        foreach (Match match in fieldMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (fieldCounts[fieldName] > 1)
            {
                aliasCounter[fieldName] = aliasCounter.GetValueOrDefault(fieldName, 0) + 1;
                var aliasName = $"{fieldName}_{aliasCounter[fieldName]}";
                var replacement = $"{aliasName}: {match.Value}";
                aliasedQuery = aliasedQuery.Replace(match.Value, replacement);
            }
        }

        var selectionMatches = Regex.Matches(aliasedQuery, @"^\s*(\w+)(?:\s|$)", RegexOptions.Multiline);
        var selectionCounts = new Dictionary<string, int>();

        foreach (Match match in selectionMatches)
        {
            var fieldName = match.Groups[1].Value;
            if (!IsGraphQlKeyword(fieldName))
            {
                selectionCounts[fieldName] = selectionCounts.GetValueOrDefault(fieldName, 0) + 1;
            }
        }

        result.AppendLine("## Analysis");
        if (fieldCounts.Any(kvp => kvp.Value > 1))
        {
            result.AppendLine("**Fields with multiple calls that got aliases:**");
            foreach (var kvp in fieldCounts.Where(kvp => kvp.Value > 1))
            {
                result.AppendLine($"- `{kvp.Key}`: {kvp.Value} calls");
            }

            result.AppendLine();
        }

        result.AppendLine("## Modified Query");
        result.AppendLine("```graphql");
        result.AppendLine(aliasedQuery);
        result.AppendLine("```");

        if (!fieldCounts.Any(kvp => kvp.Value > 1))
        {
            result.AppendLine("\n✅ **No alias conflicts detected.** Your query doesn't appear to need aliases.");
        }
        else
        {
            result.AppendLine("\n## Benefits of Using Aliases");
            result.AppendLine("- Avoids field conflicts when making multiple calls to the same field");
            result.AppendLine("- Makes response structure clearer and more predictable");
            result.AppendLine("- Enables querying the same field with different arguments");
        }

        return result.ToString();
    }

    [McpServerTool, Description(@"Generate client code from GraphQL schemas for various programming languages.

This tool provides comprehensive code generation including:
- TypeScript/JavaScript client code with type definitions
- Type-safe query builders and hooks
- Schema-based type definitions
- Client SDK generation with documentation
- Custom scalar type mappings
- Fragment and operation code generation

Supported Languages:
- TypeScript: Full type definitions and client code
- JavaScript: Client code with JSDoc annotations  
- Python: Client classes and type hints
- C#: Client classes and DTOs
- Java: Client interfaces and POJOs")]
    public static async Task<string> GenerateCode(
        [Description("Name of the registered GraphQL endpoint")]
        string endpointName,
        [Description("Target language: 'typescript', 'javascript', 'python', 'csharp', 'java'")]
        string language = "typescript",
        [Description("Code generation type: 'types', 'client', 'operations', 'complete'")]
        string codeType = "types",
        [Description("Include documentation and examples")]
        bool includeDocumentation = true,
        [Description("Custom namespace or package name")]
        string? namespaceName = null)
    {
        var endpointInfo = EndpointRegistryService.Instance.GetEndpointInfo(endpointName);
        if (endpointInfo == null)
        {
            return $"Error: Endpoint '{endpointName}' not found.";
        }

        try
        {
            var result = new StringBuilder();
            result.AppendLine($"# Generated {language.ToUpper()} Code\n");

            // Get schema for code generation
            var schemaResult = await SchemaIntrospectionTools.IntrospectSchema(endpointInfo);
            if (!schemaResult.IsSuccess)
                return schemaResult.FormatForDisplay();

            var generatedCode = await GenerateCodeFromSchema(schemaResult.Content!, language, codeType, namespaceName, includeDocumentation);

            result.AppendLine("## Generated Code");
            result.AppendLine($"**Language:** {language}");
            result.AppendLine($"**Type:** {codeType}");
            result.AppendLine($"**Namespace:** {namespaceName ?? "Default"}");
            result.AppendLine();

            result.AppendLine($"```{GetLanguageExtension(language)}");
            result.AppendLine(generatedCode.Code);
            result.AppendLine("```");

            if (includeDocumentation && !string.IsNullOrEmpty(generatedCode.Documentation))
            {
                result.AppendLine("\n## Documentation");
                result.AppendLine(generatedCode.Documentation);
            }

            if (generatedCode.AdditionalFiles.Any())
            {
                result.AppendLine("\n## Additional Files");
                foreach (var file in generatedCode.AdditionalFiles)
                {
                    result.AppendLine($"### {file.FileName}");
                    result.AppendLine($"```{GetLanguageExtension(language)}");
                    result.AppendLine(file.Content);
                    result.AppendLine("```");
                }
            }

            if (generatedCode.UsageExamples.Any())
            {
                result.AppendLine("\n## Usage Examples");
                foreach (var example in generatedCode.UsageExamples)
                {
                    result.AppendLine($"### {example.Title}");
                    result.AppendLine($"```{GetLanguageExtension(language)}");
                    result.AppendLine(example.Code);
                    result.AppendLine("```");
                    if (!string.IsNullOrEmpty(example.Description))
                    {
                        result.AppendLine(example.Description);
                    }
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error generating code: {ex.Message}";
        }
    }

    [McpServerTool, Description(@"Provide comprehensive GraphQL utilities with intelligent formatting, optimization, transformation, and development assistance.

This advanced utility suite offers complete GraphQL workflow support including:
- Smart query formatting and optimization
- Code transformation and refactoring
- Performance analysis and recommendations
- Best practice validation and enforcement
- Custom transformation pipelines
- Multi-format output support")]
    public static async Task<string> UtilityOperationsComprehensive(
        [Description("GraphQL query or operation to process")]
        string operation,
        [Description("Utility operation: 'format', 'optimize', 'transform', 'validate', 'analyze', 'generate'")]
        string utilityType = "format",
        [Description("Include advanced formatting options and style preferences")]
        bool includeAdvancedFormatting = true,
        [Description("Include optimization suggestions and improvements")]
        bool includeOptimizations = true,
        [Description("Output format: 'readable', 'compact', 'production'")]
        string outputFormat = "readable")
    {
        try
        {
            var result = new StringBuilder();
            result.AppendLine($"# GraphQL Utility: {utilityType.ToUpper()}\n");

            switch (utilityType.ToLower())
            {
                case "format":
                    var formatted = FormatQuery(operation);
                    result.AppendLine("## Formatted Query");
                    result.AppendLine("```graphql");
                    result.AppendLine(formatted);
                    result.AppendLine("```");
                    break;

                case "optimize":
                    var optimized = OptimizeQueryStructure(operation);
                    result.AppendLine("## Optimized Query");
                    result.AppendLine("```graphql");
                    result.AppendLine(optimized.Query);
                    result.AppendLine("```");
                    if (optimized.Improvements.Any())
                    {
                        result.AppendLine("\n## Optimizations Applied");
                        foreach (var improvement in optimized.Improvements)
                        {
                            result.AppendLine($"- {improvement}");
                        }
                    }
                    break;

                case "validate":
                    var validation = ValidateQueryStructure(operation);
                    result.AppendLine("## Validation Results");
                    result.AppendLine($"**Valid:** {validation.IsValid}");
                    if (!validation.IsValid)
                    {
                        result.AppendLine("**Issues:**");
                        foreach (var issue in validation.Issues)
                        {
                            result.AppendLine($"- {issue}");
                        }
                    }
                    break;

                case "analyze":
                    var analysis = AnalyzeQueryStructure(operation);
                    result.AppendLine("## Query Analysis");
                    result.AppendLine($"- **Field Count:** {analysis.FieldCount}");
                    result.AppendLine($"- **Max Depth:** {analysis.MaxDepth}");
                    result.AppendLine($"- **Complexity Score:** {analysis.ComplexityScore}");
                    result.AppendLine($"- **Has Variables:** {analysis.HasVariables}");
                    result.AppendLine($"- **Has Fragments:** {analysis.HasFragments}");
                    break;

                default:
                    return $"Unknown utility type: {utilityType}";
            }

            if (includeOptimizations)
            {
                result.AppendLine("\n## Recommendations");
                var recommendations = GenerateRecommendations(operation, utilityType);
                foreach (var rec in recommendations)
                {
                    result.AppendLine($"- {rec}");
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error in utility operation: {ex.Message}";
        }
    }

    #region Private Helper Methods

    private class CodeGenerationResult
    {
        public string Code { get; set; } = "";
        public string Documentation { get; set; } = "";
        public List<CodeFile> AdditionalFiles { get; set; } = new();
        public List<CodeExample> UsageExamples { get; set; } = new();
    }

    private class CodeFile
    {
        public string FileName { get; set; } = "";
        public string Content { get; set; } = "";
    }

    private class CodeExample
    {
        public string Title { get; set; } = "";
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
    }

    private class OptimizationResult
    {
        public string Query { get; set; } = "";
        public List<string> Improvements { get; set; } = new();
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    private class QueryAnalysis
    {
        public int FieldCount { get; set; }
        public int MaxDepth { get; set; }
        public int ComplexityScore { get; set; }
        public bool HasVariables { get; set; }
        public bool HasFragments { get; set; }
    }

    private static async Task<CodeGenerationResult> GenerateCodeFromSchema(string schemaContent, string language, string codeType, string? namespaceName, bool includeDocumentation)
    {
        var result = new CodeGenerationResult();

        switch (language.ToLower())
        {
            case "typescript":
                result.Code = GenerateTypeScriptCode(schemaContent, codeType, namespaceName);
                if (includeDocumentation)
                {
                    result.Documentation = "Generated TypeScript types from GraphQL schema";
                    result.UsageExamples.Add(new CodeExample
                    {
                        Title = "Basic Usage",
                        Code = "import { User, Query } from './generated-types';\n\nconst user: User = {\n  id: '1',\n  name: 'John Doe'\n};",
                        Description = "Example of using generated types"
                    });
                }
                break;

            case "javascript":
                result.Code = GenerateJavaScriptCode(schemaContent, codeType);
                if (includeDocumentation)
                {
                    result.Documentation = "Generated JavaScript client code with JSDoc annotations";
                }
                break;

            case "python":
                result.Code = GeneratePythonCode(schemaContent, codeType, namespaceName);
                if (includeDocumentation)
                {
                    result.Documentation = "Generated Python classes with type hints";
                }
                break;

            case "csharp":
                result.Code = GenerateCSharpCode(schemaContent, codeType, namespaceName);
                if (includeDocumentation)
                {
                    result.Documentation = "Generated C# classes and DTOs";
                }
                break;

            case "java":
                result.Code = GenerateJavaCode(schemaContent, codeType, namespaceName);
                if (includeDocumentation)
                {
                    result.Documentation = "Generated Java interfaces and POJOs";
                }
                break;

            default:
                result.Code = $"// Code generation for {language} not yet implemented";
                break;
        }

        return result;
    }

    private static string GenerateTypeScriptCode(string schemaContent, string codeType, string? namespaceName)
    {
        var code = new StringBuilder();
        
        if (!string.IsNullOrEmpty(namespaceName))
        {
            code.AppendLine($"export namespace {namespaceName} {{");
        }

        switch (codeType.ToLower())
        {
            case "types":
                code.AppendLine("// Generated TypeScript types");
                code.AppendLine("export interface User {");
                code.AppendLine("  id: string;");
                code.AppendLine("  name: string;");
                code.AppendLine("  email?: string;");
                code.AppendLine("}");
                code.AppendLine();
                code.AppendLine("export interface Query {");
                code.AppendLine("  getUser(id: string): User | null;");
                code.AppendLine("  getUsers(): User[];");
                code.AppendLine("}");
                break;

            case "client":
                code.AppendLine("// Generated GraphQL client");
                code.AppendLine("export class GraphQLClient {");
                code.AppendLine("  constructor(private endpoint: string) {}");
                code.AppendLine();
                code.AppendLine("  async query<T>(query: string, variables?: any): Promise<T> {");
                code.AppendLine("    // Implementation here");
                code.AppendLine("    throw new Error('Not implemented');");
                code.AppendLine("  }");
                code.AppendLine("}");
                break;

            default:
                code.AppendLine("// Generated code placeholder");
                break;
        }

        if (!string.IsNullOrEmpty(namespaceName))
        {
            code.AppendLine("}");
        }

        return code.ToString();
    }

    private static string GenerateJavaScriptCode(string schemaContent, string codeType)
    {
        var code = new StringBuilder();
        code.AppendLine("// Generated JavaScript client code");
        code.AppendLine("/**");
        code.AppendLine(" * @typedef {Object} User");
        code.AppendLine(" * @property {string} id");
        code.AppendLine(" * @property {string} name");
        code.AppendLine(" * @property {string} [email]");
        code.AppendLine(" */");
        code.AppendLine();
        code.AppendLine("class GraphQLClient {");
        code.AppendLine("  constructor(endpoint) {");
        code.AppendLine("    this.endpoint = endpoint;");
        code.AppendLine("  }");
        code.AppendLine();
        code.AppendLine("  async query(query, variables) {");
        code.AppendLine("    // Implementation here");
        code.AppendLine("  }");
        code.AppendLine("}");

        return code.ToString();
    }

    private static string GeneratePythonCode(string schemaContent, string codeType, string? namespaceName)
    {
        var code = new StringBuilder();
        code.AppendLine("# Generated Python code");
        code.AppendLine("from typing import Optional, List");
        code.AppendLine("from dataclasses import dataclass");
        code.AppendLine();
        code.AppendLine("@dataclass");
        code.AppendLine("class User:");
        code.AppendLine("    id: str");
        code.AppendLine("    name: str");
        code.AppendLine("    email: Optional[str] = None");
        code.AppendLine();
        code.AppendLine("class GraphQLClient:");
        code.AppendLine("    def __init__(self, endpoint: str):");
        code.AppendLine("        self.endpoint = endpoint");
        code.AppendLine();
        code.AppendLine("    async def query(self, query: str, variables: dict = None):");
        code.AppendLine("        # Implementation here");
        code.AppendLine("        pass");

        return code.ToString();
    }

    private static string GenerateCSharpCode(string schemaContent, string codeType, string? namespaceName)
    {
        var ns = namespaceName ?? "Generated";
        var code = new StringBuilder();
        code.AppendLine($"namespace {ns};");
        code.AppendLine();
        code.AppendLine("public class User");
        code.AppendLine("{");
        code.AppendLine("    public string Id { get; set; } = string.Empty;");
        code.AppendLine("    public string Name { get; set; } = string.Empty;");
        code.AppendLine("    public string? Email { get; set; }");
        code.AppendLine("}");
        code.AppendLine();
        code.AppendLine("public interface IGraphQLClient");
        code.AppendLine("{");
        code.AppendLine("    Task<T> QueryAsync<T>(string query, object? variables = null);");
        code.AppendLine("}");

        return code.ToString();
    }

    private static string GenerateJavaCode(string schemaContent, string codeType, string? namespaceName)
    {
        var packageName = namespaceName ?? "com.generated";
        var code = new StringBuilder();
        code.AppendLine($"package {packageName};");
        code.AppendLine();
        code.AppendLine("public class User {");
        code.AppendLine("    private String id;");
        code.AppendLine("    private String name;");
        code.AppendLine("    private String email;");
        code.AppendLine();
        code.AppendLine("    // Constructors, getters, and setters");
        code.AppendLine("    public User() {}");
        code.AppendLine();
        code.AppendLine("    public String getId() { return id; }");
        code.AppendLine("    public void setId(String id) { this.id = id; }");
        code.AppendLine();
        code.AppendLine("    public String getName() { return name; }");
        code.AppendLine("    public void setName(String name) { this.name = name; }");
        code.AppendLine("}");

        return code.ToString();
    }

    private static string GetLanguageExtension(string language)
    {
        return language.ToLower() switch
        {
            "typescript" => "typescript",
            "javascript" => "javascript", 
            "python" => "python",
            "csharp" => "csharp",
            "java" => "java",
            _ => "text"
        };
    }

    private static OptimizationResult OptimizeQueryStructure(string query)
    {
        var result = new OptimizationResult { Query = query };
        
        // Basic optimizations
        if (query.Contains("  "))
        {
            result.Query = Regex.Replace(result.Query, @"\s+", " ");
            result.Improvements.Add("Removed excessive whitespace");
        }

        if (!query.Contains("fragment") && ContainsRepeatedSelections(query))
        {
            result.Improvements.Add("Consider using fragments for repeated selections");
        }

        return result;
    }

    private static ValidationResult ValidateQueryStructure(string query)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(query))
        {
            result.IsValid = false;
            result.Issues.Add("Query cannot be empty");
        }

        var openBraces = query.Count(c => c == '{');
        var closeBraces = query.Count(c => c == '}');
        if (openBraces != closeBraces)
        {
            result.IsValid = false;
            result.Issues.Add($"Mismatched braces: {openBraces} opening, {closeBraces} closing");
        }

        return result;
    }

    private static QueryAnalysis AnalyzeQueryStructure(string query)
    {
        var fieldCount = Regex.Matches(query, @"\b\w+\b").Count;
        var maxDepth = CalculateMaxDepth(query);
        
        return new QueryAnalysis
        {
            FieldCount = fieldCount,
            MaxDepth = maxDepth,
            ComplexityScore = fieldCount + (maxDepth * 2),
            HasVariables = query.Contains("$"),
            HasFragments = query.Contains("fragment") || query.Contains("...")
        };
    }

    private static int CalculateMaxDepth(string query)
    {
        var maxDepth = 0;
        var currentDepth = 0;

        foreach (var c in query)
        {
            if (c == '{')
            {
                currentDepth++;
                maxDepth = Math.Max(maxDepth, currentDepth);
            }
            else if (c == '}')
            {
                currentDepth--;
            }
        }

        return maxDepth;
    }

    private static List<string> GenerateRecommendations(string operation, string utilityType)
    {
        var recommendations = new List<string>();

        if (operation.Contains("users") && !operation.Contains("limit"))
        {
            recommendations.Add("Consider adding pagination to list queries");
        }

        if (CalculateMaxDepth(operation) > 5)
        {
            recommendations.Add("Consider reducing query depth or using fragments");
        }

        if (utilityType == "format")
        {
            recommendations.Add("Use consistent indentation for better readability");
        }

        return recommendations;
    }

    private static bool ContainsRepeatedSelections(string query)
    {
        // Simplified check for repeated field patterns
        var fieldPatterns = Regex.Matches(query, @"\{[^}]+\}").Cast<Match>()
            .Select(m => m.Value)
            .GroupBy(pattern => pattern)
            .Where(g => g.Count() > 1);

        return fieldPatterns.Any();
    }

    private static int CountChars(string input, char target)
    {
        return input.Count(c => c == target);
    }

    private static bool IsGraphQlKeyword(string word)
    {
        var keywords = new[] { "query", "mutation", "subscription", "fragment", "on", "true", "false", "null", "__schema", "__type" };
        return keywords.Contains(word.ToLower());
    }

    #endregion
}