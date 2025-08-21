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

            var recipes = new List<RecipeAdapter>();
            
            var order = await orderManagerService.GetOrderDetails(evt.OrderIdentifier);

            foreach (var recipe in order.Items)
            {
                recipes.Add(await recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            await kitchenRequestRepository.AddNew(kitchenRequest);
        }
    }
}