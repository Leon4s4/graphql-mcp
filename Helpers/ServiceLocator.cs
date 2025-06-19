using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Graphql.Mcp.Helpers;

/// <summary>
/// Service locator pattern implementation to provide access to DI services from static contexts.
/// This should only be used in MCP tool classes where DI injection is not available.
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes the service locator with the service provider
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the SmartResponseService instance from DI container
    /// Creates a new scope to ensure proper lifetime management
    /// </summary>
    public static async Task<TResult> ExecuteWithSmartResponseServiceAsync<TResult>(
        Func<SmartResponseService, Task<TResult>> action)
    {
        if (_serviceProvider == null)
        {
            // Fallback: create minimal instance for static contexts
            var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
            var logger = new NullLogger<SmartResponseService>();
            var fallbackService = new SmartResponseService(cache, logger);
            return await action(fallbackService);
        }

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SmartResponseService>();
        return await action(service);
    }

    /// <summary>
    /// Gets the SmartResponseService instance from DI container (legacy method for compatibility)
    /// Creates a new scope to ensure proper lifetime management
    /// </summary>
    public static SmartResponseService GetSmartResponseService()
    {
        if (_serviceProvider == null)
        {
            // Fallback: create minimal instance for static contexts
            var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
            var logger = new NullLogger<SmartResponseService>();
            return new SmartResponseService(cache, logger);
        }

        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SmartResponseService>();
    }
}
