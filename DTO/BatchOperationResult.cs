namespace Graphql.Mcp.DTO;

/// <summary>
/// Result of a single operation in a batch
/// </summary>
public class BatchOperationResult
{
    /// <summary>
    /// Name of the operation
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Operation result data
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Error information if operation failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Execution time for this operation
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }

    /// <summary>
    /// Operation index in the batch
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Endpoint used for this operation
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Query that was executed
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Variables used in the operation
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; }

    /// <summary>
    /// Whether this operation was retried
    /// </summary>
    public bool WasRetried { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryAttempts { get; set; }
}