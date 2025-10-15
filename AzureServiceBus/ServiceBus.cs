using AzureServiceBus.Abstractions;
using EventBusManager;
using EventBusManager.Abstractions;
using EventBusManager.Events;

namespace AzureServiceBus;

public class ServiceBus : IServiceBus, IAsyncDisposable
{
    protected readonly IServiceBusPersistenceConnection _serviceBusPersistenceConnection;
    protected readonly ILogger<ServiceBus> _logger;
    protected readonly IEventBusSubscriptionsManager _eventBusSubscriptionsManager;
    protected readonly IServiceProvider _serviceProvider;

    private readonly ServiceBusSender _sender;
    protected readonly string _topicName;
    protected readonly string _subscriptionName;

    private const string IntegrationEventSuffix = "IntegrationEvent";

    public ServiceBus(ILogger<ServiceBus> logger,
        IServiceBusPersistenceConnection serviceBusPersistenceConnection,
        IEventBusSubscriptionsManager eventBusSubscriptionsManager, IServiceProvider serviceProvider,
        string topicName, string subscriptionClientName)
    {
        _logger = logger;
        _serviceBusPersistenceConnection = serviceBusPersistenceConnection;
        _eventBusSubscriptionsManager = eventBusSubscriptionsManager ?? new InMemoryEventBusSubscriptionsManager();
        _serviceProvider = serviceProvider;
        _subscriptionName = subscriptionClientName;
        _topicName = topicName;
        _sender = serviceBusPersistenceConnection.TopicClient.CreateSender(topicName);
        RemoveDefaultRule();
    }

    public void Publish(IntegrationEvent @event)
    {
        var eventName = @event.GetType().Name.Replace(IntegrationEventSuffix, "");
        var jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        var message = new ServiceBusMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            Body = new BinaryData(body),
            Subject = eventName,
            SessionId = @event.SessionId,
        };

        _sender.SendMessageAsync(message)
            .GetAwaiter()
            .GetResult();
    }

    public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name.Replace(IntegrationEventSuffix, "");

        var containsKey = _eventBusSubscriptionsManager.HasSubscriptionsForEvent<T>();
        if (!containsKey)
        {
            try
            {
                _serviceBusPersistenceConnection.AdministrationClient.CreateRuleAsync(_topicName, _subscriptionName,
                    new CreateRuleOptions
                    {
                        Filter = new CorrelationRuleFilter { Subject = eventName },
                        Name = eventName
                    }).GetAwaiter().GetResult();
            }
            catch (ServiceBusException)
            {
                _logger.LogWarning("The messaging entity {EventName} already exists", eventName);
            }
        }

        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

        _eventBusSubscriptionsManager.AddSubscription<T, TH>();
    }

    public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name.Replace(IntegrationEventSuffix, "");

        try
        {
            _serviceBusPersistenceConnection
                .AdministrationClient
                .DeleteRuleAsync(_topicName, _subscriptionName, eventName)
                .GetAwaiter()
                .GetResult();
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            _logger.LogWarning("The messaging entity {EventName} Could not be found", eventName);
        }

        _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

        _eventBusSubscriptionsManager.RemoveSubscription<T, TH>();
    }

    public async ValueTask DisposeAsync()
    {
        _eventBusSubscriptionsManager.Clear();
        await _serviceBusPersistenceConnection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
    {
        _logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName,
            typeof(TH).Name);

        _eventBusSubscriptionsManager.AddDynamicSubscription<TH>(eventName);
    }

    public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
    {
        _logger.LogInformation("Unsubscribing from dynamic event {EventName}", eventName);

        _eventBusSubscriptionsManager.RemoveDynamicSubscription<TH>(eventName);
    }

    private void RemoveDefaultRule()
    {
        try
        {
            _serviceBusPersistenceConnection
                .AdministrationClient
                .DeleteRuleAsync(_topicName, _subscriptionName, RuleProperties.DefaultRuleName)
                .GetAwaiter()
                .GetResult();
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            _logger.LogWarning("The messaging entity {DefaultRuleName} Could not be found",
                RuleProperties.DefaultRuleName);
        }
    }
}