using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class OrderQualityCheckedEventHandler : Handles<OrderQualityCheckedEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderQualityCheckedEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [Channel("kitchen.quality-checked")] // Creates a Channel
        [SubscribeOperation(typeof(OrderQualityCheckedEvent), Summary = "Handle an order quality event.", OperationId = "kitchen.quality-checked")]
        public async Task Handle(OrderQualityCheckedEvent evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order quality checked");

            if (order.OrderType == OrderType.Delivery)
            {
                order.ReadyForDelivery();
            }
            else
            {
                order.MarkAsAwaitingCollection();
            }

            await _orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}