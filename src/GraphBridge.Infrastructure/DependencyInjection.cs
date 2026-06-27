using GraphBridge.Application.BuildEstate;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.LegalMatters;
using GraphBridge.Application.LoanApprovals;
using GraphBridge.Application.Onboarding;
using GraphBridge.Infrastructure.Auth;
using GraphBridge.Infrastructure.LiveServices;
using GraphBridge.Infrastructure.MockServices;
using GraphBridge.Infrastructure.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GraphBridge.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services in the DI container.
/// Registers mock or live Graph service implementations based on the GraphBridge:GraphMode setting,
/// and always registers in-memory stores as singletons for Demo_Mode entity persistence.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure layer services: in-memory stores, and either mock or live
    /// Graph service implementations depending on the configured GraphBridge:GraphMode value.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration for reading mode and Azure AD settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGraphBridgeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var graphMode = configuration["GraphBridge:GraphMode"];

        // Always register in-memory stores as singletons (used by all CQRS handlers for entity persistence)
        services.AddSingleton<IEmployeeStore, InMemoryEmployeeStore>();
        services.AddSingleton<ILegalMatterStore, InMemoryLegalMatterStore>();
        services.AddSingleton<ILoanApprovalStore, InMemoryLoanApprovalStore>();
        services.AddSingleton<IBuildEstateProjectStore, InMemoryBuildEstateStore>();

        if (string.Equals(graphMode, "Live", StringComparison.OrdinalIgnoreCase))
        {
            RegisterLiveServices(services, configuration);
        }
        else
        {
            RegisterMockServices(services);
        }

        return services;
    }

    private static void RegisterLiveServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Azure AD options for token acquisition
        services.Configure<AzureAdOptions>(configuration.GetSection(AzureAdOptions.SectionName));

        // Register token cache service for MSAL token acquisition and caching
        services.AddSingleton<ITokenCacheService, TokenCacheService>();

        // Register live Graph service implementations
        services.AddSingleton<IGraphUserService, LiveGraphUserService>();
        services.AddSingleton<IGraphGroupService, LiveGraphGroupService>();
        services.AddSingleton<IGraphMailService, LiveGraphMailService>();
        services.AddSingleton<IGraphCalendarService, LiveGraphCalendarService>();
        services.AddSingleton<IGraphTeamsService, LiveGraphTeamsService>();
        services.AddSingleton<IGraphDriveService, LiveGraphDriveService>();
        services.AddSingleton<IGraphPlannerService, LiveGraphPlannerService>();
        services.AddSingleton<IGraphSecurityService, LiveGraphSecurityService>();
        services.AddSingleton<IGraphReportService, LiveGraphReportService>();
    }

    private static void RegisterMockServices(IServiceCollection services)
    {
        // Register mock Graph service implementations for Demo_Mode
        services.AddSingleton<IGraphUserService, MockGraphUserService>();
        services.AddSingleton<IGraphGroupService, MockGraphGroupService>();
        services.AddSingleton<IGraphMailService, MockGraphMailService>();
        services.AddSingleton<IGraphCalendarService, MockGraphCalendarService>();
        services.AddSingleton<IGraphTeamsService, MockGraphTeamsService>();
        services.AddSingleton<IGraphDriveService, MockGraphDriveService>();
        services.AddSingleton<IGraphPlannerService, MockGraphPlannerService>();
        services.AddSingleton<IGraphSecurityService, MockGraphSecurityService>();
        services.AddSingleton<IGraphReportService, MockGraphReportService>();
    }
}
