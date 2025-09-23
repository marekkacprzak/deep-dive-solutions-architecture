using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.AddItemToOrder;

public class AddItemToOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRecipeService _recipeService;

    public AddItemToOrderHandler(IOrderRepository orderRepository, IRecipeService recipeService)
    {
        _orderRepository = orderRepository;
        _recipeService = recipeService;
    }
    
    public async Task<OrderDto?> Handle(AddItemToOrderCommand command)
    {
        try
        {
            var recipe = await _recipeService.GetRecipe(command.RecipeIdentifier);
            
            var order = await _orderRepository.Retrieve(command.OrderIdentifier);

            order.AddOrderItem(command.RecipeIdentifier, recipe.Name, command.Quantity, recipe.Price);

            await _orderRepository.Update(order);

            return new  OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}