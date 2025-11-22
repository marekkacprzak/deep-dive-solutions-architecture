using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.Kitchen.Core.Handlers
{
    public class OrderSubmittedEventHandler(
        IKitchenRequestRepository kitchenRequestRepository,
        IRecipeService recipeService,
        ILogger<OrderSubmittedEventHandler> logger,
        IOrderManagerService orderManagerService)
    {
        private readonly ILogger<OrderSubmittedEventHandler> _logger = logger;

        public async Task Handle(OrderSubmittedEvent evt)
        {
            Guard.AgainstNull(evt, nameof(evt));
            Activity.Current?.SetTag("order.identifier", evt.OrderIdentifier);
            
            var existingRequestForOrder = await kitchenRequestRepository.Retrieve(evt.OrderIdentifier);

            if (existingRequestForOrder != null)
            {
                Activity.Current?.SetTag("order.received", "true");
                
                return;
            }

            var recipes = new List<RecipeAdapter>();
            
            var order = await orderManagerService.GetOrderDetails(evt.OrderIdentifier);

            if (order is null)
            {
                return;
            }

            if (order.Items is null)
            {
                return;
            }

            foreach (var recipe in order.Items)
            {
                recipes.Add(await recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            await kitchenRequestRepository.AddNew(kitchenRequest);
        }
    }
}