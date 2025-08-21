using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.Delivery.DataTransfer;

public class DriverCollectedOrderMessageMapper: IAmAMessageMapper<DriverCollectedOrderEventV1> {
    public Message MapToMessage(DriverCollectedOrderEventV1 request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: request.EventName, messageType: MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public DriverCollectedOrderEventV1 MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<DriverCollectedOrderEventV1>(message.Body.Value);
    }
}