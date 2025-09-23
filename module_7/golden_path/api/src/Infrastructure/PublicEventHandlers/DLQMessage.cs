using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Paramore.Brighter;

namespace Infrastructure.PublicEventHandlers;

public class DLQMessage : IRequest
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

    public string EventName => eventName;
    public string EventVersion { get; }
    public string Data { get; set; }
    public Guid Id { get; set; }
    public Activity Span { get; set; }
    public string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class DLQMessageMapper : IAmAMessageMapper<DLQMessage>
{
    public Message MapToMessage(DLQMessage request)
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