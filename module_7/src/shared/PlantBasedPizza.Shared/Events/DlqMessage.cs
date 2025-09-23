using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Paramore.Brighter;

namespace PlantBasedPizza.Shared.Events;

public class DLQMessage : PublicEvent
{
    private string eventName;
    [JsonConstructor]
    public DLQMessage()
    {
    }
    
    public DLQMessage(string eventName) : base()
    {
        this.eventName = eventName;
    }

    public override string EventName => eventName;
    public override string EventVersion { get; }
    public string Data { get; set; }
    public Activity Span { get; set; }
    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }

    public Id? CorrelationId { get; set; }
    public Id Id { get; set; }
}

public class DLQMessageMapper : IAmAMessageMapper<DLQMessage>
{
    public Message MapToMessage(DLQMessage request, Publication publication)
    {
        var header = new MessageHeader(request.Id, $"{request.EventName}.deadletter", MessageType.MT_EVENT);
        var body = new MessageBody(request.Data);
        var message = new Message(header, body);
        return message;
    }

    public DLQMessage MapToRequest(Message message)
    {
        var evt = JsonSerializer.Deserialize<DLQMessage>(message.Body.Value);

        return evt;
    }

    public IRequestContext? Context { get; set; }
}