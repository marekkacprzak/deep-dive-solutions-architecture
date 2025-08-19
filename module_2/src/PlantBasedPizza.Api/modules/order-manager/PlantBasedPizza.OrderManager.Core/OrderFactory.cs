using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.OrderManager.Core;

public class OrderFactory(ILogger<OrderFactory> logger)
    : IOrderFactory
{
    public async Task<Order> CreateAsync(OrderType type, string customerIdentifier,
        DeliveryDetails? deliveryDetails = null, string correlationId = "")
    {
        Guard.AgainstNullOrEmpty(customerIdentifier, nameof(customerIdentifier));

        if (type == OrderType.Delivery && deliveryDetails == null)
            throw new ArgumentException("If order type is delivery a delivery address must be specified",
                nameof(deliveryDetails));

        logger.LogInformation($"Creating a new order with type {type}");
        Activity.Current?.AddTag("order.type", type.ToString());

        var orderIdentifier = Guid.NewGuid().ToString();

        var order = new Order(orderIdentifier, type, customerIdentifier, deliveryDetails);

        Activity.Current?.AddTag("order.id", orderIdentifier);
        Activity.Current?.AddTag("customer.id", customerIdentifier);

        return order;
    }
}