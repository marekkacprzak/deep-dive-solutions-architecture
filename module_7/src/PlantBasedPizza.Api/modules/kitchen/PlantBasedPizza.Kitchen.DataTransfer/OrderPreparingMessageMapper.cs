



using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.Kitchen.DataTransfer;

public class OrderPreparingMessageMapper: IAmAMessageMapper<OrderPreparingEventV1> {
    public Message MapToMessage(OrderPreparingEventV1 request, Publication publication)
    {
        var header = new MessageHeader(messageId: request.Id, topic: request.EventName, messageType: MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderPreparingEventV1 MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<OrderPreparingEventV1>(message.Body.Value)!;
    }

    public IRequestContext? Context { get; set; }
}