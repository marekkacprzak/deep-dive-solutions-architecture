
namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    public class DriverCollectedOrderEventHandler
    {
        private readonly IOrderRepository _orderRepository;

        public DriverCollectedOrderEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task Handle(DriverCollectedOrderEvent evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory($"Order collected by driver {evt.DriverName}");
            
            await _orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}