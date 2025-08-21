



using System.Diagnostics;
using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class DLQMessage : IEvent
{
    public string EventName { get; set; }
    public string Data { get; set; }
    public Guid Id { get; set; }
    public Activity Span { get; set; }
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
}