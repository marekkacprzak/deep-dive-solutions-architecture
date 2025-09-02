using PlantBasedPizza.OrderManager.Core.CompleteOrder;

namespace PlantBasedPizza.OrderManager.Core.Handlers;

public class DriverDeliveredOrderEventHandler(CompleteOrderCommandHandler completeOrderCommandHandler)
{
    public async Task Handle(OrderDeliveredEvent evt)
    {
        await completeOrderCommandHandler.Handle(new CompleteOrderCommand
        {
            OrderIdentifier = evt.OrderIdentifier,
        });
    }
}