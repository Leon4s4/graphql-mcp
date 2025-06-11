using Microsoft.Extensions.DependencyInjection;

namespace Tools;

/// <summary>
/// Service locator to access DI services from static tool classes
/// </summary>
public static class ServiceProvider
{
    private static IServiceProvider? _serviceProvider;
    
    /// <summary>
    /// Initialize the service provider - called once during application startup
    /// </summary>
    /// <param name="serviceProvider">The application's service provider</param>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    /// <summary>
    /// Get a service of the specified type
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve</typeparam>
    /// <returns>The service instance</returns>
    /// <exception cref="InvalidOperationException">If the service provider is not initialized or service not found</exception>
    public static T GetRequiredService<T>() where T : notnull
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("ServiceProvider not initialized. Call ServiceProvider.Initialize() during application startup.");
        }
        
        return _serviceProvider.GetRequiredService<T>();
    }
}
