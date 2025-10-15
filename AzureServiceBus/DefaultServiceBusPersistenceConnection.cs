using AzureServiceBus.Abstractions;

namespace AzureServiceBus;

public class DefaultServiceBusPersistenceConnection : IServiceBusPersistenceConnection
{
    private readonly string _serviceBusConnectionString;
    private ServiceBusClient _topicClient;
    private readonly ServiceBusAdministrationClient _subscriptionClient;

    private bool _disposed;

    public ServiceBusAdministrationClient AdministrationClient => _subscriptionClient;


    public DefaultServiceBusPersistenceConnection(string serviceBusConnectionString)
    {
        _serviceBusConnectionString = serviceBusConnectionString;
        _topicClient = new ServiceBusClient(serviceBusConnectionString);
        _subscriptionClient = new ServiceBusAdministrationClient(serviceBusConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;
        await _topicClient.DisposeAsync();
    }

    public ServiceBusClient TopicClient
    {
        get
        {
            if (_topicClient.IsClosed)
            {
                _topicClient = new ServiceBusClient(_serviceBusConnectionString);
            }

            return _topicClient;
        }
    }
}