using AzureServiceBus.Abstractions;
using EventBusManager;
using EventBusManager.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBus.Processors
{
    public class EventBusSessionProcessor : ISessionEventBusProcessor, IAsyncDisposable
    {
        private readonly IServiceBusPersistenceConnection _serviceBusPersistenceConnection;
        private readonly ILogger<EventBusSessionProcessor> _logger;
        private readonly IEventBusSubscriptionsManager _eventBusSubscriptionsManager;
        private readonly IServiceProvider _serviceProvider;

        private readonly ServiceBusSessionProcessor _processor;
        public EventBusSessionProcessor(ILogger<EventBusSessionProcessor> logger,
            IServiceBusPersistenceConnection serviceBusPersistenceConnection,
            IEventBusSubscriptionsManager eventBusSubscriptionsManager, IServiceProvider serviceProvider,
            string topicName, string subscriptionClientName)
        {
            _logger = logger;
            _serviceBusPersistenceConnection = serviceBusPersistenceConnection;
            _eventBusSubscriptionsManager = eventBusSubscriptionsManager ?? new InMemoryEventBusSubscriptionsManager();
            _serviceProvider = serviceProvider;
            var options = new ServiceBusSessionProcessorOptions { MaxConcurrentSessions = 10, AutoCompleteMessages = false, MaxConcurrentCallsPerSession = 1 };
            _processor =
                serviceBusPersistenceConnection.TopicClient.CreateSessionProcessor(topicName, subscriptionClientName, options);
        }

        public async Task RegisterSubscriptionClientMessageHandlerAsync()
        {
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync();
        }

        private async Task ProcessMessageAsync(ProcessSessionMessageEventArgs args)
        {
            var eventName = $"{args.Message.Subject}";
            var messageData = args.Message.Body.ToString();

            if (await ProcessEventAsync(eventName, messageData))
            {
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation(string.Format("Processed the message {0} in session {1}", args.Message.SequenceNumber, args.SessionId));
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            var ex = args.Exception;
            var context = args.ErrorSource;

            _logger.LogError(new EventId(Environment.ProcessId, ex.Message), ex,
                "Error handling message - Context: {@ExceptionContext}", context);

            return Task.CompletedTask;
        }

        private async Task<bool> ProcessEventAsync(string eventName, string message)
        {
            if (!_eventBusSubscriptionsManager.HasSubscriptionsForEvent(eventName)) return false;
            await using var scope = _serviceProvider.CreateAsyncScope();
            var subscriptions = _eventBusSubscriptionsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                if (subscription.IsDynamic)
                {
                    if (scope.ServiceProvider.GetService(subscription.HandlerType) is not
                        IDynamicIntegrationEventHandler dynamicHandler) continue;

                    using dynamic eventData = JsonDocument.Parse(message);
                    await dynamicHandler.Handle(eventData);
                    continue;
                }

                var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                if (handler == null) continue;
                var eventType = _eventBusSubscriptionsManager.GetEventTypeByName(eventName);
                if (eventType == null) continue;
                var integrationEvent = JsonSerializer.Deserialize(message, eventType);
                var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                await (Task)concreteType.GetMethod("Handle")?.Invoke(handler, new[] { integrationEvent })!;
            }

            return true;
        }

        public async ValueTask DisposeAsync()
        {
            await _processor.CloseAsync();
        }
    }
}
