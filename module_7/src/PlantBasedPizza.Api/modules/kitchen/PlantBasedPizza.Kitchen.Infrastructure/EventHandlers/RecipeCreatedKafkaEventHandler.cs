using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.Recipes.DataTransfer;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Policies;
using Saunter.Attributes;
using OrderSubmittedEvent = PlantBasedPizza.Kitchen.Core.Handlers.OrderSubmittedEvent;

namespace PlantBasedPizza.Kitchen.Infrastructure.EventHandlers;

[AsyncApi]
public class RecipeCreatedKafkaEventHandler(
    IDistributedCache cache,
    IAmACommandProcessor processor)
    : RequestHandlerAsync<RecipeCreatedEventV1>
{
    [Channel("recipe.recipeCreated.v1")]
    [SubscribeOperation(typeof(RecipeCreatedEventV1), Summary = "Handle an order submitted event.",
        OperationId = "order-manager.order-submitted")]
    [RequestLoggingAsync(1, HandlerTiming.Before)]
    public override async Task<RecipeCreatedEventV1> HandleAsync(RecipeCreatedEventV1 command,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var cacheKey = $"kitchen:recipe:{command.RecipeIdentifier}";

            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(new RecipeAdapter(command.RecipeIdentifier)),
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