using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Paramore.Brighter;

namespace PlantBasedPizza.Shared.Events;

public class DLQMessage : PublicEvent
{
    private string _eventName = string.Empty;
    
    [JsonConstructor]
    public DLQMessage()
    {
        Data = string.Empty;
        EventVersion = string.Empty;
    }
    
    public DLQMessage(string eventName) : base()
    {
        _eventName = eventName;
        Data = string.Empty;
        EventVersion = string.Empty;
    }

    public override string EventName => _eventName;
    public override string EventVersion { get; }
    public string Data { get; set; }
    
    [JsonIgnore]
    public new Activity? Span { get; set; }
    
    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }

    public new Id? Id { get; set; }
}

public class DLQMessageMapper : IAmAMessageMapper<DLQMessage>
{
    public Message MapToMessage(DLQMessage request, Publication publication)
    {
        var header = new MessageHeader(request.Id ?? new Id(Guid.NewGuid().ToString()), $"{request.EventName}.deadletter", MessageType.MT_EVENT);
        var body = new MessageBody(request.Data);
        var message = new Message(header, body);
        return message;
    }

    public DLQMessage MapToRequest(Message message)
    {
        var topic = message.Header.Topic; // zależnie od implementacji nagłówka
        var eventName = topic?.Value?.Replace(".deadletter", "");
        var dlq = new DLQMessage(eventName ?? message.Header.Topic) { Data = message.Body.Value, Id = message.Id };
        return dlq;
    }

    public IRequestContext? Context { get; set; }
}