using System.Reflection;
using FluentValidation;
using GraphBridge.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GraphBridge.Application;

/// <summary>
/// Extension methods for registering Application layer services in the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR (scanning the Application assembly), FluentValidation validators,
    /// and pipeline behaviors (Validation first, then Logging) into the service collection.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR handlers from the Application assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Register pipeline behaviors in order: Validation first, then Logging
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        // Register all FluentValidation validators from the Application assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
