namespace AzureServiceBus.Abstractions;

public interface IServiceBusPersistenceConnection : IAsyncDisposable
{
    ServiceBusClient TopicClient { get; }
    ServiceBusAdministrationClient AdministrationClient { get; }
}