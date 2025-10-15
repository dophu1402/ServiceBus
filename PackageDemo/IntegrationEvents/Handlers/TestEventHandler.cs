using EventBusManager.Abstractions;
using PackageDemo.IntegrationEvents.Events;

namespace PackageDemo.IntegrationEvents.Handlers
{
    public class TestEventHandler(ILogger<TestEventHandler> logger) : IIntegrationEventHandler<TestEvent>
    {
        public Task Handle(TestEvent @event)
        {
            logger.LogInformation("Processing message {message} from {event}", @event.Message, nameof(TestEventHandler));
            return Task.CompletedTask;
        }
    }
}
