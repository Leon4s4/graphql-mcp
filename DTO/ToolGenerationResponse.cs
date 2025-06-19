namespace Graphql.Mcp.DTO;

/// <summary>
/// Response for GraphQL tool generation operations
/// </summary>
public class ToolGenerationResponse
{
    public bool Success { get; set; }
    public string ResponseId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public ToolGenerationData? Data { get; set; }
    public ResponseMetadata? Metadata { get; set; }

    public static ToolGenerationResponse CreateSuccess(ToolGenerationData data, string? message = null)
    {
        return new ToolGenerationResponse
        {
            Success = true,
            ResponseId = Guid.NewGuid().ToString("N")[..8],
            Timestamp = DateTime.UtcNow,
            Data = data,
            Metadata = new ResponseMetadata
            {
                OperationType = "ToolGeneration",
                CacheStatus = "Fresh",
                RecommendedActions = message != null ? [message] : ["Tools generated successfully"]
            }
        };
    }

    public static ToolGenerationResponse CreateError(string errorCode, string errorMessage, string? details = null)
    {
        return new ToolGenerationResponse
        {
            Success = false,
            ResponseId = Guid.NewGuid().ToString("N")[..8],
            Timestamp = DateTime.UtcNow,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Metadata = new ResponseMetadata
            {
                OperationType = "ToolGeneration",
                CacheStatus = "Error",
                RecommendedActions = details != null ? [details] : [errorMessage]
            }
        };
    }

    /// <summary>
    /// Formats the response for display to users (backward compatibility)
    /// </summary>
    public string FormatForDisplay()
    {
        if (Success && Data != null)
        {
            var message = $"Schema root types detected:\n";
            message += $"- Query: {Data.QueryType}\n";
            message += $"- Mutation: {Data.MutationType ?? "None"}\n";
            message += $"- Subscription: {Data.SubscriptionType ?? "None"}\n";
            message += $"- Allow Mutations: {Data.AllowMutations}\n\n";

            if (Data.QueryToolsGenerated > 0)
            {
                message += $"Generated {Data.QueryToolsGenerated} query tools from {Data.QueryFieldsCount} fields\n";
            }

            if (Data.MutationToolsGenerated > 0)
            {
                message += $"Generated {Data.MutationToolsGenerated} mutation tools from {Data.MutationFieldsCount} fields\n";
            }
            else if (!Data.AllowMutations)
            {
                message += "Mutations are disabled for this endpoint\n";
            }
            else
            {
                message += "No mutation type found in schema\n";
            }

            message += $"\nSuccessfully registered endpoint '{Data.EndpointName}' with {Data.TotalToolsGenerated} dynamic tools.";
            
            return message;
        }

        return $"Error: {ErrorMessage}";
    }
}

/// <summary>
/// Data structure for tool generation results
/// </summary>
public class ToolGenerationData
{
    public string EndpointName { get; set; } = "";
    public string EndpointUrl { get; set; } = "";
    public string? QueryType { get; set; }
    public string? MutationType { get; set; }
    public string? SubscriptionType { get; set; }
    public bool AllowMutations { get; set; }
    public int QueryToolsGenerated { get; set; }
    public int MutationToolsGenerated { get; set; }
    public int TotalToolsGenerated { get; set; }
    public int QueryFieldsCount { get; set; }
    public int MutationFieldsCount { get; set; }
    public List<GeneratedToolInfo> GeneratedTools { get; set; } = new();
    public Dictionary<string, object> SchemaMetadata { get; set; } = new();
}

/// <summary>
/// Information about a generated tool
/// </summary>
public class GeneratedToolInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = ""; // "query" or "mutation"
    public string Description { get; set; } = "";
    public List<string> Parameters { get; set; } = new();
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }
}
