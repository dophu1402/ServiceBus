using AzureServiceBus.Abstractions;
using AzureServiceBus.Processors;
using EventBusManager.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBus;

public static class ServiceCollectionExtensions
{
    public static void ServiceBusPersistenceConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IServiceBusPersistenceConnection>(_ =>
        {
            var serviceBusConnectionString = configuration.GetRequiredConnectionString("EventBus");

            return new DefaultServiceBusPersistenceConnection(serviceBusConnectionString);
        });
    }

    public static void AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        var eventBusSection = configuration.GetSection("EventBus");

        if (!eventBusSection.Exists()) return;

        services.AddSingleton<IEventBus, ServiceBus>(sp =>
        {
            var serviceBusPersistenceConnection = sp.GetRequiredService<IServiceBusPersistenceConnection>();
            var logger = sp.GetRequiredService<ILogger<ServiceBus>>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
            var topicName = eventBusSection.GetRequiredValue("TopicName");
            var subscriptionClientName = eventBusSection.GetRequiredValue("SubscriptionClientName");

            return new ServiceBus(logger, serviceBusPersistenceConnection, eventBusSubscriptionsManager, sp,
                topicName, subscriptionClientName);
        });
    }

    public static void AddSessionEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        var eventBusSection = configuration.GetSection("EventBus");

        if (!eventBusSection.Exists()) return;

        services.AddSingleton<ISessionEventBus, ServiceBus>(sp =>
        {
            var serviceBusPersistenceConnection = sp.GetRequiredService<IServiceBusPersistenceConnection>();
            var logger = sp.GetRequiredService<ILogger<ServiceBus>>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
            var topicName = eventBusSection.GetRequiredValue("TopicName");
            var subscriptionClientName = eventBusSection.GetRequiredValue("SessionSubscriptionClientName");

            return new ServiceBus(logger, serviceBusPersistenceConnection, eventBusSubscriptionsManager, sp,
                topicName, subscriptionClientName);
        });
    }

    public static void AddEventBusProcessor(this IServiceCollection services, IConfiguration configuration)
    {
        var eventBusSection = configuration.GetSection("EventBus");

        if (!eventBusSection.Exists()) return;

        services.AddSingleton<IEventBusProcessor, EventBusProcessor>(sp =>
        {
            var serviceBusPersistenceConnection = sp.GetRequiredService<IServiceBusPersistenceConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusProcessor>>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
            var topicName = eventBusSection.GetRequiredValue("TopicName");
            var subscriptionClientName = eventBusSection.GetRequiredValue("SubscriptionClientName");

            return new EventBusProcessor(logger, serviceBusPersistenceConnection, eventBusSubscriptionsManager, sp,
                topicName, subscriptionClientName);
        });
    }

    public static void AddSessionEventBusProcessor(this IServiceCollection services, IConfiguration configuration)
    {
        var eventBusSection = configuration.GetSection("EventBus");

        if (!eventBusSection.Exists()) return;

        services.AddSingleton<ISessionEventBusProcessor, EventBusSessionProcessor>(sp =>
        {
            var serviceBusPersistenceConnection = sp.GetRequiredService<IServiceBusPersistenceConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusSessionProcessor>>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
            var topicName = eventBusSection.GetRequiredValue("TopicName");
            var subscriptionClientName = eventBusSection.GetRequiredValue("SessionSubscriptionClientName");

            return new EventBusSessionProcessor(logger, serviceBusPersistenceConnection, eventBusSubscriptionsManager, sp,
                topicName, subscriptionClientName);
        });
    }
}