using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.Recipes.DataTransfer;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.PublicEventHandlers;

[AsyncApi]
public class RecipeCreatedKafkaEventHandler(
    IDistributedCache cache,
    IAmACommandProcessor processor)
    : RequestHandlerAsync<RecipeCreatedEventV1>
{
    [Channel("recipe.recipe-created")]
    [SubscribeOperation(typeof(RecipeCreatedEventV1), Summary = "Handle a recipe created event.",
        OperationId = "recipe.recipe-created")]
    [RequestLoggingAsync(1, HandlerTiming.Before)]
    public override async Task<RecipeCreatedEventV1> HandleAsync(RecipeCreatedEventV1 command,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var cacheKey = $"order-manager:recipe:{command.RecipeIdentifier}";

            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(new Recipe
                {
                    Name = command.Name,
                    Price = command.Price
                }),
                cancellationToken);
        }
        // Generic exception handling to ensure all errors get routed to the DLQ
        catch (Exception)
        {
            await FallbackAsync(command);
        }

        return await base.HandleAsync(command);
    }

    public override async Task<RecipeCreatedEventV1> FallbackAsync(RecipeCreatedEventV1 command,
        CancellationToken cancellationToken = new())
    {
        processor.Post(new DLQMessage(RecipeCreatedEventV1.EventTypeName)
        {
            Data = command.AsString()
        });

        return await base.FallbackAsync(command, cancellationToken);
    }
}