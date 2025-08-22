



using Dapr.Client;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class DaprEventPublisher(DaprClient daprClient) : OrderEventPublisher
{
    private const string EVENT_SOURCE_HEADER_KEY = "cloudevent.source";
    private const string EVENT_TYPE_HEADER_KEY = "cloudevent.type";
    private const string EVENT_ID_HEADER_KEY = "cloudevent.id";
    private const string EVENT_TIME_HEADER_KEY = "cloudevent.time";
    private const string SOURCE = "orders";
    private const string PUB_SUB_NAME = "public";
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

    public async Task Publish(OrderCompletedEvent evt)
    {
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EVENT_TYPE_HEADER_KEY, "order.orderCompleted.v1" },
            { EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() },
            { EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DATE_FORMAT) }
        };

        await daprClient.PublishEventAsync(PUB_SUB_NAME, "order.orderCompleted.v1", evt, eventMetadata);
    }
}