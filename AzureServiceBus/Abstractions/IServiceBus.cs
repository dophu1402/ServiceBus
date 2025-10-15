using EventBusManager.Abstractions;

namespace AzureServiceBus.Abstractions;

public interface IServiceBus : ISessionEventBus
{
}

public interface ISessionEventBus : IEventBus
{
}