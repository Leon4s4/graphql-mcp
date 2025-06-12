using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Graphql.Mcp.Helpers;
using ModelContextProtocol.Server;

namespace Graphql.Mcp.Tools;

[McpServerToolType]
public static class SchemaEvolutionTools
{
    [McpServerTool, Description("Detect breaking changes between schema versions")]
    public static async Task<string> DetectBreakingChanges(
        [Description("Old schema endpoint or SDL")] string oldSchema,
        [Description("New schema endpoint or SDL")] string newSchema,
        [Description("Severity level filter (all, critical, major, minor)")] string severityFilter = "all")
    {
       
            var result = new StringBuilder();
            result.AppendLine("# Schema Evolution Analysis\n");

            // Get schema data
            var oldSchemaData = await GetSchemaData(oldSchema);
            var newSchemaData = await GetSchemaData(newSchema);

            if (oldSchemaData == null || newSchemaData == null)
            {
                return "Error: Could not retrieve schema data for comparison";
            }

            // Analyze changes
            var changes = AnalyzeSchemaChanges(oldSchemaData.Value, newSchemaData.Value);
            
            // Filter by severity
            var filteredChanges = FilterChangesBySeverity(changes, severityFilter);

            // Generate report
            result.AppendLine($"## Change Summary");
            result.AppendLine($"- **Total Changes:** {filteredChanges.Count}");
            result.AppendLine($"- **Breaking Changes:** {filteredChanges.Count(c => c.IsBreaking)}");
            result.AppendLine($"- **Non-Breaking Changes:** {filteredChanges.Count(c => !c.IsBreaking)}\n");

            if (filteredChanges.Any(c => c.IsBreaking))
            {
                result.AppendLine("## ⚠️ Breaking Changes");
                foreach (var change in filteredChanges.Where(c => c.IsBreaking))
                {
                    result.AppendLine($"- **{change.Severity}**: {change.Description}");
                    if (!string.IsNullOrEmpty(change.Impact))
                    {
                        result.AppendLine($"  - *Impact:* {change.Impact}");
                    }
                    if (!string.IsNullOrEmpty(change.Recommendation))
                    {
                        result.AppendLine($"  - *Recommendation:* {change.Recommendation}");
                    }
                }
                result.AppendLine();
            }

            if (filteredChanges.Any(c => !c.IsBreaking))
            {
                result.AppendLine("## ✅ Non-Breaking Changes");
                foreach (var change in filteredChanges.Where(c => !c.IsBreaking))
                {
                    result.AppendLine($"- **{change.Severity}**: {change.Description}");
                }
                result.AppendLine();
            }

            // Migration suggestions
            var migrationSuggestions = GenerateMigrationSuggestions(filteredChanges);
            if (migrationSuggestions.Any())
            {
                result.AppendLine("## Migration Suggestions");
                foreach (var suggestion in migrationSuggestions)
                {
                    result.AppendLine($"- {suggestion}");
                }
                result.AppendLine();
            }

            // Compatibility score
            var compatibilityScore = CalculateCompatibilityScore(changes);
            result.AppendLine($"## Compatibility Score: {compatibilityScore:P0}");
            result.AppendLine(GetCompatibilityRecommendation(compatibilityScore));

            return result.ToString();
    }

    [McpServerTool, Description("Track schema evolution metrics and trends")]
    public static async Task<string> TrackSchemaEvolution(
        [Description("List of schema endpoints or versions as JSON array")] string schemaVersions,
        [Description("Include detailed change history")] bool includeHistory = true)
    {
       
            var result = new StringBuilder();
            result.AppendLine("# Schema Evolution Tracking\n");

            var versions = JsonSerializer.Deserialize<string[]>(schemaVersions);
            if (versions == null || versions.Length < 2)
            {
                return "Error: At least 2 schema versions are required for evolution tracking";
            }

            var evolutionData = new List<EvolutionMetrics>();

            // Analyze each version transition
            for (var i = 1; i < versions.Length; i++)
            {
                var oldSchema = await GetSchemaData(versions[i - 1]);
                var newSchema = await GetSchemaData(versions[i]);

                if (oldSchema != null && newSchema != null)
                {
                    var changes = AnalyzeSchemaChanges(oldSchema.Value, newSchema.Value);
                    var metrics = CalculateEvolutionMetrics(changes, i);
                    evolutionData.Add(metrics);
                }
            }

            // Generate evolution report
            result.AppendLine("## Evolution Metrics");
            result.AppendLine("| Version | Breaking Changes | Non-Breaking | Compatibility Score |");
            result.AppendLine("|---------|------------------|--------------|-------------------|");

            foreach (var data in evolutionData)
            {
                result.AppendLine($"| v{data.Version} | {data.BreakingChanges} | {data.NonBreakingChanges} | {data.CompatibilityScore:P0} |");
            }
            result.AppendLine();

            // Trends analysis
            result.AppendLine("## Trends Analysis");
            var avgBreakingChanges = evolutionData.Average(d => d.BreakingChanges);
            var avgCompatibility = evolutionData.Average(d => d.CompatibilityScore);

            result.AppendLine($"- **Average Breaking Changes per Version:** {avgBreakingChanges:F1}");
            result.AppendLine($"- **Average Compatibility Score:** {avgCompatibility:P0}");

            if (evolutionData.Count > 1)
            {
                var trend = evolutionData.Last().CompatibilityScore - evolutionData.First().CompatibilityScore;
                result.AppendLine($"- **Compatibility Trend:** {(trend > 0 ? "Improving" : trend < 0 ? "Declining" : "Stable")} ({trend:+0.0%;-0.0%;0%})");
            }

            return result.ToString();
       
    }

    private static async Task<JsonElement?> GetSchemaData(string schemaSource)
    {
        try
        {
            // Check if it's a URL
            if (Uri.TryCreate(schemaSource, UriKind.Absolute, out _))
            {
                var schemaJson = await SchemaIntrospectionTools.IntrospectSchema(schemaSource);
                var data = JsonSerializer.Deserialize<JsonElement>(schemaJson);
                if (data.TryGetProperty("data", out var schemaData))
                {
                    return schemaData;
                }
            }
            else
            {
                // Assume it's SDL (Schema Definition Language)
                // For now, return null - would need SDL parser for full implementation
                return null;
            }
        }
        catch
        {
            // If parsing fails, try as SDL
        }

        return null;
    }

    private static List<SchemaChange> AnalyzeSchemaChanges(JsonElement oldSchema, JsonElement newSchema)
    {
        var changes = new List<SchemaChange>();

        // Get types from both schemas
        var oldTypes = GetSchemaTypes(oldSchema);
        var newTypes = GetSchemaTypes(newSchema);

        // Detect type changes
        foreach (var oldType in oldTypes)
        {
            var newType = newTypes.FirstOrDefault(t => t.Name == oldType.Name);
            if (newType == null)
            {
                changes.Add(new SchemaChange
                {
                    Type = ChangeType.TypeRemoved,
                    Severity = ChangeSeverity.Critical,
                    IsBreaking = true,
                    Description = $"Type '{oldType.Name}' was removed",
                    Impact = "All queries using this type will fail",
                    Recommendation = "Ensure no clients are using this type before removal"
                });
            }
            else
            {
                // Compare type details
                changes.AddRange(CompareTypes(oldType, newType));
            }
        }

        // Detect new types
        foreach (var newType in newTypes)
        {
            if (!oldTypes.Any(t => t.Name == newType.Name))
            {
                changes.Add(new SchemaChange
                {
                    Type = ChangeType.TypeAdded,
                    Severity = ChangeSeverity.Minor,
                    IsBreaking = false,
                    Description = $"Type '{newType.Name}' was added",
                    Impact = "New functionality available to clients"
                });
            }
        }

        return changes;
    }

    private static List<SchemaChange> CompareTypes(TypeInfo oldType, TypeInfo newType)
    {
        var changes = new List<SchemaChange>();

        // Compare fields
        foreach (var oldField in oldType.Fields)
        {
            var newField = newType.Fields.FirstOrDefault(f => f.Name == oldField.Name);
            if (newField == null)
            {
                changes.Add(new SchemaChange
                {
                    Type = ChangeType.FieldRemoved,
                    Severity = ChangeSeverity.Critical,
                    IsBreaking = true,
                    Description = $"Field '{oldField.Name}' was removed from type '{oldType.Name}'",
                    Impact = "Queries selecting this field will fail",
                    Recommendation = "Use deprecation before removal"
                });
            }
            else if (oldField.TypeName != newField.TypeName)
            {
                var isBreaking = IsTypeChangeBreaking(oldField.TypeName, newField.TypeName);
                changes.Add(new SchemaChange
                {
                    Type = ChangeType.FieldTypeChanged,
                    Severity = isBreaking ? ChangeSeverity.Critical : ChangeSeverity.Major,
                    IsBreaking = isBreaking,
                    Description = $"Field '{oldField.Name}' type changed from '{oldField.TypeName}' to '{newField.TypeName}' in type '{oldType.Name}'",
                    Impact = isBreaking ? "May cause client parsing errors" : "Client adaptation may be needed"
                });
            }
        }

        // Detect new fields
        foreach (var newField in newType.Fields)
        {
            if (!oldType.Fields.Any(f => f.Name == newField.Name))
            {
                changes.Add(new SchemaChange
                {
                    Type = ChangeType.FieldAdded,
                    Severity = ChangeSeverity.Minor,
                    IsBreaking = false,
                    Description = $"Field '{newField.Name}' was added to type '{oldType.Name}'",
                    Impact = "New data available to clients"
                });
            }
        }

        return changes;
    }

    private static bool IsTypeChangeBreaking(string oldType, string newType)
    {
        // Simplistic check - could be more sophisticated
        return !(oldType.Contains("String") && newType.Contains("String")) &&
               !(oldType.Contains("Int") && newType.Contains("Int")) &&
               !(oldType.Contains("Float") && newType.Contains("Float"));
    }

    private static List<TypeInfo> GetSchemaTypes(JsonElement schema)
    {
        var types = new List<TypeInfo>();

        if (schema.TryGetProperty("__schema", out var schemaElement) &&
            schemaElement.TryGetProperty("types", out var typesArray))
        {
            foreach (var typeElement in typesArray.EnumerateArray())
            {
                if (typeElement.TryGetProperty("name", out var nameElement))
                {
                    var typeName = nameElement.GetString();
                    if (!string.IsNullOrEmpty(typeName) && !typeName.StartsWith("__"))
                    {
                        var typeInfo = new TypeInfo { Name = typeName };
                        
                        if (typeElement.TryGetProperty("fields", out var fieldsArray))
                        {
                            foreach (var fieldElement in fieldsArray.EnumerateArray())
                            {
                                if (fieldElement.TryGetProperty("name", out var fieldNameElement) &&
                                    fieldElement.TryGetProperty("type", out var fieldTypeElement))
                                {
                                    typeInfo.Fields.Add(new FieldInfo
                                    {
                                        Name = fieldNameElement.GetString() ?? "",
                                        TypeName = GraphQlTypeHelpers.GetTypeName(fieldTypeElement)
                                    });
                                }
                            }
                        }

                        types.Add(typeInfo);
                    }
                }
            }
        }

        return types;
    }

    private static List<SchemaChange> FilterChangesBySeverity(List<SchemaChange> changes, string severityFilter)
    {
        return severityFilter.ToLower() switch
        {
            "critical" => changes.Where(c => c.Severity == ChangeSeverity.Critical).ToList(),
            "major" => changes.Where(c => c.Severity == ChangeSeverity.Major || c.Severity == ChangeSeverity.Critical).ToList(),
            "minor" => changes.Where(c => c.Severity == ChangeSeverity.Minor).ToList(),
            _ => changes
        };
    }

    private static List<string> GenerateMigrationSuggestions(List<SchemaChange> changes)
    {
        var suggestions = new List<string>();

        if (changes.Any(c => c.Type == ChangeType.FieldRemoved))
        {
            suggestions.Add("Consider using field deprecation before removal to give clients time to adapt");
        }

        if (changes.Any(c => c.Type == ChangeType.TypeRemoved))
        {
            suggestions.Add("Ensure all client applications are updated before removing types");
        }

        if (changes.Any(c => c.Type == ChangeType.FieldTypeChanged && c.IsBreaking))
        {
            suggestions.Add("For breaking type changes, consider adding new fields alongside old ones temporarily");
        }

        return suggestions;
    }

    private static double CalculateCompatibilityScore(List<SchemaChange> changes)
    {
        if (!changes.Any()) return 1.0;

        var breakingChanges = changes.Count(c => c.IsBreaking);
        var totalChanges = changes.Count;

        return 1.0 - (breakingChanges / (double)totalChanges);
    }

    private static string GetCompatibilityRecommendation(double score)
    {
        return score switch
        {
            >= 0.9 => "✅ Excellent compatibility - minimal client impact expected",
            >= 0.7 => "⚠️ Good compatibility - some client updates may be needed",
            >= 0.5 => "⚠️ Moderate compatibility - significant client updates required",
            _ => "❌ Poor compatibility - major breaking changes detected"
        };
    }

    private static EvolutionMetrics CalculateEvolutionMetrics(List<SchemaChange> changes, int version)
    {
        return new EvolutionMetrics
        {
            Version = version,
            BreakingChanges = changes.Count(c => c.IsBreaking),
            NonBreakingChanges = changes.Count(c => !c.IsBreaking),
            CompatibilityScore = CalculateCompatibilityScore(changes)
        };
    }

    private class SchemaChange
    {
        public ChangeType Type { get; set; }
        public ChangeSeverity Severity { get; set; }
        public bool IsBreaking { get; set; }
        public string Description { get; set; } = "";
        public string? Impact { get; set; }
        public string? Recommendation { get; set; }
    }

    private class TypeInfo
    {
        public string Name { get; set; } = "";
        public List<FieldInfo> Fields { get; set; } = [];
    }

    private class FieldInfo
    {
        public string Name { get; set; } = "";
        public string TypeName { get; set; } = "";
    }

    private class EvolutionMetrics
    {
        public int Version { get; set; }
        public int BreakingChanges { get; set; }
        public int NonBreakingChanges { get; set; }
        public double CompatibilityScore { get; set; }
    }

    private enum ChangeType
    {
        TypeAdded,
        TypeRemoved,
        FieldAdded,
        FieldRemoved,
        FieldTypeChanged,
        ArgumentAdded,
        ArgumentRemoved
    }

    private enum ChangeSeverity
    {
        Minor,
        Major,
        Critical
    }
}
