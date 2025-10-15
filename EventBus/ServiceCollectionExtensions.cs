using EventBusManager.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventBusManager;

public static class ServiceCollectionExtensions
{
    public static void AddInMemoryEventSubscriptionManager(this IServiceCollection services)
    {
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
    }
}