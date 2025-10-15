using AzureServiceBus.Abstractions;
using PackageDemo.IntegrationEvents.Events;
using PackageDemo.IntegrationEvents.Handlers;
using EventBusManager;
using EventBusManager.Abstractions;
using AzureServiceBus;
using System.Reflection;

namespace PackageDemo
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEventHandlers(this IServiceCollection services)
        {
            var eventHandlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && type.GetInterfaces()
                            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
                        .ToList();

            foreach (var handlerType in eventHandlerTypes)
            {
                // Get the event type (TEvent) that the handler handles
                var eventType = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                    .GetGenericArguments()[0];

                // Register the handler with the DI container
                var handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                services.AddTransient(handlerInterface, handlerType);
            }
        }

        public static void AddEventBusServices(this IServiceCollection services, IConfiguration configuration)
        {
            var eventBusSection = configuration.GetSection("EventBus");

            if (!eventBusSection.Exists()) return;

            if (string.Equals(eventBusSection["ProviderName"], "ServiceBus", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSessionEventBus(configuration);
                services.AddSessionEventBusProcessor(configuration);
                services.AddEventBus(configuration);
                services.AddEventBusProcessor(configuration);
                services.AddInMemoryEventSubscriptionManager();
                services.ServiceBusPersistenceConnection(configuration);
            }
        }

        public static void AddEventBusSubscriptions(this IApplicationBuilder applicationBuilder)
        {
            var sessionProcessor = applicationBuilder.ApplicationServices.GetService<ISessionEventBusProcessor>();
            StartEventBusProcessor(sessionProcessor);

            var eventBusProcessor = applicationBuilder.ApplicationServices.GetService<IEventBusProcessor>();
            StartEventBusProcessor(eventBusProcessor);

            var eventBus = applicationBuilder.ApplicationServices.GetService<IEventBus>();
            eventBus?
                .Subscribe<TestEvent,
                    IIntegrationEventHandler<TestEvent>>();
        }

        private static void StartEventBusProcessor<T> (T? processor) where T : IBaseEvenBusProcessor
        {
            processor?.RegisterSubscriptionClientMessageHandlerAsync().GetAwaiter().GetResult();
        }
    }
}
