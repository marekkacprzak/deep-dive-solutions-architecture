using System.Text.Json;
using Paramore.Brighter;
using PlantBasedPizza.Events;

public class OrderSubmittedMessageMapper : IAmAMessageMapper<OrderSubmittedEvent>
{
    public Message MapToMessage(OrderSubmittedEvent request)
    {
        var header = new MessageHeader(request.Id, request.EventName, MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderSubmittedEvent MapToRequest(Message message)
    {
        var evt = JsonSerializer.Deserialize<OrderSubmittedEvent>(message.Body.Value);

        return evt;
    }
}