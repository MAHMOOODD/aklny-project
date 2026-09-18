using Resturant_Backend.Middlewares;

namespace Resturant_Backend.Common.Extensions;

/// <summary>
/// Extension methods for registering common middleware
/// </summary>
public static class MiddlewareRegistration
{
    /// <summary>
    /// Adds global exception handling middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    /// <summary>
    /// Registers all common services
    /// </summary>
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        // Add any common services here
        return services;
    }
}