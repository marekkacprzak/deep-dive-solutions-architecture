



using PlantBasedPizza.Recipes.DataTransfer;

namespace PlantBasedPizza.Recipes.Infrastructure;

public interface RecipeEventPublisher
{
    Task AddToEventOutbox(RecipeCreatedEventV1 evt);

    Task ClearOutbox();
}