using System.Text.Json.Serialization;

namespace EventBusManager.Events;

public record IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreatedTime = DateTime.UtcNow;
    }

    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createdTime, string sessionId)
    {
        Id = id;
        CreatedTime = createdTime;
        SessionId = sessionId;
    }

    [JsonInclude] public Guid Id { get; private init; }

    [JsonInclude] public DateTime CreatedTime { get; private init; }

    [JsonInclude] public string SessionId { get; set; }
}