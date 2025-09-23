using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.DataTransfer;
using OrderDto = PlantBasedPizza.OrderManager.Core.OrderDto;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class DistributedEventPublisher(IAmACommandProcessor processor, IDistributedCache orderCache, IServiceScopeFactory serviceScope) : OrderEventPublisher
{
    public async Task Publish(OrderCreatedEventV1 evt)
    {
        await processor.PostAsync(evt);
    }

    public async Task Publish(OrderSubmittedEventV1 evt)
    {
        using var scope = serviceScope.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        
        var order = await orderRepository.Retrieve(evt.OrderIdentifier);

        var orderDto = new OrderDto(order);
        
        await orderCache.SetStringAsync($"order:{evt.OrderIdentifier}", JsonSerializer.Serialize(orderDto), new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });
        await processor.PostAsync(evt);
    }


    public async Task Publish(OrderReadyForDeliveryEventV1 evt)
    {
        await processor.PostAsync(evt);
    }

    public async Task Publish(OrderCompletedEventV1 evt)
    {
        await processor.PostAsync(evt);
    }
}