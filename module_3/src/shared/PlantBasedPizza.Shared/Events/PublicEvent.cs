using System.Diagnostics;
using System.Text.Json.Serialization;
using Paramore.Brighter;

namespace PlantBasedPizza.Shared.Events;

public abstract class PublicEvent : IPublicEvent, IEvent
{
    public PublicEvent()
    {
        Id = Id.Random();
        CorrelationId = Id.Random();
        EventDate = DateTime.UtcNow;
    }

    public DateTime EventDate { get; }

    [JsonIgnore]
    public abstract string EventName { get; }

    [JsonIgnore]
    public abstract string EventVersion { get; }

    public abstract string AsString();

    public Id? CorrelationId { get; set; }
    public Id Id { get; set; }

    public Activity Span
    {
        get => Activity.Current;
        set
        {
            // NoOp
        }
    }
}