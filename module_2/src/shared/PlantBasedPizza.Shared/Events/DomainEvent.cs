using System.Diagnostics;
using System.Text.Json.Serialization;
using Paramore.Brighter;

namespace PlantBasedPizza.Shared.Events;

public abstract class DomainEvent : IDomainEvent, IEvent
{
    public abstract string EventId { get; }

    public abstract DateTime EventDate { get; }

    public abstract string CorrelationId { get; set; }

    [JsonIgnore]
    public abstract string EventName { get; }

    [JsonIgnore]
    public abstract string EventVersion { get; }

    public abstract string AsString();

    public Guid Id
    {
        get
        {
            return Guid.Parse(EventId);
        }
        set
        {
            //NoOp
        }
    }

    public Activity Span
    {
        get => Activity.Current;
        set
        {
            // NoOp
        }
    }
}