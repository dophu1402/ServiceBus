using EventBusManager.Events;

namespace PackageDemo.IntegrationEvents.Events
{
    public record TestEvent : IntegrationEvent
    {
        public string Message { get; set; } = string.Empty;
    }
}
