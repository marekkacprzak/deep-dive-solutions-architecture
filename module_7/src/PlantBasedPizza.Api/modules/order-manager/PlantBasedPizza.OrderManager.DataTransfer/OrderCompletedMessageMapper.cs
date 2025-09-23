using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderCompletedMessageMapper : IAmAMessageMapper<OrderCompletedEventV1>
{
    public Message MapToMessage(OrderCompletedEventV1 request, Publication publication)
    {
        var header = new MessageHeader(request.Id, request.EventName, MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderCompletedEventV1 MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<OrderCompletedEventV1>(message.Body.Value);
    }

    public IRequestContext? Context { get; set; }
}