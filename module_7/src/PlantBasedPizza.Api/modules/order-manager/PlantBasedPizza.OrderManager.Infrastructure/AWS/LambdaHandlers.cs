using System.Text.Json;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.KafkaEvents;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Delivery.DataTransfer;
using PlantBasedPizza.OrderManager.Core.Handlers;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Orders.Lambda;

public class Handlers(IServiceScopeFactory serviceScopeFactory)
{
    [LambdaFunction]
    public async Task OrderDelivered(KafkaEvent evt)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<DriverDeliveredOrderEventHandler>();
        
        foreach (var topics in evt.Records)
        {
            foreach (var record in topics.Value)
            {
                var data = JsonSerializer.Deserialize<OrderDeliveredEventV1>(record.Value);

                if (data is null)
                {
                    continue;
                }

                await handler.Handle(new OrderDeliveredEvent(data.OrderIdentifier));
            }
        }
    }
}