using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.Delivery.DataTransfer;

public class OrderDeliveredMessageMapper: IAmAMessageMapper<OrderDeliveredEventV1> {
    public Message MapToMessage(OrderDeliveredEventV1 request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: request.EventName, messageType: MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderDeliveredEventV1 MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<OrderDeliveredEventV1>(message.Body.Value);
    }
}