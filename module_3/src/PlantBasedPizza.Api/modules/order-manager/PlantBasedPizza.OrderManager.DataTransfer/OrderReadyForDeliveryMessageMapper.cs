



using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderReadyForDeliveryMessageMapper: IAmAMessageMapper<OrderReadyForDeliveryEventV1> {
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
    public Message MapToMessage(OrderReadyForDeliveryEventV1 request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: request.EventName, messageType: MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public OrderReadyForDeliveryEventV1 MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<OrderReadyForDeliveryEventV1>(message.Body.Value, _jsonSerializerOptions);
    }
}