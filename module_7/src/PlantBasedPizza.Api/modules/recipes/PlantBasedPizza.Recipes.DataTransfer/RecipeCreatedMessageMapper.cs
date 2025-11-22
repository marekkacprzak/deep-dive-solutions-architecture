using System.Text.Json;
using Paramore.Brighter;

namespace PlantBasedPizza.Recipes.DataTransfer;

public class RecipeCreatedMessageMapper : IAmAMessageMapper<RecipeCreatedEventV1>
{
    public Message MapToMessage(RecipeCreatedEventV1 request, Publication publication)
    {
        var header = new MessageHeader(request.Id, request.EventName, MessageType.MT_EVENT);
        var body = new MessageBody(request.AsString());
        var message = new Message(header, body);
        return message;
    }

    public RecipeCreatedEventV1 MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<RecipeCreatedEventV1>(message.Body.Value) ?? throw new InvalidOperationException("Failed to deserialize message");
    }

    public IRequestContext? Context { get; set; }
}